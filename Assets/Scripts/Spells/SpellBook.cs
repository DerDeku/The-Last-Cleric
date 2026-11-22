using System.Collections.Generic;
using UnityEngine;

public static class SpellBook
{
    public static SpellData Equipped(int slot)
    {
        if (!SaveGame.HasLoadout)
            return Default(slot);

        string id = SaveGame.LoadoutId(slot);
        if (string.IsNullOrEmpty(id))
            return null;

        SpellData spell = Find(id);
        if (spell == null || !IsKnown(spell))
            return null;
        return spell;
    }

    public static SpellData Default(int slot)
    {
        PrepareRoster roster = Load();
        if (roster == null || roster.spells == null || slot < 1 || slot > roster.spells.Length)
            return null;
        return roster.spells[slot - 1];
    }

    public static bool IsKnown(SpellData spell)
    {
        if (spell == null)
            return false;

        PrepareRoster roster = Load();
        if (roster == null)
            return false;

        if (roster.spells != null)
        {
            foreach (SpellData starter in roster.spells)
            {
                if (starter == spell || Same(starter, spell))
                    return true;
            }
        }

        SkillTree tree = roster.cleric != null ? roster.cleric.tree : null;
        if (tree == null || tree.nodes == null)
            return false;

        foreach (SkillNode node in tree.nodes)
        {
            if (node == null || node.spell == null || !Same(node.spell, spell))
                continue;
            if (SkillRules.IsUnlocked(tree, node))
                return true;
        }

        return false;
    }

    public static List<SpellData> Known(PrepareRoster roster)
    {
        var list = new List<SpellData>();
        if (roster == null)
            return list;

        if (roster.spells != null)
        {
            foreach (SpellData spell in roster.spells)
                Add(list, spell);
        }

        SkillTree tree = roster.cleric != null ? roster.cleric.tree : null;
        if (tree != null && tree.nodes != null)
        {
            foreach (SkillNode node in tree.nodes)
            {
                if (node != null && node.spell != null && SkillRules.IsUnlocked(tree, node))
                    Add(list, node.spell);
            }
        }

        return list;
    }

    public static void Equip(int slot, SpellData spell)
    {
        if (spell == null || !IsKnown(spell) || !Profile.IsSpellSlotEnabled(slot))
            return;

        if (!SaveGame.HasLoadout)
        {
            for (int i = 1; i <= 4; i++)
                SaveGame.SetLoadout(i, Default(i) != null ? Default(i).Id : "");
        }

        SaveGame.SetLoadout(slot, spell.Id);
    }

    public static string Describe(SpellData spell)
    {
        if (spell == null)
            return "";

        if (spell.kind == SpellKind.ChargedHeal)
            return Loc.Format("spell.charge.desc", Num(spell.maxHold), spell.power);

        if (spell.kind == SpellKind.TriageHeal)
        {
            int below = Mathf.RoundToInt(spell.bonusHealth * 100f);
            int boosted = spell.power + Mathf.RoundToInt(spell.power * spell.bonusBelow);
            return Loc.Format("spell.triage.desc", spell.power, below, boosted);
        }

        return Loc.Text("spell." + spell.kind);
    }

    public static SpellData Find(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        PrepareRoster roster = Load();
        if (roster == null)
            return null;

        if (roster.spells != null)
        {
            foreach (SpellData spell in roster.spells)
            {
                if (spell != null && spell.Id == id)
                    return spell;
            }
        }

        SkillTree tree = roster.cleric != null ? roster.cleric.tree : null;
        if (tree == null || tree.nodes == null)
            return null;

        foreach (SkillNode node in tree.nodes)
        {
            if (node != null && node.spell != null && node.spell.Id == id)
                return node.spell;
        }

        return null;
    }

    static void Add(List<SpellData> list, SpellData spell)
    {
        if (spell == null)
            return;
        foreach (SpellData have in list)
        {
            if (Same(have, spell))
                return;
        }

        list.Add(spell);
    }

    static bool Same(SpellData a, SpellData b)
    {
        return a != null && b != null && a.Id == b.Id;
    }

    static PrepareRoster Load()
    {
        return Resources.Load<PrepareRoster>("Menu/Roster");
    }

    static string Num(float value)
    {
        if (Mathf.Abs(value - Mathf.Round(value)) < 0.001f)
            return Mathf.RoundToInt(value).ToString();
        return value.ToString("0.##");
    }
}
