#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

public static class DebugMenu
{
    public static void Draw(PrepareRoster roster)
    {
        var panel = new Rect(280f, 24f, 340f, Screen.height - 48f);
        GUI.color = new Color(0.12f, 0.05f, 0.05f, 0.96f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(panel.x + 12f, panel.y + 12f, panel.width - 24f, panel.height - 24f));
        GUILayout.Label("Debug", MenuText.Title());
        DrawWaves();
        GUILayout.Space(10f);
        string gold = SaveGame.InfiniteGold ? "Gold normal" : "Gold unendlich";
        if (GUILayout.Button(gold, GUILayout.Height(32f)))
            SaveGame.SetInfiniteGold(!SaveGame.InfiniteGold);
        GUILayout.Space(10f);
        if (GUILayout.Button("Kleriker leeren", GUILayout.Height(32f)))
            Clear(roster != null ? roster.cleric : null);
        if (GUILayout.Button("Helden leeren", GUILayout.Height(32f)))
            ClearHeroes(roster);
        if (GUILayout.Button("Kleriker alles frei", GUILayout.Height(32f)))
            Unlock(roster != null ? roster.cleric : null);
        if (GUILayout.Button("Helden alles frei", GUILayout.Height(32f)))
            UnlockHeroes(roster);
        GUILayout.EndArea();
    }

    static void DrawWaves()
    {
        WaveManager manager = Object.FindAnyObjectByType<WaveManager>();
        WaveTable table = manager != null ? manager.WaveTable : null;
        int count = CampaignProgress.Count(table);
        int selected = CampaignProgress.Selected(table);
        Color previous = GUI.backgroundColor;
        for (int i = 0; i < count; i++)
        {
            int chapter;
            int wave;
            CampaignProgress.Locate(table, i, out chapter, out wave);
            GUI.backgroundColor = i == selected ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button("Kapitel " + chapter + " - Welle " + wave, GUILayout.Height(26f)))
                CampaignProgress.Jump(table, i);
        }

        GUI.backgroundColor = previous;
        if (count > 0 && GUILayout.Button("Alle Wellen frei", GUILayout.Height(28f)))
            CampaignProgress.OpenAll(table);
    }

    static void ClearHeroes(PrepareRoster roster)
    {
        if (roster == null || roster.heroes == null)
            return;
        foreach (RosterEntry entry in roster.heroes)
            Clear(entry);
    }

    static void UnlockHeroes(PrepareRoster roster)
    {
        if (roster == null || roster.heroes == null)
            return;
        foreach (RosterEntry entry in roster.heroes)
            Unlock(entry);
    }

    static void Clear(RosterEntry entry)
    {
        if (entry == null || entry.tree == null || string.IsNullOrEmpty(entry.tree.id))
            return;
        SaveGame.ClearTree(entry.tree.id);
    }

    static void Unlock(RosterEntry entry)
    {
        if (entry == null || entry.tree == null || entry.tree.nodes == null)
            return;
        foreach (SkillNode node in entry.tree.nodes)
        {
            if (node != null && !string.IsNullOrEmpty(node.id))
                SaveGame.SetNodeRank(entry.tree.id, node.id, SkillRules.MaxStacks(node));
        }
    }
}
#endif
