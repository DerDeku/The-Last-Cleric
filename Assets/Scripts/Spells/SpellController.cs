using UnityEngine;

[DefaultExecutionOrder(10)]
public class SpellController : MonoBehaviour
{
    public static SpellController Instance { get; private set; }

    [SerializeField] SpellData periodicHeal;
    [SerializeField] SpellData burstHeal;
    [SerializeField] SpellData shield;

    readonly float[] cooldownRemaining = new float[4];

    float tickTimer;
    int selectedSlot = 1;
    bool castShieldWhenReady;
    SpellData casting;
    int castingSlot;
    float castTimer;
    AudioSource audio;

    public int SelectedSlot => selectedSlot;
    public bool ShieldArmed => castShieldWhenReady;
    public Vector2 WorldCursor { get; private set; }

    public float CastFill
    {
        get
        {
            if (casting == null || casting.castTime <= 0f)
                return 0f;
            return Mathf.Clamp01(castTimer / casting.castTime);
        }
    }

    public SpellData Selected => GetSlot(selectedSlot);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExists()
    {
        if (FindAnyObjectByType<SpellController>() != null)
            return;

        GameManager host = FindAnyObjectByType<GameManager>();
        GameObject body = host != null ? host.gameObject : new GameObject("Game");
        if (host == null)
            body.AddComponent<GameManager>();
        if (body.GetComponent<InputManager>() == null)
            body.AddComponent<InputManager>();
        body.AddComponent<SpellController>();
        if (body.GetComponent<Hotbar>() == null)
            body.AddComponent<Hotbar>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        LoadMissingAssets();
        audio = gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;
        audio.volume = 0.32f;
        SpellSounds.Bind(audio);
    }

    void Update()
    {
        UpdateWorldCursor();
        TickCooldowns();
        ReadHotkey();
        CastQueuedShield();
        TickCast();
        TickPeriodicHeal();
    }

    public SpellData GetSlot(int slot)
    {
        switch (slot)
        {
            case 1: return periodicHeal;
            case 2: return burstHeal;
            case 3: return shield;
            default: return null;
        }
    }

    public float CooldownRemaining(int slot)
    {
        if (slot < 1 || slot > cooldownRemaining.Length)
            return 0f;
        return cooldownRemaining[slot - 1];
    }

    public float IndicatorFill()
    {
        SpellData spell = Selected;
        if (spell == null)
            return 0f;

        if (spell.kind == SpellKind.PeriodicHeal)
        {
            if (spell.tickRate <= 0f)
                return 0f;
            return Mathf.Clamp01(tickTimer / spell.tickRate);
        }

        if (spell.cooldown <= 0f)
            return 0f;

        float remaining = CooldownRemaining(selectedSlot);
        return 1f - Mathf.Clamp01(remaining / spell.cooldown);
    }

    void ReadHotkey()
    {
        if (InputManager.Instance == null)
            return;

        int hotkey = InputManager.Instance.PressedHotkey;
        if (hotkey < 1 || hotkey > 4)
            return;

        selectedSlot = hotkey;
        castShieldWhenReady = hotkey == 3 && CooldownRemaining(3) > 0f;

        if (hotkey == 1 || hotkey == 4 || CooldownRemaining(hotkey) > 0f)
        {
            CancelCast();
            return;
        }

        BeginCast(hotkey);
    }

    void CastQueuedShield()
    {
        if (!castShieldWhenReady || selectedSlot != 3 || CooldownRemaining(3) > 0f)
            return;

        castShieldWhenReady = false;
        BeginCast(3);
    }

    void BeginCast(int slot)
    {
        SpellData spell = GetSlot(slot);
        if (spell == null || spell.kind == SpellKind.PeriodicHeal || CooldownRemaining(slot) > 0f)
            return;

        if (spell.castTime <= 0f)
        {
            FinishCast(spell, slot);
            return;
        }

        casting = spell;
        castingSlot = slot;
        castTimer = 0f;
    }

    void TickCast()
    {
        if (casting == null)
            return;

        castTimer += Time.deltaTime;
        if (castTimer < casting.castTime)
            return;

        SpellData spell = casting;
        int slot = castingSlot;
        casting = null;
        FinishCast(spell, slot);
    }

    void CancelCast()
    {
        casting = null;
        castTimer = 0f;
    }

    void FinishCast(SpellData spell, int slot)
    {
        Apply(spell);
        cooldownRemaining[slot - 1] = Mathf.Max(0f, spell.cooldown);
    }

    void TickPeriodicHeal()
    {
        SpellData spell = Selected;
        if (spell == null || spell.kind != SpellKind.PeriodicHeal)
            return;

        float interval = Mathf.Max(0.05f, spell.tickRate);
        tickTimer += Time.deltaTime;
        if (tickTimer < interval)
            return;

        tickTimer = 0f;
        Apply(spell);
    }

    void TickCooldowns()
    {
        for (int i = 0; i < cooldownRemaining.Length; i++)
        {
            if (cooldownRemaining[i] > 0f)
                cooldownRemaining[i] = Mathf.Max(0f, cooldownRemaining[i] - Time.deltaTime);
        }
    }

    void Apply(SpellData spell)
    {
        if (spell.kind == SpellKind.Shield)
            HealEffect.ShieldFriends(WorldCursor, spell.radius, spell.power);
        else
            HealEffect.HealFriends(WorldCursor, spell.radius, spell.power);

        SpellSounds.Play(spell.kind);
    }

    void UpdateWorldCursor()
    {
        Camera camera = Camera.main;
        var pointer = UnityEngine.InputSystem.Mouse.current;
        if (camera == null || pointer == null)
            return;

        Vector2 screen = pointer.position.ReadValue();
        float depth = Mathf.Abs(camera.transform.position.z);
        Vector3 world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        world.z = 0f;
        WorldCursor = world;
    }

    void LoadMissingAssets()
    {
#if UNITY_EDITOR
        if (periodicHeal == null)
            periodicHeal = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellData>("Assets/Data/Spells/Periodic Heal.asset");
        if (burstHeal == null)
            burstHeal = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellData>("Assets/Data/Spells/Burst Heal.asset");
        if (shield == null)
            shield = UnityEditor.AssetDatabase.LoadAssetAtPath<SpellData>("Assets/Data/Spells/Shield.asset");
#endif
    }
}
