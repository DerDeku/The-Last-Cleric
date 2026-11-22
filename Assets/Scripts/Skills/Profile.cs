using UnityEngine;

public enum HeroState
{
    Locked,
    Unlocked,
    Active
}

public static class Profile
{
    const int StartingPoints = 2;

    public static bool IsEventDone(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return true;
        return PlayerPrefs.GetInt(EventKey(eventId), 0) == 1;
    }

    public static void CompleteEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return;
        PlayerPrefs.SetInt(EventKey(eventId), 1);
        PlayerPrefs.Save();
    }

    public static bool IsNodeUnlocked(string treeId, string nodeId)
    {
        return PlayerPrefs.GetInt(NodeKey(treeId, nodeId), 0) == 1;
    }

    public static int Points(string treeId)
    {
        string key = PointsKey(treeId);
        if (!PlayerPrefs.HasKey(key))
            return StartingPoints;
        return PlayerPrefs.GetInt(key, 0);
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
        string key = HeroKey(entry.id);
        if (!PlayerPrefs.HasKey(key))
            return entry.startsActive;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    static void SetHeroActive(RosterEntry entry, bool active)
    {
        PlayerPrefs.SetInt(HeroKey(entry.id), active ? 1 : 0);
        PlayerPrefs.Save();
    }

    static string HeroKey(string id)
    {
        return "profile.hero.active." + id;
    }

    public static bool TrySpend(string treeId, string nodeId, int cost)
    {
        int points = Points(treeId);
        if (points < cost || IsNodeUnlocked(treeId, nodeId))
            return false;

        PlayerPrefs.SetInt(PointsKey(treeId), points - Mathf.Max(0, cost));
        PlayerPrefs.SetInt(NodeKey(treeId, nodeId), 1);
        PlayerPrefs.Save();
        return true;
    }

    static string EventKey(string eventId)
    {
        return "profile.event." + eventId;
    }

    static string NodeKey(string treeId, string nodeId)
    {
        return "profile.node." + treeId + "." + nodeId;
    }

    static string PointsKey(string treeId)
    {
        return "profile.points." + treeId;
    }
}
