using UnityEngine;

public static class SkillText
{
    public static string Describe(SkillTree tree, SkillNode node, UnitDefinition definition, string headline = null)
    {
        if (node == null)
            return "";

        if (node.spell != null)
            return Join(SpellBook.Describe(node.spell), Flavor(node));

        if (node.root && definition != null)
            return Join(string.IsNullOrEmpty(headline) ? definition.displayName : headline, Flavor(node), Summary(tree, definition));

        if (node.effect == SkillEffectKind.None)
            return Flavor(node);

        string change = definition != null ? Range(tree, node, definition) : Loc.Format(AddKey(node.effect), Signed(Applied(node)));
        return Join(change, Flavor(node));
    }

    public static string Mark(SkillNode node)
    {
        if (node == null || node.effect == SkillEffectKind.None)
            return "";
        return Loc.Text("skill.mark." + node.effect);
    }

    static string Summary(SkillTree tree, UnitDefinition definition)
    {
        return Stat("skill.stat.health", definition, tree, SkillEffectKind.Health) + "\n"
            + Stat("skill.stat.damage", definition, tree, SkillEffectKind.Damage) + "\n"
            + Stat("skill.stat.speed", definition, tree, SkillEffectKind.MoveSpeed) + "\n"
            + Stat("skill.stat.range", definition, tree, SkillEffectKind.Range) + "\n"
            + Stat("skill.stat.interval", definition, tree, SkillEffectKind.Interval);
    }

    static string Stat(string key, UnitDefinition definition, SkillTree tree, SkillEffectKind kind)
    {
        float bonus = UnlockedBonus(tree, kind);
        float total = Base(definition, kind) + bonus;
        string line = Loc.Format(key, Num(total));
        if (Mathf.Abs(bonus) > 0.001f)
            line += " (" + Signed(bonus) + ")";
        return line;
    }

    static float UnlockedBonus(SkillTree tree, SkillEffectKind kind)
    {
        if (tree == null || tree.nodes == null || kind == SkillEffectKind.None)
            return 0f;

        float sum = 0f;
        foreach (SkillNode node in tree.nodes)
        {
            if (node == null || node.effect != kind || !SkillRules.IsUnlocked(tree, node))
                continue;
            sum += Applied(node);
        }

        return sum;
    }

    static float Base(UnitDefinition definition, SkillEffectKind kind)
    {
        if (definition == null)
            return 0f;

        switch (kind)
        {
            case SkillEffectKind.Damage: return definition.contactDamage;
            case SkillEffectKind.MoveSpeed: return definition.moveSpeed;
            case SkillEffectKind.Range: return definition.attackRange;
            case SkillEffectKind.Health: return definition.maxHealth;
            case SkillEffectKind.Interval: return definition.attackInterval;
            default: return 0f;
        }
    }

    static string Range(SkillTree tree, SkillNode node, UnitDefinition definition)
    {
        float applied = Applied(node);
        float current = Base(definition, node.effect) + UnlockedBonus(tree, node.effect);
        bool owned = SkillRules.IsUnlocked(tree, node);
        float before = owned ? current - applied : current;
        float after = owned ? current : current + applied;
        return Loc.Format(EffectKey(node.effect), Num(before), Num(after));
    }

    static float Applied(SkillNode node)
    {
        if (node.effect == SkillEffectKind.Damage || node.effect == SkillEffectKind.Health)
            return Mathf.RoundToInt(node.amount);
        return node.amount;
    }

    static string Flavor(SkillNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.bodyKey))
            return "";
        return Loc.Text(node.bodyKey);
    }

    static string Join(params string[] parts)
    {
        string text = "";
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;
            if (text.Length > 0)
                text += "\n";
            text += part;
        }
        return text;
    }

    static string EffectKey(SkillEffectKind kind)
    {
        switch (kind)
        {
            case SkillEffectKind.Damage: return "skill.effect.damage";
            case SkillEffectKind.MoveSpeed: return "skill.effect.speed";
            case SkillEffectKind.Range: return "skill.effect.range";
            case SkillEffectKind.Health: return "skill.effect.health";
            case SkillEffectKind.Interval: return "skill.effect.interval";
            default: return "prepare.empty";
        }
    }

    static string AddKey(SkillEffectKind kind)
    {
        switch (kind)
        {
            case SkillEffectKind.Damage: return "skill.effect.damage.add";
            case SkillEffectKind.MoveSpeed: return "skill.effect.speed.add";
            case SkillEffectKind.Range: return "skill.effect.range.add";
            case SkillEffectKind.Health: return "skill.effect.health.add";
            case SkillEffectKind.Interval: return "skill.effect.interval.add";
            default: return "prepare.empty";
        }
    }

    static string Num(float value)
    {
        if (Mathf.Abs(value - Mathf.Round(value)) < 0.001f)
            return Mathf.RoundToInt(value).ToString();
        return value.ToString("0.##");
    }

    static string Signed(float value)
    {
        string number = Num(value);
        return value > 0f ? "+" + number : number;
    }
}
