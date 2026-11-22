using UnityEngine;

public enum HeroState
{
    Locked,
    Unlocked,
    Active
}

public static class Profile
{
    public static bool IsEventDone(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return true;
        return SaveGame.HasEvent(eventId);
    }

    public static void CompleteEvent(string eventId)
    {
        SaveGame.AddEvent(eventId);
    }

    public static bool IsNodeUnlocked(string treeId, string nodeId)
    {
        return SaveGame.HasNode(treeId, nodeId);
    }

    public static bool IsSpellSlotEnabled(int slot)
    {
        PrepareRoster roster = Resources.Load<PrepareRoster>("Menu/Roster");
        if (roster == null || slot != roster.lockedSlot)
            return true;
        if (roster.cleric == null || roster.cleric.tree == null || string.IsNullOrEmpty(roster.lockedNode))
            return false;
        return IsNodeUnlocked(roster.cleric.tree.id, roster.lockedNode);
    }

    public static HeroState HeroStateOf(RosterEntry entry)
    {
        if (entry == null || !IsEventDone(entry.unlockEvent))
            return HeroState.Locked;
        return IsHeroActive(entry) ? HeroState.Active : HeroState.Unlocked;
    }

    public static int ActiveCount(PrepareRoster roster)
    {
        if (roster == null || roster.heroes == null)
            return 0;

        int count = 0;
        foreach (RosterEntry entry in roster.heroes)
        {
            if (HeroStateOf(entry) == HeroState.Active)
                count++;
        }

        return count;
    }

    public static bool TryActivate(PrepareRoster roster, RosterEntry entry)
    {
        if (roster == null || HeroStateOf(entry) != HeroState.Unlocked)
            return false;
        if (ActiveCount(roster) >= Mathf.Max(1, roster.partySize))
            return false;

        SetHeroActive(entry, true);
        return true;
    }

    public static void Deactivate(RosterEntry entry)
    {
        if (HeroStateOf(entry) != HeroState.Active)
            return;
        SetHeroActive(entry, false);
    }

    public static void BenchForEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return;

        PrepareRoster roster = Resources.Load<PrepareRoster>("Menu/Roster");
        if (roster == null || roster.heroes == null)
            return;

        foreach (RosterEntry entry in roster.heroes)
        {
            if (entry != null && entry.unlockEvent == eventId)
                SaveGame.SetHero(entry.id, false);
        }
    }

    public static void PlaceParty(PrepareRoster roster)
    {
        Hero[] heroes = Object.FindObjectsByType<Hero>();
        foreach (Hero hero in heroes)
        {
            if (hero == null || hero.Definition == null || roster == null || roster.heroes == null)
                continue;

            RosterEntry match = null;
            foreach (RosterEntry entry in roster.heroes)
            {
                if (entry != null && entry.unit == hero.Definition)
                    match = entry;
            }

            if (match == null)
                continue;

            hero.SetDeployed(HeroStateOf(match) == HeroState.Active);
        }
    }

    static bool IsHeroActive(RosterEntry entry)
    {
        bool active;
        if (!SaveGame.TryHero(entry.id, out active))
            return entry.startsActive;
        return active;
    }

    static void SetHeroActive(RosterEntry entry, bool active)
    {
        SaveGame.SetHero(entry.id, active);
    }

    public static bool TrySpend(string treeId, string nodeId, int cost)
    {
        if (IsNodeUnlocked(treeId, nodeId) || !SaveGame.TrySpendGold(cost))
            return false;

        SaveGame.AddNode(treeId, nodeId);
        return true;
    }
}
