using UnityEngine;

public class Hotbar : MonoBehaviour
{
    GUIStyle labelStyle;

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        SpellController spells = SpellController.Instance;
        if (spells == null)
            return;

        EnsureStyle();
        const float slotWidth = 120f;
        const float gap = 8f;
        float total = 4f * slotWidth + 3f * gap;
        float x = (Screen.width - total) * 0.5f;
        float y = Screen.height - 78f;

        for (int i = 0; i < 4; i++)
        {
            int slot = i + 1;
            var rect = new Rect(x + i * (slotWidth + gap), y, slotWidth, 62f);
            bool locked = !Profile.IsSpellSlotEnabled(slot);
            bool selected = spells.SelectedSlot == slot && !locked;
            if (locked)
                GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.55f);
            else if (selected)
                GUI.color = new Color(0.95f, 0.82f, 0.25f, 0.95f);
            else
                GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.82f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            SpellData spell = spells.GetSlot(slot);
            string name = Loc.Text("hotbar.empty");
            if (locked)
                name = Loc.Text("hotbar.locked");
            else if (spell != null)
                name = Loc.Text("spell." + spell.kind);

            float cooldown = spells.CooldownRemaining(slot);
            string timer = "";
            if (!locked && spell != null)
            {
                timer = cooldown > 0.05f ? cooldown.ToString("0.0") : Loc.Text("hotbar.ready");
                if (spells.IsWaiting(slot))
                    timer = Loc.Text("hotbar.waiting");
                else if (spells.IsQueued(slot))
                    timer = Loc.Text("hotbar.queue");
            }

            GUI.color = locked ? new Color(0.65f, 0.65f, 0.65f, 1f) : Color.white;
            GUI.Label(rect, slot + "\n" + name + "\n" + timer, labelStyle);
        }

        GUI.color = Color.white;
    }

    void EnsureStyle()
    {
        if (labelStyle != null)
            return;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };
        labelStyle.normal.textColor = Color.white;
    }
}
