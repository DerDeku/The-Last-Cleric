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
    float baseAttackInterval;
    float bonusAttackInterval;
    UnitTeam team = UnitTeam.Friendly;

    public UnitDefinition Definition => definition;
    public UnitTeam Team => team;
    public int MaxHealth => Mathf.Max(1, baseMaxHealth + bonusMaxHealth);
    public float MoveSpeed => Mathf.Max(0f, baseMoveSpeed + bonusMoveSpeed);
    public int ContactDamage => Mathf.Max(0, baseContactDamage + bonusContactDamage);
    public float AttackRange => Mathf.Max(0f, baseAttackRange + bonusAttackRange);
    public float AttackInterval => Mathf.Max(0.05f, baseAttackInterval + bonusAttackInterval);
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

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
        baseAttackInterval = Mathf.Max(0.05f, source.attackInterval);
        team = source.team;
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

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        Health = Mathf.Min(MaxHealth, Health + amount);
    }

    public void Damage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        Health = Mathf.Max(0, Health - amount);
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
    }
}
