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
                int rank = SkillRules.Rank(entry.tree, node);
                if (node == null || node.effect == SkillEffectKind.None || rank <= 0)
                    continue;
                Apply(hero, node, rank);
            }
        }
    }

    public static void Apply(Unit unit, SkillNode node, int rank)
    {
        if (unit == null || node == null || rank <= 0)
            return;

        float amount = node.amount * rank;
        switch (node.effect)
        {
            case SkillEffectKind.Damage:
                unit.AddBonusContactDamage(Mathf.RoundToInt(amount));
                break;
            case SkillEffectKind.MoveSpeed:
                unit.AddBonusMoveSpeed(amount);
                break;
            case SkillEffectKind.Range:
                unit.AddBonusAttackRange(amount);
                break;
            case SkillEffectKind.Health:
                unit.AddBonusMaxHealth(Mathf.RoundToInt(amount));
                break;
            case SkillEffectKind.Interval:
                unit.AddBonusAttackInterval(amount);
                break;
        }
    }
}
