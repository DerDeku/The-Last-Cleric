using UnityEngine;

public class PrepareScreen : IMenuScreen
{
    int section;
    int heroIndex;
    string selectedNode;
    int equipSlot = 1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    bool debugOpen;
#endif

    public void Draw(MenuHost host)
    {
        PrepareRoster roster = host.Roster;
        GUILayout.BeginArea(new Rect(24f, 20f, 240f, Screen.height - 40f));
        DrawWaves();
        GUILayout.Space(8f);
        GUILayout.Label(Loc.Format("prepare.gold", SaveGame.GoldText), MenuText.Body());
        GUILayout.Space(12f);
        if (GUILayout.Button(Loc.Text("prepare.cleric"), GUILayout.Height(40f)))
            section = 1;
        if (GUILayout.Button(Loc.Text("prepare.heroes"), GUILayout.Height(40f)))
            section = 0;
        GUILayout.FlexibleSpace();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (GUILayout.Button(debugOpen ? "Debug schließen" : "Debug", GUILayout.Height(28f)))
            debugOpen = !debugOpen;
#endif
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (debugOpen)
            DebugMenu.Draw(roster);
#endif
    }

    static void DrawWaves()
    {
        WaveManager manager = Object.FindAnyObjectByType<WaveManager>();
        WaveTable table = manager != null ? manager.WaveTable : null;
        int count = CampaignProgress.ChapterCount(table);
        if (count == 0)
            return;

        int selected = CampaignProgress.SelectedChapter(table);
        Color previous = GUI.backgroundColor;
        for (int i = 0; i < count; i++)
        {
            if (!CampaignProgress.ChapterOpen(table, i))
                continue;

            GUI.backgroundColor = i == selected ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button(Loc.Format("prepare.wave", i + 1, 1), GUILayout.Height(28f)))
                CampaignProgress.SelectChapter(table, i);
        }

        GUI.backgroundColor = previous;
    }

    void DrawHeroes(PrepareRoster roster)
    {
        var list = new Rect(280f, 28f, 160f, Screen.height - 56f);
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

        string heroName = selected != null && !string.IsNullOrEmpty(selected.nameKey) ? Loc.Text(selected.nameKey) : null;
        DrawTree(new Rect(460f, 28f, Screen.width - 490f, Screen.height - 120f), selected != null ? selected.tree : null, selected != null ? selected.unit : null, heroName);
        var action = new Rect(460f, Screen.height - 72f, 220f, 40f);
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
        RosterEntry cleric = roster != null ? roster.cleric : null;
        SkillTree tree = cleric != null ? cleric.tree : null;
        DrawTree(new Rect(280f, 28f, Screen.width - 540f, Screen.height - 56f), tree, cleric != null ? cleric.unit : null, Loc.Text("prepare.cleric"));

        GUILayout.BeginArea(new Rect(Screen.width - 250f, 28f, 220f, Screen.height - 56f));
        GUILayout.Label(Loc.Text("prepare.spells"), MenuText.Body());
        Color previous = GUI.backgroundColor;
        for (int slot = 1; slot <= 4; slot++)
        {
            bool open = Profile.IsSpellSlotEnabled(slot);
            SpellData equipped = open ? SpellBook.Equipped(slot) : null;
            string name = !open ? Loc.Text("hotbar.locked") : equipped == null ? Loc.Text("hotbar.empty") : Loc.Text("spell." + equipped.kind);
            GUI.backgroundColor = slot == equipSlot ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button(slot + "  " + name, GUILayout.Height(32f)) && open)
                equipSlot = slot;
        }

        GUI.backgroundColor = previous;
        GUILayout.Space(12f);
        foreach (SpellData spell in SpellBook.Known(roster))
        {
            if (GUILayout.Button(Loc.Text("spell." + spell.kind), GUILayout.Height(32f)))
                SpellBook.Equip(equipSlot, spell);
        }

        GUILayout.EndArea();
    }

    void DrawTree(Rect panel, SkillTree tree, UnitDefinition definition, string headline)
    {
        GUI.Box(panel, GUIContent.none);
        if (tree == null || tree.nodes == null)
        {
            GUI.Label(new Rect(panel.x + 16f, panel.y + 16f, panel.width - 32f, 32f), Loc.Text("prepare.empty"), MenuText.Body());
            return;
        }

        DrawEdges(panel, tree);
        SkillNode selected = SkillRules.Find(tree, selectedNode);
        if (selected == null || !SkillRules.IsVisible(tree, selected))
            selected = FindRoot(tree);
        SkillNode hovered = null;
        foreach (SkillNode node in tree.nodes)
        {
            if (!SkillRules.IsVisible(tree, node))
                continue;

            Rect rect = NodeRect(panel, node);
            bool unlocked = SkillRules.IsUnlocked(tree, node);
            GUI.color = unlocked ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
            string title = Loc.Text(node.titleKey);
            if (!unlocked && node.cost > 0)
                title += " (" + node.cost + ")";
            if (GUI.Button(rect, title))
                selectedNode = node.id;
            string mark = SkillText.Mark(node);
            if (mark.Length > 0)
                GUI.Label(new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, 16f), mark, MenuText.Mark());
            GUI.color = Color.white;
            if (rect.Contains(Event.current.mousePosition))
                hovered = node;
        }

        if (selected != null && SkillRules.IsVisible(tree, selected))
        {
            float textY = panel.yMax - 156f;
            GUI.Label(new Rect(panel.x + 16f, textY + 22f, panel.width - 180f, 126f), SkillText.Describe(tree, selected, definition, headline), MenuText.Wrap());
            var buy = new Rect(panel.xMax - 168f, textY + 28f, 148f, 36f);
            if (SkillRules.CanBuy(tree, selected) && GUI.Button(buy, Loc.Text("prepare.unlock")))
                Profile.TrySpend(tree.id, selected.id, selected.cost);
        }

        if (hovered != null && Event.current.type == EventType.Repaint)
            DrawPopup(hovered, tree, definition, headline);
    }

    static SkillNode FindRoot(SkillTree tree)
    {
        if (tree == null || tree.nodes == null)
            return null;

        foreach (SkillNode node in tree.nodes)
        {
            if (node != null && node.root)
                return node;
        }

        return null;
    }

    static void DrawPopup(SkillNode node, SkillTree tree, UnitDefinition definition, string headline)
    {
        string text = Loc.Text(node.titleKey) + "\n" + SkillText.Describe(tree, node, definition, headline);
        var style = MenuText.Wrap();
        float width = 280f;
        float height = style.CalcHeight(new GUIContent(text), width - 16f) + 16f;
        Vector2 mouse = Event.current.mousePosition;
        var box = new Rect(mouse.x + 18f, mouse.y + 18f, width, height);
        if (box.xMax > Screen.width)
            box.x = mouse.x - width - 12f;
        if (box.yMax > Screen.height)
            box.y = Screen.height - height - 8f;

        GUI.color = new Color(0.08f, 0.09f, 0.1f, 0.96f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(box.x + 8f, box.y + 8f, width - 16f, height - 12f), text, style);
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
        float y = Mathf.Lerp(panel.y + 50f, panel.yMax - 190f, Mathf.Clamp01(node.position.y));
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
