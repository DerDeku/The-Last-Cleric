using System.Collections.Generic;
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
    readonly List<int> chain = new List<int>();
    int armedSlot;
    SpellData casting;
    int castingSlot;
    float castTimer;
    bool holding;
    bool waitRelease;
    float holdTimer;
    SpellData holdSpell;
    AudioSource audio;

    public int SelectedSlot => selectedSlot;
    public bool IsWaiting(int slot) => armedSlot == slot && CooldownRemaining(slot) > 0f;
    public bool IsQueued(int slot) => chain.Contains(slot);
    public Vector2 WorldCursor { get; private set; }

    public float CastFill
    {
        get
        {
                if (holding && holdSpell != null && holdSpell.maxHold > 0f)
                return Mathf.Clamp01(holdTimer / holdSpell.maxHold);
            if (casting == null || casting.castTime <= 0f)
                return 0f;
            return Mathf.Clamp01(castTimer / casting.castTime);
        }
    }

    public int PreviewHeal(Unit unit)
    {
        if (!holding || holdSpell == null || unit == null || holdSpell.maxHold <= 0f)
            return 0;

        if (unit.Team != UnitTeam.Friendly || !unit.IsAlive || !unit.IsDeployed)
            return 0;

        int amount = Mathf.RoundToInt(holdSpell.power * Mathf.Clamp01(holdTimer / holdSpell.maxHold));
        if (amount <= 0 || !HealEffect.Hits(WorldCursor, holdSpell.radius, unit))
            return 0;
        return amount;
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
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            return;

        UpdateWorldCursor();
        TickCooldowns();
        ReadHotkey();
        TickCast();
        TickHold();
        PromoteChain();
        TickPeriodicHeal();
    }

    public SpellData GetSlot(int slot)
    {
        SpellData equipped = SpellBook.Equipped(slot);
        if (equipped != null || SaveGame.HasLoadout)
            return equipped;

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

        if (!Profile.IsSpellSlotEnabled(hotkey))
            return;

        SpellData spell = GetSlot(hotkey);
        if (spell != null && spell.kind == SpellKind.PeriodicHeal)
        {
            selectedSlot = hotkey;
            chain.Clear();
            armedSlot = 0;
            CancelHold();
            CancelCast();
            return;
        }

        if (spell != null && spell.kind == SpellKind.ChargedHeal)
        {
            selectedSlot = hotkey;
            chain.Clear();
            armedSlot = 0;
            CancelCast();
            return;
        }

        if (spell == null)
        {
            if (hotkey == 4 && casting == null && chain.Count == 0 && !holding)
                selectedSlot = 4;
            return;
        }

        selectedSlot = hotkey;
        if (casting != null && castingSlot == hotkey)
            return;

        if (CooldownRemaining(hotkey) > 0f)
        {
            armedSlot = hotkey;
            return;
        }

        if (armedSlot == hotkey)
            armedSlot = 0;
        if (chain.Contains(hotkey))
            return;

        bool busy = casting != null || chain.Count > 0;
        if (GameSettings.ChainCasting && busy)
        {
            chain.Add(hotkey);
            return;
        }

        CancelCast();
        chain.Clear();
        BeginCast(hotkey);
    }

    void PromoteChain()
    {
        if (casting != null)
            return;

        while (chain.Count > 0)
        {
            int slot = chain[0];
            chain.RemoveAt(0);
            if (CooldownRemaining(slot) > 0f)
                continue;
            BeginCast(slot);
            return;
        }

        if (armedSlot == 0 || CooldownRemaining(armedSlot) > 0f)
            return;

        int armed = armedSlot;
        armedSlot = 0;
        BeginCast(armed);
    }

    void BeginCast(int slot)
    {
        SpellData spell = GetSlot(slot);
        if (spell == null || spell.kind == SpellKind.PeriodicHeal || spell.kind == SpellKind.ChargedHeal || CooldownRemaining(slot) > 0f)
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

    public void ResetBattle()
    {
        for (int i = 0; i < cooldownRemaining.Length; i++)
            cooldownRemaining[i] = 0f;
        tickTimer = 0f;
        selectedSlot = 1;
        chain.Clear();
        armedSlot = 0;
        CancelCast();
        CancelHold();
    }

    void CancelHold()
    {
        holding = false;
        holdTimer = 0f;
        holdSpell = null;
        waitRelease = false;
    }

    void TickHold()
    {
        if (InputManager.Instance == null)
            return;

        if (waitRelease)
        {
            if (!InputManager.Instance.IsHeld(selectedSlot))
                waitRelease = false;
            return;
        }

        SpellData spell = Selected;
        bool down = spell != null
            && spell.kind == SpellKind.ChargedHeal
            && InputManager.Instance.IsHeld(selectedSlot)
            && CooldownRemaining(selectedSlot) <= 0f
            && casting == null;

        if (!down)
        {
            if (holding)
                ReleaseHold(holdTimer >= 0.2f);
            return;
        }

        holding = true;
        holdSpell = spell;
        float max = Mathf.Max(0.05f, spell.maxHold);
        holdTimer = Mathf.Min(max, holdTimer + Time.deltaTime);
        if (holdTimer >= max - 0.0001f)
            ReleaseHold(true);
    }

    void ReleaseHold(bool cast)
    {
        SpellData spell = holdSpell;
        float held = holdTimer;
        int slot = selectedSlot;
        CancelHold();
        waitRelease = true;
        if (!cast || spell == null)
            return;

        float max = Mathf.Max(0.05f, spell.maxHold);
        int amount = Mathf.RoundToInt(spell.power * Mathf.Clamp01(held / max));
        if (amount <= 0)
            return;

        HealEffect.Prophecy(WorldCursor, spell.radius, amount, "spell." + spell.kind);
        SpellSounds.Play(spell.kind);
        cooldownRemaining[slot - 1] = Mathf.Max(0f, spell.cooldown);
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
        string source = "spell." + spell.kind;
        if (spell.kind == SpellKind.Shield)
            HealEffect.ShieldFriends(WorldCursor, spell.radius, spell.power, source);
        else if (spell.kind == SpellKind.TriageHeal)
            HealEffect.HealFriends(WorldCursor, spell.radius, unit => TriageAmount(spell, unit), source);
        else
            HealEffect.HealFriends(WorldCursor, spell.radius, spell.power, source);

        SpellSounds.Play(spell.kind);
    }

    static int TriageAmount(SpellData spell, Unit unit)
    {
        int amount = spell.power;
        if (unit != null && unit.MaxHealth > 0 && (float)unit.Health / unit.MaxHealth < spell.bonusHealth)
            amount += Mathf.RoundToInt(spell.power * spell.bonusBelow);
        return amount;
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
