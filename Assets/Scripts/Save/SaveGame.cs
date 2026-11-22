using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SavePair
{
    public string id;
    public int value;
}

[Serializable]
public class SaveFile
{
    public int gold;
    public bool infiniteGold;
    public int cleared;
    public int selected;
    public string[] events = new string[0];
    public string[] nodes = new string[0];
    public string[] loadout = new string[0];
    public SavePair[] points = new SavePair[0];
    public SavePair[] heroes = new SavePair[0];
}

public static class SaveGame
{
    public const int SlotCount = 3;

    public static int ActiveSlot { get; private set; } = -1;
    public static SaveFile Current { get; private set; }

    public static bool InfiniteGold => Current != null && Current.infiniteGold;
    public static int Gold => InfiniteGold ? int.MaxValue : (Current != null ? Current.gold : 0);
    public static string GoldText => InfiniteGold ? "∞" : (Current != null ? Current.gold : 0).ToString();
    public static int Cleared => Current != null ? Mathf.Max(0, Current.cleared) : 0;

    public static int SelectedWave(int waveCount)
    {
        int last = Mathf.Max(0, waveCount - 1);
        int picked = Current != null ? Current.selected : 0;
        return Mathf.Clamp(picked, 0, Mathf.Min(Cleared, last));
    }

    public static void SetSelected(int index)
    {
        if (Current == null)
            return;
        Current.selected = Mathf.Max(0, index);
        Save();
    }

    public static bool HasLoadout => Current != null && Current.loadout != null && Current.loadout.Length == 4;

    public static string LoadoutId(int slot)
    {
        if (!HasLoadout || slot < 1 || slot > Current.loadout.Length)
            return null;
        return Current.loadout[slot - 1];
    }

    public static void SetLoadout(int slot, string spellId)
    {
        if (Current == null || slot < 1 || slot > 4)
            return;

        if (Current.loadout == null || Current.loadout.Length != 4)
        {
            var next = new string[4];
            if (Current.loadout != null)
            {
                for (int i = 0; i < Current.loadout.Length && i < 4; i++)
                    next[i] = Current.loadout[i];
            }

            Current.loadout = next;
        }

        Current.loadout[slot - 1] = spellId ?? "";
        Save();
    }

    public static void SetProgress(int cleared, int selected)
    {
        if (Current == null)
            return;
        Current.cleared = Mathf.Max(0, cleared);
        Current.selected = Mathf.Max(0, selected);
        Save();
    }

    public static void MigrateLegacy()
    {
        if (PlayerPrefs.GetInt("saves.migrated", 0) == 1)
            return;

        if (!AnySlot())
        {
            SaveFile imported = ReadLegacy();
            if (imported != null)
                Write(1, imported);
        }

        PlayerPrefs.SetInt("saves.migrated", 1);
        PlayerPrefs.Save();
    }

    public static bool Exists(int slot)
    {
        return File.Exists(PathFor(slot));
    }

    public static int GoldIn(int slot)
    {
        if (!Exists(slot))
            return 0;
        SaveFile file = Read(slot);
        if (file != null && file.infiniteGold)
            return int.MaxValue;
        return file != null ? file.gold : 0;
    }

    public static string GoldTextIn(int slot)
    {
        if (!Exists(slot))
            return "0";
        SaveFile file = Read(slot);
        if (file != null && file.infiniteGold)
            return "∞";
        return file != null ? file.gold.ToString() : "0";
    }

    public static void LoadOrCreate(int slot)
    {
        slot = Mathf.Clamp(slot, 1, SlotCount);
        Current = Exists(slot) ? Read(slot) : new SaveFile();
        ActiveSlot = slot;
        Write(slot, Current);
    }

    public static void Delete(int slot)
    {
        string path = PathFor(slot);
        if (File.Exists(path))
            File.Delete(path);
        if (ActiveSlot == slot)
        {
            ActiveSlot = -1;
            Current = null;
        }
    }

    public static void Save()
    {
        if (ActiveSlot < 1 || Current == null)
            return;
        Write(ActiveSlot, Current);
    }

    public static void SetInfiniteGold(bool enabled)
    {
        if (Current == null)
            return;
        Current.infiniteGold = enabled;
        Save();
    }

    public static void AddGold(int amount)
    {
        if (Current == null || amount <= 0 || Current.infiniteGold)
            return;
        Current.gold += amount;
        Save();
    }

    public static bool TrySpendGold(int amount)
    {
        if (Current == null || amount < 0)
            return false;
        if (Current.infiniteGold)
            return true;
        if (Current.gold < amount)
            return false;
        Current.gold -= amount;
        Save();
        return true;
    }

    public static bool HasEvent(string id)
    {
        return Contains(Current != null ? Current.events : null, id);
    }

    public static void AddEvent(string id)
    {
        if (Current == null || string.IsNullOrEmpty(id) || HasEvent(id))
            return;
        Current.events = Append(Current.events, id);
        Save();
    }

    public static void RemoveEvent(string id)
    {
        if (Current == null || string.IsNullOrEmpty(id) || Current.events == null)
            return;
        Current.events = Without(Current.events, id, false);
        Save();
    }

    public static void ClearTree(string treeId)
    {
        if (Current == null || string.IsNullOrEmpty(treeId) || Current.nodes == null)
            return;
        Current.nodes = Without(Current.nodes, treeId + "/", true);
        Save();
    }

    public static bool HasNode(string treeId, string nodeId)
    {
        return NodeRank(treeId, nodeId) > 0;
    }

    public static int NodeRank(string treeId, string nodeId)
    {
        if (Current == null || Current.nodes == null)
            return 0;

        string id = NodeId(treeId, nodeId);
        string mark = id + "*";
        foreach (string item in Current.nodes)
        {
            if (item == id)
                return 1;
            if (item == null || !item.StartsWith(mark))
                continue;
            int rank;
            if (int.TryParse(item.Substring(mark.Length), out rank))
                return Mathf.Max(0, rank);
        }

        return 0;
    }

    public static void SetNodeRank(string treeId, string nodeId, int rank)
    {
        if (Current == null || string.IsNullOrEmpty(treeId) || string.IsNullOrEmpty(nodeId))
            return;

        string id = NodeId(treeId, nodeId);
        Current.nodes = Without(Current.nodes, id, true);
        if (rank == 1)
            Current.nodes = Append(Current.nodes, id);
        else if (rank > 1)
            Current.nodes = Append(Current.nodes, id + "*" + rank);
        Save();
    }

    public static void AddNode(string treeId, string nodeId)
    {
        if (Current == null || HasNode(treeId, nodeId))
            return;
        SetNodeRank(treeId, nodeId, 1);
    }

    public static bool TryPoints(string treeId, out int points)
    {
        points = 0;
        SavePair pair = Find(Current != null ? Current.points : null, treeId);
        if (pair == null)
            return false;
        points = pair.value;
        return true;
    }

    public static void SetPoints(string treeId, int points)
    {
        if (Current == null)
            return;
        Current.points = SetPair(Current.points, treeId, points);
        Save();
    }

    public static bool TryHero(string id, out bool active)
    {
        active = false;
        SavePair pair = Find(Current != null ? Current.heroes : null, id);
        if (pair == null)
            return false;
        active = pair.value == 1;
        return true;
    }

    public static void SetHero(string id, bool active)
    {
        if (Current == null)
            return;
        Current.heroes = SetPair(Current.heroes, id, active ? 1 : 0);
        Save();
    }

    static bool AnySlot()
    {
        for (int i = 1; i <= SlotCount; i++)
        {
            if (Exists(i))
                return true;
        }

        return false;
    }

    static SaveFile ReadLegacy()
    {
        var file = new SaveFile();
        bool any = false;
        string[] trees = { "fighter", "ranger", "cleric" };
        string[] nodes = { "guard", "step", "reach", "oath", "aim", "ward" };
        foreach (string tree in trees)
        {
            if (PlayerPrefs.HasKey("profile.points." + tree))
            {
                file.points = SetPair(file.points, tree, PlayerPrefs.GetInt("profile.points." + tree));
                any = true;
            }

            foreach (string node in nodes)
            {
                if (PlayerPrefs.GetInt("profile.node." + tree + "." + node, 0) != 1)
                    continue;
                file.nodes = Append(file.nodes, NodeId(tree, node));
                any = true;
            }
        }

        string[] heroes = { "fighter", "ranger" };
        foreach (string hero in heroes)
        {
            string key = "profile.hero.active." + hero;
            if (!PlayerPrefs.HasKey(key))
                continue;
            file.heroes = SetPair(file.heroes, hero, PlayerPrefs.GetInt(key));
            any = true;
        }

        if (PlayerPrefs.GetInt("profile.event.demo.oath", 0) == 1)
        {
            file.events = Append(file.events, "demo.oath");
            any = true;
        }

        int coins = PlayerPrefs.GetInt("meta.coins", 0);
        if (coins > 0)
        {
            file.gold = coins;
            any = true;
        }

        return any ? file : null;
    }

    static SaveFile Read(int slot)
    {
        try
        {
            SaveFile file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(PathFor(slot)));
            return file ?? new SaveFile();
        }
        catch (Exception)
        {
            return new SaveFile();
        }
    }

    static void Write(int slot, SaveFile file)
    {
        string path = PathFor(slot);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(file, true));
    }

    static string PathFor(int slot)
    {
        return Path.Combine(Application.persistentDataPath, "saves", "slot-" + slot + ".json");
    }

    static string NodeId(string treeId, string nodeId)
    {
        return treeId + "/" + nodeId;
    }

    static bool Contains(string[] list, string id)
    {
        if (list == null || string.IsNullOrEmpty(id))
            return false;
        foreach (string item in list)
        {
            if (item == id)
                return true;
        }

        return false;
    }

    static string[] Without(string[] list, string token, bool prefix)
    {
        if (list == null)
            return new string[0];

        var kept = new System.Collections.Generic.List<string>();
        foreach (string item in list)
        {
            if (string.IsNullOrEmpty(item))
                continue;
            bool drop = prefix ? item.StartsWith(token) : item == token;
            if (!drop)
                kept.Add(item);
        }

        return kept.ToArray();
    }

    static string[] Append(string[] list, string id)
    {
        int length = list != null ? list.Length : 0;
        var next = new string[length + 1];
        if (list != null)
            Array.Copy(list, next, length);
        next[length] = id;
        return next;
    }

    static SavePair Find(SavePair[] list, string id)
    {
        if (list == null)
            return null;
        foreach (SavePair pair in list)
        {
            if (pair != null && pair.id == id)
                return pair;
        }

        return null;
    }

    static SavePair[] SetPair(SavePair[] list, string id, int value)
    {
        SavePair found = Find(list, id);
        if (found != null)
        {
            found.value = value;
            return list;
        }

        int length = list != null ? list.Length : 0;
        var next = new SavePair[length + 1];
        if (list != null)
            Array.Copy(list, next, length);
        next[length] = new SavePair { id = id, value = value };
        return next;
    }
}
