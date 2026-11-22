using UnityEngine;

public static class PartySpawner
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Ensure()
    {
        PrepareRoster roster = Resources.Load<PrepareRoster>("Menu/Roster");
        if (roster != null && roster.heroes != null)
        {
            foreach (RosterEntry entry in roster.heroes)
            {
                if (entry == null || entry.unit == null || HasHero(entry.unit))
                    continue;

                var body = new GameObject(string.IsNullOrEmpty(entry.unit.displayName) ? entry.id : entry.unit.displayName);
                body.transform.position = new Vector3(entry.home.x, entry.home.y, 0f);
                Hero hero = body.AddComponent<Hero>();
                hero.ApplyDefinition(entry.unit);
            }
        }

        ClericAvatar.EnsureBehindRanger();
    }

    static bool HasHero(UnitDefinition definition)
    {
        foreach (Hero hero in Object.FindObjectsByType<Hero>())
        {
            if (hero != null && hero.Definition == definition)
                return true;
        }

        return false;
    }
}
