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
        int shown = 0;
        for (int slot = 1; slot <= 4; slot++)
        {
            if (Profile.IsSpellSlotEnabled(slot))
                shown++;
        }

        if (shown == 0)
            return;

        float total = shown * slotWidth + (shown - 1) * gap;
        float x = (Screen.width - total) * 0.5f;
        float y = Screen.height - 78f;
        int drawn = 0;
        for (int slot = 1; slot <= 4; slot++)
        {
            if (!Profile.IsSpellSlotEnabled(slot))
                continue;

            var rect = new Rect(x + drawn * (slotWidth + gap), y, slotWidth, 62f);
            drawn++;
            bool selected = spells.SelectedSlot == slot;
            GUI.color = selected
                ? new Color(0.95f, 0.82f, 0.25f, 0.95f)
                : new Color(0.12f, 0.12f, 0.12f, 0.82f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            SpellData spell = spells.GetSlot(slot);
            string name = spell != null ? Loc.Text("spell." + spell.kind) : Loc.Text("hotbar.empty");
            string timer = "";
            if (spell != null)
            {
                float cooldown = spells.CooldownRemaining(slot);
                timer = cooldown > 0.05f ? cooldown.ToString("0.0") : Loc.Text("hotbar.ready");
                if (spells.IsWaiting(slot))
                    timer = Loc.Text("hotbar.waiting");
                else if (spells.IsQueued(slot))
                    timer = Loc.Text("hotbar.queue");
            }

            GUI.color = Color.white;
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
