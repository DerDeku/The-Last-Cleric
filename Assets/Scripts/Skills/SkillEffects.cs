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
                if (node == null || node.effect == SkillEffectKind.None || !SkillRules.IsUnlocked(entry.tree, node))
                    continue;
                Apply(hero, node);
            }
        }
    }

    public static void Apply(Unit unit, SkillNode node)
    {
        if (unit == null || node == null)
            return;

        switch (node.effect)
        {
            case SkillEffectKind.Damage:
                unit.AddBonusContactDamage(Mathf.RoundToInt(node.amount));
                break;
            case SkillEffectKind.MoveSpeed:
                unit.AddBonusMoveSpeed(node.amount);
                break;
            case SkillEffectKind.Range:
                unit.AddBonusAttackRange(node.amount);
                break;
            case SkillEffectKind.Health:
                unit.AddBonusMaxHealth(Mathf.RoundToInt(node.amount));
                break;
            case SkillEffectKind.Interval:
                unit.AddBonusAttackInterval(node.amount);
                break;
        }
    }
}
