using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class BattleLog
{
    public struct Row
    {
        public string Name;
        public string Source;
        public int Amount;
    }

    static readonly List<Row> kills = new List<Row>();
    static readonly List<Row> damage = new List<Row>();
    static readonly List<Row> heals = new List<Row>();
    static readonly List<Row> shields = new List<Row>();
    static readonly List<Row> unlocks = new List<Row>();

    public static float Time { get; private set; }
    public static int Gold { get; private set; }
    public static IReadOnlyList<Row> Kills => kills;
    public static IReadOnlyList<Row> Damage => damage;
    public static IReadOnlyList<Row> Heals => heals;
    public static IReadOnlyList<Row> Shields => shields;
    public static IReadOnlyList<Row> Unlocks => unlocks;

    public static void Reset()
    {
        Time = 0f;
        Gold = 0;
        kills.Clear();
        damage.Clear();
        heals.Clear();
        shields.Clear();
        unlocks.Clear();
    }

    public static void Tick(float step)
    {
        if (step > 0f)
            Time += step;
    }

    public static void AddKill(string name)
    {
        Add(kills, string.IsNullOrEmpty(name) ? "?" : name, "", 1);
    }

    public static void AddGold(int amount)
    {
        if (amount > 0)
            Gold += amount;
    }

    public static void AddDamage(string who, string source, int amount)
    {
        if (amount <= 0)
            return;
        Add(damage, who, string.IsNullOrEmpty(source) ? "fight.auto" : source, amount);
    }

    public static void AddUnlock(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;
        Add(unlocks, key, "", 1);
    }

    public static void AddHeal(string source, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(source))
            return;
        Add(heals, source, "", amount);
    }

    public static void AddShield(string source, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(source))
            return;
        Add(shields, source, "", amount);
    }

    public static void Order()
    {
        kills.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.Ordinal));
        damage.Sort((a, b) => b.Amount.CompareTo(a.Amount));
        heals.Sort((a, b) => b.Amount.CompareTo(a.Amount));
        shields.Sort((a, b) => b.Amount.CompareTo(a.Amount));
    }

    public static bool DismissPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;
        Mouse mouse = Mouse.current;
        return mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame);
    }

    public static string Stopwatch()
    {
        int whole = Mathf.Max(0, Mathf.FloorToInt(Time));
        return (whole / 60) + ":" + (whole % 60).ToString("00");
    }

    static void Add(List<Row> rows, string name, string source, int amount)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            Row row = rows[i];
            if (row.Name != name || row.Source != source)
                continue;
            row.Amount += amount;
            rows[i] = row;
            return;
        }

        rows.Add(new Row { Name = name, Source = source, Amount = amount });
    }
}
