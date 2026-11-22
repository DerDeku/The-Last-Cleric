using UnityEngine;

public class PrepareScreen : IMenuScreen
{
    int section;
    int heroIndex;
    string selectedNode;

    public void Draw(MenuHost host)
    {
        PrepareRoster roster = host.Roster;
        GUILayout.BeginArea(new Rect(24f, 20f, 220f, Screen.height - 40f));
        GUILayout.Label(Loc.Text("prepare.title"), MenuText.Title());
        GUILayout.Space(16f);
        if (GUILayout.Button(Loc.Text("prepare.heroes"), GUILayout.Height(40f)))
            section = 0;
        if (GUILayout.Button(Loc.Text("prepare.cleric"), GUILayout.Height(40f)))
            section = 1;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(Loc.Text("menu.back"), GUILayout.Height(36f)))
            host.ShowMain();
        bool canStart = Profile.ActiveCount(roster) > 0;
        if (canStart && GUILayout.Button(Loc.Text("prepare.start"), GUILayout.Height(42f)))
            host.BeginBattle();
        GUILayout.EndArea();

        if (section == 0)
            DrawHeroes(roster);
        else
            DrawCleric(roster);
    }

    void DrawHeroes(PrepareRoster roster)
    {
        var list = new Rect(250f, 28f, 160f, Screen.height - 56f);
        GUILayout.BeginArea(list);
        int partySize = roster != null ? Mathf.Max(1, roster.partySize) : 1;
        GUILayout.Label(Loc.Format("prepare.party", Profile.ActiveCount(roster), partySize), MenuText.Body());
        RosterEntry[] heroes = roster != null ? roster.heroes : null;
        if (heroes != null)
        {
            for (int i = 0; i < heroes.Length; i++)
            {
                RosterEntry entry = heroes[i];
                string label = entry == null || string.IsNullOrEmpty(entry.nameKey) ? Loc.Text("prepare.empty") : Loc.Text(entry.nameKey);
                label += "\n" + Loc.Text(StateKey(Profile.HeroStateOf(entry)));
                if (GUILayout.Button(label, GUILayout.Height(64f)))
                {
                    heroIndex = i;
                    selectedNode = null;
                }
            }
        }
        GUILayout.EndArea();

        RosterEntry selected = heroes != null && heroIndex >= 0 && heroIndex < heroes.Length ? heroes[heroIndex] : null;
        HeroState state = Profile.HeroStateOf(selected);
        if (state == HeroState.Locked)
        {
            GUI.Label(new Rect(430f, 40f, 420f, 32f), Loc.Text("hero.state.locked"), MenuText.Body());
            return;
        }

        DrawTree(new Rect(430f, 28f, Screen.width - 460f, Screen.height - 120f), selected != null ? selected.tree : null);
        var action = new Rect(430f, Screen.height - 72f, 220f, 40f);
        if (state == HeroState.Active && GUI.Button(action, Loc.Text("prepare.bench")))
            Profile.Deactivate(selected);
        if (state == HeroState.Unlocked && Profile.ActiveCount(roster) < partySize && GUI.Button(action, Loc.Text("prepare.bring")))
            Profile.TryActivate(roster, selected);
    }

    static string StateKey(HeroState state)
    {
        if (state == HeroState.Active)
            return "hero.state.active";
        if (state == HeroState.Unlocked)
            return "hero.state.unlocked";
        return "hero.state.locked";
    }

    void DrawCleric(PrepareRoster roster)
    {
        SkillTree tree = roster != null && roster.cleric != null ? roster.cleric.tree : null;
        DrawTree(new Rect(260f, 28f, Screen.width - 520f, Screen.height - 56f), tree);

        GUILayout.BeginArea(new Rect(Screen.width - 240f, 28f, 210f, Screen.height - 56f));
        GUILayout.Label(Loc.Text("prepare.spells"), MenuText.Body());
        SpellData[] spells = roster != null ? roster.spells : null;
        if (spells != null)
        {
            foreach (SpellData spell in spells)
            {
                if (spell == null)
                    continue;
                GUILayout.Label(Loc.Text("spell." + spell.kind), MenuText.Body());
            }
        }
        GUILayout.EndArea();
    }

    void DrawTree(Rect panel, SkillTree tree)
    {
        GUI.Box(panel, GUIContent.none);
        if (tree == null || tree.nodes == null)
        {
            GUI.Label(new Rect(panel.x + 16f, panel.y + 16f, panel.width - 32f, 32f), Loc.Text("prepare.empty"), MenuText.Body());
            return;
        }

        DrawEdges(panel, tree);
        SkillNode selected = SkillRules.Find(tree, selectedNode);
        foreach (SkillNode node in tree.nodes)
        {
            if (!SkillRules.IsVisible(tree, node))
                continue;

            Rect rect = NodeRect(panel, node);
            bool unlocked = SkillRules.IsUnlocked(tree, node);
            GUI.color = unlocked ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
            if (GUI.Button(rect, Loc.Text(node.titleKey)))
                selectedNode = node.id;
            GUI.color = Color.white;
        }

        if (selected == null || !SkillRules.IsVisible(tree, selected))
            return;

        float textY = panel.yMax - 110f;
        GUI.Label(new Rect(panel.x + 16f, textY, panel.width - 32f, 24f), Loc.Format("prepare.points", Profile.Points(tree.id)), MenuText.Body());
        GUI.Label(new Rect(panel.x + 16f, textY + 24f, panel.width - 180f, 48f), Loc.Text(selected.bodyKey), MenuText.Body());
        if (SkillRules.CanBuy(tree, selected) && GUI.Button(new Rect(panel.xMax - 150f, textY + 28f, 130f, 36f), Loc.Text("prepare.unlock")))
            Profile.TrySpend(tree.id, selected.id, selected.cost);
    }

    static void DrawEdges(Rect panel, SkillTree tree)
    {
        foreach (SkillNode node in tree.nodes)
        {
            if (node == null || node.links == null || !SkillRules.IsVisible(tree, node))
                continue;

            foreach (string link in node.links)
            {
                SkillNode other = SkillRules.Find(tree, link);
                if (other == null || !SkillRules.IsVisible(tree, other) || string.Compare(node.id, other.id) > 0)
                    continue;

                Vector2 from = NodeCenter(panel, node);
                Vector2 to = NodeCenter(panel, other);
                DrawLine(from, to);
            }
        }
    }

    static Rect NodeRect(Rect panel, SkillNode node)
    {
        Vector2 center = NodeCenter(panel, node);
        return new Rect(center.x - 46f, center.y - 28f, 92f, 56f);
    }

    static Vector2 NodeCenter(Rect panel, SkillNode node)
    {
        float x = Mathf.Lerp(panel.x + 70f, panel.xMax - 70f, Mathf.Clamp01(node.position.x));
        float y = Mathf.Lerp(panel.y + 50f, panel.yMax - 150f, Mathf.Clamp01(node.position.y));
        return new Vector2(x, y);
    }

    static void DrawLine(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        float length = delta.magnitude;
        if (length < 1f)
            return;

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, from);
        GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        GUI.DrawTexture(new Rect(from.x, from.y - 1.5f, length, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.matrix = matrix;
    }
}
