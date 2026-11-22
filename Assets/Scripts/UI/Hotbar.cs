using UnityEngine;

public class Hotbar : MonoBehaviour
{
    GUIStyle labelStyle;

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
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
            bool selected = spells.SelectedSlot == slot;
            GUI.color = selected ? new Color(0.95f, 0.82f, 0.25f, 0.95f) : new Color(0.12f, 0.12f, 0.12f, 0.82f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            SpellData spell = spells.GetSlot(slot);
            string name = spell == null ? "Leer" : spell.spellName;
            float cooldown = spells.CooldownRemaining(slot);
            bool waiting = slot == 3 && spells.ShieldArmed && cooldown > 0.05f;
            string timer = cooldown > 0.05f ? cooldown.ToString("0.0") : "bereit";
            if (waiting)
                timer = "wartet";
            if (spell == null)
                timer = "";

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
