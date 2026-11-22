using UnityEngine;

public static class SkillEffects
{
    public static void ApplyRoster(PrepareRoster roster)
    {
        if (roster == null || roster.heroes == null)
            return;

        Hero[] heroes = Object.FindObjectsByType<Hero>();
        foreach (RosterEntry entry in roster.heroes)
            ApplyEntry(entry, heroes);
    }

    static void ApplyEntry(RosterEntry entry, Hero[] heroes)
    {
        if (entry == null || entry.unit == null || entry.tree == null || entry.tree.nodes == null)
            return;

        foreach (Hero hero in heroes)
        {
            if (hero == null || !hero.IsDeployed || hero.Definition != entry.unit)
                continue;

            foreach (SkillNode node in entry.tree.nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.effectId) || !SkillRules.IsUnlocked(entry.tree, node))
                    continue;
                Apply(hero, node.effectId);
            }
        }
    }

    public static void Apply(Unit unit, string effectId)
    {
        if (unit == null || string.IsNullOrEmpty(effectId))
            return;

        switch (effectId)
        {
            case "fighter.guard":
                unit.AddBonusContactDamage(2);
                break;
            case "fighter.step":
                unit.AddBonusMoveSpeed(0.25f);
                break;
            case "fighter.reach":
                unit.AddBonusAttackRange(0.15f);
                break;
            case "ranger.aim":
                unit.AddBonusAttackRange(0.5f);
                break;
        }
    }
}
