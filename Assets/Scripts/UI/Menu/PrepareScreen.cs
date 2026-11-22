using System.Collections.Generic;
using UnityEngine;

public class PrepareScreen : IMenuScreen
{
    struct SpellPick
    {
        public SpellData spell;
        public Rect rect;
        public int slot;
    }

    int section;
    int heroIndex;
    string selectedNode;
    int clericPage;
    string dragSpellId;
    int dragFromSlot;
    Vector2 dragOrigin;
    bool dragMoved;
    bool spellBarFull;
    readonly List<SpellPick> picks = new List<SpellPick>();
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
        DrawDragGhost();
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
        var panel = new Rect(280f, 28f, Screen.width - 304f, Screen.height - 56f);
        Color previous = GUI.backgroundColor;
        GUI.backgroundColor = clericPage == 0 ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
        if (GUI.Button(new Rect(panel.x, panel.y, 150f, 34f), Loc.Text("prepare.tree")))
            clericPage = 0;
        GUI.backgroundColor = clericPage == 1 ? new Color(0.95f, 0.82f, 0.35f, 1f) : Color.white;
        if (GUI.Button(new Rect(panel.x + 158f, panel.y, 150f, 34f), Loc.Text("prepare.spells")))
            clericPage = 1;
        GUI.backgroundColor = previous;

        var body = new Rect(panel.x, panel.y + 44f, panel.width, panel.height - 44f);
        if (clericPage == 0)
        {
            RosterEntry cleric = roster != null ? roster.cleric : null;
            SkillTree tree = cleric != null ? cleric.tree : null;
            DrawTree(body, tree, cleric != null ? cleric.unit : null, Loc.Text("prepare.cleric"));
            return;
        }

        DrawSpellPage(body, roster);
    }

    void DrawSpellPage(Rect body, PrepareRoster roster)
    {
        LayoutSpells(body, roster);
        HandleSpellPointer();
        GUI.Box(body, GUIContent.none);

        GUI.Label(new Rect(body.x + 16f, body.y + 10f, body.width - 32f, 24f), Loc.Text("spell.bar"), MenuText.Body());
        foreach (SpellPick pick in picks)
        {
            if (pick.slot <= 0)
                continue;
            DrawSpellCard(pick.rect, pick.spell, pick.slot.ToString(), true);
        }

        float hintY = body.y + 132f;
        string hint = spellBarFull ? Loc.Text("spell.full") : Loc.Text("spell.hint");
        GUI.Label(new Rect(body.x + 16f, hintY, body.width - 32f, 40f), hint, MenuText.Small());
        GUI.Label(new Rect(body.x + 16f, hintY + 42f, body.width - 32f, 24f), Loc.Text("spell.known"), MenuText.Body());
        foreach (SpellPick pick in picks)
        {
            if (pick.slot > 0)
                continue;
            DrawSpellCard(pick.rect, pick.spell, null, false);
        }
    }

    void LayoutSpells(Rect body, PrepareRoster roster)
    {
        picks.Clear();
        int open = 0;
        for (int slot = 1; slot <= 4; slot++)
        {
            if (Profile.IsSpellSlotEnabled(slot))
                open++;
        }

        float slotWidth = open > 0 ? Mathf.Min(220f, (body.width - 32f - (open - 1) * 10f) / open) : 180f;
        const float slotHeight = 78f;
        float x = body.x + 16f;
        float y = body.y + 38f;
        for (int slot = 1; slot <= 4; slot++)
        {
            if (!Profile.IsSpellSlotEnabled(slot))
                continue;

            picks.Add(new SpellPick
            {
                spell = SpellBook.Equipped(slot),
                rect = new Rect(x, y, slotWidth, slotHeight),
                slot = slot
            });
            x += slotWidth + 10f;
        }

        List<SpellData> known = SpellBook.Known(roster);
        int columns = body.width >= 760f ? 2 : 1;
        float cardWidth = (body.width - 32f - (columns - 1) * 12f) / columns;
        const float cardHeight = 108f;
        float cardsY = body.y + 204f;
        for (int i = 0; i < known.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            var rect = new Rect(body.x + 16f + column * (cardWidth + 12f), cardsY + row * (cardHeight + 10f), cardWidth, cardHeight);
            picks.Add(new SpellPick { spell = known[i], rect = rect, slot = 0 });
        }
    }

    void HandleSpellPointer()
    {
        Event pointer = Event.current;
        if (pointer == null)
            return;

        if (pointer.type == EventType.MouseDown && pointer.button == 0)
        {
            SpellPick hit;
            if (!PickAt(pointer.mousePosition, out hit) || hit.spell == null && hit.slot <= 0)
            {
                dragSpellId = null;
                return;
            }

            if (hit.spell == null)
            {
                dragSpellId = null;
                return;
            }

            dragSpellId = hit.spell.Id;
            dragFromSlot = hit.slot;
            dragOrigin = pointer.mousePosition;
            dragMoved = false;
            pointer.Use();
            return;
        }

        if (pointer.type == EventType.MouseDrag && !string.IsNullOrEmpty(dragSpellId))
        {
            if ((pointer.mousePosition - dragOrigin).sqrMagnitude > 36f)
                dragMoved = true;
            pointer.Use();
            return;
        }

        if (pointer.type != EventType.MouseUp || pointer.button != 0 || string.IsNullOrEmpty(dragSpellId))
            return;

        if ((pointer.mousePosition - dragOrigin).sqrMagnitude > 36f)
            dragMoved = true;

        if (dragMoved)
        {
            SpellPick hit;
            if (PickSlot(pointer.mousePosition, out hit))
            {
                SpellBook.Place(dragFromSlot, dragSpellId, hit.slot);
                spellBarFull = false;
            }
            else if (dragFromSlot > 0)
            {
                SpellBook.Clear(dragFromSlot);
                spellBarFull = false;
            }
        }
        else if (dragFromSlot <= 0)
        {
            if (SpellBook.SlotOf(dragSpellId) > 0)
                spellBarFull = false;
            else
            {
                int empty = SpellBook.FirstEmpty();
                if (empty > 0)
                {
                    SpellBook.Place(0, dragSpellId, empty);
                    spellBarFull = false;
                }
                else
                    spellBarFull = true;
            }
        }

        dragSpellId = null;
        dragMoved = false;
        pointer.Use();
    }

    bool PickAt(Vector2 point, out SpellPick hit)
    {
        for (int i = picks.Count - 1; i >= 0; i--)
        {
            if (!picks[i].rect.Contains(point))
                continue;
            hit = picks[i];
            return true;
        }

        hit = default;
        return false;
    }

    bool PickSlot(Vector2 point, out SpellPick hit)
    {
        foreach (SpellPick pick in picks)
        {
            if (pick.slot <= 0 || !pick.rect.Contains(point))
                continue;
            hit = pick;
            return true;
        }

        hit = default;
        return false;
    }

    void DrawSpellCard(Rect rect, SpellData spell, string key, bool bar)
    {
        bool over = rect.Contains(Event.current.mousePosition);
        bool drop = bar && dragMoved && over;
        Color fill = spell != null ? spell.fillColor : new Color(0.45f, 0.48f, 0.5f, 1f);
        fill.a = 1f;
        GUI.color = drop ? new Color(0.95f, 0.82f, 0.35f, 1f) : over ? new Color(0.2f, 0.22f, 0.2f, 1f) : new Color(0.12f, 0.13f, 0.14f, 1f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = fill;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 6f, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string title = spell == null ? Loc.Text("hotbar.empty") : Loc.Text("spell." + spell.kind);
        if (!string.IsNullOrEmpty(key))
            title = key + "   " + title;
        GUI.Label(new Rect(rect.x + 14f, rect.y + 6f, rect.width - 22f, 22f), title, MenuText.Body());
        if (spell == null)
            return;

        if (bar)
        {
            GUI.Label(new Rect(rect.x + 14f, rect.y + 32f, rect.width - 22f, 40f), SpellBook.Facts(spell), MenuText.Small());
            return;
        }

        GUI.Label(new Rect(rect.x + 14f, rect.y + 30f, rect.width - 22f, 48f), SpellBook.Describe(spell), MenuText.Small());
        GUI.Label(new Rect(rect.x + 14f, rect.yMax - 24f, rect.width - 22f, 20f), SpellBook.Facts(spell), MenuText.Small());
    }

    void DrawDragGhost()
    {
        if (!dragMoved || string.IsNullOrEmpty(dragSpellId) || Event.current.type != EventType.Repaint)
            return;

        SpellData spell = SpellBook.Find(dragSpellId);
        if (spell == null)
            return;

        var ghost = new Rect(Event.current.mousePosition.x + 14f, Event.current.mousePosition.y + 10f, 210f, 36f);
        GUI.color = new Color(0.08f, 0.09f, 0.1f, 0.94f);
        GUI.DrawTexture(ghost, Texture2D.whiteTexture);
        Color fill = spell.fillColor;
        fill.a = 1f;
        GUI.color = fill;
        GUI.DrawTexture(new Rect(ghost.x, ghost.y, 6f, ghost.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(ghost.x + 12f, ghost.y + 6f, ghost.width - 18f, 24f), Loc.Text("spell." + spell.kind), MenuText.Body());
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
            int rank = SkillRules.Rank(tree, node);
            int stacks = SkillRules.MaxStacks(node);
            bool stack = stacks > 1;
            bool complete = SkillRules.IsComplete(tree, node);
            if (stack)
            {
                GUI.color = new Color(0.45f, 0.72f, 0.95f, 1f);
                GUI.DrawTexture(new Rect(rect.x - 4f, rect.y - 4f, rect.width + 8f, rect.height + 8f), Texture2D.whiteTexture);
            }

            GUI.color = complete
                ? new Color(0.95f, 0.82f, 0.35f, 1f)
                : stack ? new Color(0.8f, 0.9f, 1f, 1f) : Color.white;
            string title = Loc.Text(node.titleKey);
            int price = SkillRules.NextCost(tree, node);
            if (!complete && price > 0)
                title += " (" + price + ")";
            if (stack && rank > 0)
                title += "\n(" + rank + "/" + stacks + ")";
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
                SkillRules.TryBuy(tree, selected);
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

    static Vector2 NodeCenter(Rect panel, SkillNode node)
    {
        float x = Mathf.Lerp(panel.x + 70f, panel.xMax - 70f, Mathf.Clamp01(node.position.x));
        float y = Mathf.Lerp(panel.y + 50f, panel.yMax - 190f, Mathf.Clamp01(node.position.y));
        return new Vector2(x, y);
    }

    static Rect NodeRect(Rect panel, SkillNode node)
    {
        Vector2 center = NodeCenter(panel, node);
        float height = SkillRules.MaxStacks(node) > 1 ? 68f : 56f;
        return new Rect(center.x - 46f, center.y - height * 0.5f, 92f, height);
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
