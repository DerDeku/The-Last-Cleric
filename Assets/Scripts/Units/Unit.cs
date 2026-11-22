using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] UnitDefinition definition;

    int baseMaxHealth = 1;
    int bonusMaxHealth;
    float baseMoveSpeed;
    float bonusMoveSpeed;
    int baseContactDamage;
    int bonusContactDamage;
    float baseAttackRange;
    float bonusAttackRange;
    float baseAttackHeight;
    float baseAttackInterval;
    float bonusAttackInterval;
    UnitTeam team = UnitTeam.Friendly;
    AttackShape attackShape = AttackShape.Single;
    AreaForm areaForm = AreaForm.Circle;
    AttackDelivery attackDelivery = AttackDelivery.Melee;
    bool previewAttack;
    BattleLine targetLine = BattleLine.Frontline;
    bool holdPosition;
    float attackMoveMultiplier;

    public UnitDefinition Definition => definition;
    public UnitTeam Team => team;
    public AttackShape AttackShape => attackShape;
    public AreaForm AreaForm => areaForm;
    public AttackDelivery AttackDelivery => attackDelivery;
    public bool PreviewAttack => previewAttack;
    public BattleLine TargetLine => targetLine;
    public bool HoldPosition => holdPosition;
    public float AttackMoveMultiplier => Mathf.Clamp01(attackMoveMultiplier);
    public Unit AttackFocus { get; private set; }

    public void SetAttackFocus(Unit unit)
    {
        AttackFocus = unit;
    }
    public int MaxHealth => Mathf.Max(1, baseMaxHealth + bonusMaxHealth);
    public float MoveSpeed => Mathf.Max(0f, baseMoveSpeed + bonusMoveSpeed);
    public int ContactDamage => Mathf.Max(0, baseContactDamage + bonusContactDamage);
    public float AttackRange => Mathf.Max(0f, baseAttackRange + bonusAttackRange);
    public float AttackHeight => baseAttackHeight > 0f ? baseAttackHeight : AttackRange;
    public float AttackInterval => Mathf.Max(0.05f, baseAttackInterval + bonusAttackInterval);
    const float ShieldHold = 5f;
    const float ShieldFade = 2.5f;

    float shieldIdle;
    float shieldDrain;
    int shieldFadeFrom;
    bool shieldFading;

    public int Health { get; private set; }
    public int Shield { get; private set; }
    public bool IsAlive => Health > 0;

    void LateUpdate()
    {
        TickShield();
    }

    protected virtual void Awake()
    {
        if (definition != null)
            ApplyDefinition(definition);
    }

    public void ApplyDefinition(UnitDefinition source)
    {
        definition = source;
        if (source == null)
            return;

        baseMaxHealth = Mathf.Max(1, source.maxHealth);
        baseMoveSpeed = Mathf.Max(0f, source.moveSpeed);
        baseContactDamage = Mathf.Max(0, source.contactDamage);
        baseAttackRange = Mathf.Max(0f, source.attackRange);
        baseAttackHeight = Mathf.Max(0f, source.attackHeight);
        baseAttackInterval = Mathf.Max(0.05f, source.attackInterval);
        team = source.team;
        attackShape = source.attackShape;
        areaForm = source.areaForm;
        attackDelivery = source.attackDelivery;
        previewAttack = source.previewAttack;
        targetLine = source.targetLine;
        holdPosition = source.holdPosition;
        attackMoveMultiplier = Mathf.Clamp01(source.attackMoveMultiplier);
        bonusMaxHealth = 0;
        bonusMoveSpeed = 0f;
        bonusContactDamage = 0;
        bonusAttackRange = 0f;
        bonusAttackInterval = 0f;
        Health = MaxHealth;

        if (!string.IsNullOrEmpty(source.displayName))
            gameObject.name = source.displayName;

        ApplyAppearance(source);
    }

    public void AddBonusMaxHealth(int amount)
    {
        bonusMaxHealth += amount;
        if (amount > 0)
            Health += amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
    }

    public void AddBonusMoveSpeed(float amount)
    {
        bonusMoveSpeed += amount;
    }

    public void AddBonusContactDamage(int amount)
    {
        bonusContactDamage += amount;
    }

    public void AddBonusAttackRange(float amount)
    {
        bonusAttackRange += amount;
    }

    public void AddBonusAttackInterval(float amount)
    {
        bonusAttackInterval += amount;
    }

    public bool CanHit(Unit other)
    {
        if (other == null)
            return false;

        Vector2 offset = (Vector2)other.transform.position - (Vector2)transform.position;
        if (areaForm == AreaForm.Square)
            return Mathf.Abs(offset.x) <= AttackRange && Mathf.Abs(offset.y) <= AttackHeight;

        return offset.sqrMagnitude <= AttackRange * AttackRange;
    }

    public bool IsInAttackPosition(Unit other)
    {
        if (!CanHit(other))
            return false;

        if (attackDelivery == AttackDelivery.Ranged)
            return true;

        return other.CanHit(this);
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        int before = Health;
        Health = Mathf.Min(MaxHealth, Health + amount);
        if (Health > before)
            HealFeedback.Play(this);
    }

    public void AddShield(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        Shield += amount;
        shieldIdle = 0f;
        shieldDrain = 0f;
        shieldFading = false;
        HealFeedback.PlayShield(this);
    }

    void TickShield()
    {
        if (!IsAlive || Shield <= 0)
        {
            shieldIdle = 0f;
            shieldDrain = 0f;
            shieldFading = false;
            return;
        }

        shieldIdle += Time.deltaTime;
        if (shieldIdle < ShieldHold)
            return;

        if (!shieldFading)
        {
            shieldFading = true;
            shieldFadeFrom = Mathf.Max(1, Shield);
            shieldDrain = 0f;
        }

        shieldDrain += shieldFadeFrom * Time.deltaTime / ShieldFade;
        int loss = Mathf.FloorToInt(shieldDrain);
        if (loss <= 0)
            return;

        shieldDrain -= loss;
        Shield = Mathf.Max(0, Shield - loss);
    }

    public void Damage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        bool shieldOnly = false;
        if (Shield > 0)
        {
            int absorbed = Mathf.Min(Shield, amount);
            Shield -= absorbed;
            amount -= absorbed;
            shieldOnly = amount <= 0;
        }

        if (amount > 0)
            Health = Mathf.Max(0, Health - amount);

        HitFeedback.Play(this, shieldOnly);
        if (Health == 0)
            OnDeath();
    }

    protected virtual void OnDeath()
    {
    }

    void ApplyAppearance(UnitDefinition source)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        renderer.sprite = source.shape == UnitShape.Circle ? RuntimeSprites.Circle() : RuntimeSprites.Square();
        renderer.color = source.bodyColor;
        renderer.sortingOrder = 10;
        transform.localScale = Vector3.one * Mathf.Max(0.1f, source.bodySize);

        if (GetComponentInChildren<HealthBar>() == null)
            HealthBar.Attach(this, source.healthBarColor);
        if (GetComponent<UnitAttack>() == null)
            gameObject.AddComponent<UnitAttack>();
        if (source.previewAttack && GetComponentInChildren<AttackPreview>() == null)
            AttackPreview.Attach(this);
    }

    void OnDrawGizmos()
    {
        if (definition == null || !definition.previewAttack)
            return;

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
        float range = definition.attackRange;
        float height = definition.attackHeight > 0f ? definition.attackHeight : range;
        if (definition.areaForm == AreaForm.Square)
            Gizmos.DrawWireCube(transform.position, new Vector3(range * 2f, height * 2f, 0f));
        else
            Gizmos.DrawWireSphere(transform.position, range);
    }
}
