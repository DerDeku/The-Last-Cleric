using System.Collections.Generic;
using UnityEngine;

public static class SpellBook
{
    public static SpellData Equipped(int slot)
    {
        SpellData spell = Default(slot);
        if (SaveGame.HasLoadout)
        {
            string id = SaveGame.LoadoutId(slot);
            spell = string.IsNullOrEmpty(id) ? null : Find(id);
        }

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
                if ((starter == spell || Same(starter, spell)) && !IsLocked(spell))
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
            {
                if (!IsLocked(spell))
                    Add(list, spell);
            }
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

        EnsureLoadout();
        SaveGame.SetLoadout(slot, spell.Id);
    }

    public static void Clear(int slot)
    {
        if (!Profile.IsSpellSlotEnabled(slot))
            return;

        EnsureLoadout();
        SaveGame.SetLoadout(slot, "");
    }

    public static int FirstEmpty()
    {
        for (int slot = 1; slot <= 4; slot++)
        {
            if (Profile.IsSpellSlotEnabled(slot) && Equipped(slot) == null)
                return slot;
        }

        return 0;
    }

    public static int SlotOf(string spellId)
    {
        if (string.IsNullOrEmpty(spellId))
            return 0;

        for (int slot = 1; slot <= 4; slot++)
        {
            SpellData there = Equipped(slot);
            if (there != null && there.Id == spellId)
                return slot;
        }

        return 0;
    }

    public static void Place(int fromSlot, string spellId, int toSlot)
    {
        SpellData spell = Find(spellId);
        if (spell == null || fromSlot == toSlot)
            return;

        SpellData occupant = Equipped(toSlot);
        Equip(toSlot, spell);
        for (int slot = 1; slot <= 4; slot++)
        {
            if (slot == toSlot)
                continue;

            SpellData there = Equipped(slot);
            if (there != null && there.Id == spell.Id)
                Clear(slot);
        }

        if (fromSlot > 0 && occupant != null && occupant.Id != spell.Id && Equipped(fromSlot) == null)
            Equip(fromSlot, occupant);
    }

    public static string Describe(SpellData spell)
    {
        if (spell == null)
            return "";

        if (spell.kind == SpellKind.PeriodicHeal)
            return Loc.Format("spell.periodic.desc", spell.power, Num(spell.tickRate));

        if (spell.kind == SpellKind.BurstHeal)
            return Loc.Format("spell.burst.desc", spell.power);

        if (spell.kind == SpellKind.Shield)
            return Loc.Format("spell.shield.desc", spell.power);

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

    public static string Facts(SpellData spell)
    {
        if (spell == null)
            return "";

        string radius = Loc.Format("spell.radius", Num(spell.radius));
        if (spell.kind == SpellKind.PeriodicHeal)
            return radius + "    " + Loc.Format("spell.every", Num(spell.tickRate));

        if (spell.kind == SpellKind.ChargedHeal)
            return radius + "    " + Loc.Format("spell.hold", Num(spell.maxHold)) + "    " + Loc.Format("spell.cooldown", Num(spell.cooldown));

        string cast = spell.castTime > 0.01f ? Loc.Format("spell.cast", Num(spell.castTime)) + "    " : "";
        string cooldown = spell.cooldown > 0.01f ? Loc.Format("spell.cooldown", Num(spell.cooldown)) : "";
        return radius + "    " + cast + cooldown;
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

    static void EnsureLoadout()
    {
        if (SaveGame.HasLoadout)
            return;

        for (int i = 1; i <= 4; i++)
        {
            SpellData spell = Default(i);
            SaveGame.SetLoadout(i, spell != null ? spell.Id : "");
        }
    }

    static bool IsLocked(SpellData spell)
    {
        return spell != null && spell.Id == "shield" && SaveGame.Cleared < 1 && !SaveGame.HasEvent("wave.1");
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
