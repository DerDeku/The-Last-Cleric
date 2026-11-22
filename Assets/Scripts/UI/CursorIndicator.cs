using UnityEngine;
using UnityEngine.InputSystem;

public class CursorIndicator : MonoBehaviour
{
    [SerializeField] float borderWidth = 4f;
    [SerializeField] float emptyRadius = 28f;
    [SerializeField] Color emptyFill = new Color(1f, 1f, 1f, 0.08f);
    [SerializeField] Color emptyBorder = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] Color chargeColor = new Color(1f, 1f, 1f, 0.4f);

    Texture2D circleTexture;
    Texture2D chargeTexture;
    float builtRadius = -1f;
    float builtBorder = -1f;
    Color builtFill;
    Color builtBorderColor;
    float builtCharge = -1f;

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        var pointer = UnityEngine.InputSystem.Mouse.current;
        if (pointer == null)
            return;

        SpellData spell = SpellController.Instance != null ? SpellController.Instance.Selected : null;
        float pixelRadius = spell == null ? emptyRadius : WorldRadiusToPixels(spell.radius);
        Color fill = spell == null ? emptyFill : spell.fillColor;
        Color border = spell == null ? emptyBorder : spell.borderColor;
        float charge = SpellController.Instance != null ? SpellController.Instance.IndicatorFill() : 0f;

        RebuildCircle(pixelRadius, fill, border);
        RebuildCharge(charge);
        if (circleTexture == null)
            return;

        Vector2 screen = pointer.position.ReadValue();
        float x = screen.x - pixelRadius;
        float y = Screen.height - screen.y - pixelRadius;
        float diameter = pixelRadius * 2f;
        var circleRect = new Rect(x, y, diameter, diameter);
        GUI.DrawTexture(circleRect, circleTexture, ScaleMode.StretchToFill, true);

        if (chargeTexture != null && charge > 0.01f)
            GUI.DrawTexture(circleRect, chargeTexture, ScaleMode.StretchToFill, true);
    }

    void OnDestroy()
    {
        if (circleTexture != null)
            Destroy(circleTexture);
        if (chargeTexture != null)
            Destroy(chargeTexture);
    }

    float WorldRadiusToPixels(float worldRadius)
    {
        Camera camera = Camera.main;
        if (camera == null)
            return emptyRadius;

        Vector2 center = SpellController.Instance != null
            ? SpellController.Instance.WorldCursor
            : Vector2.zero;
        Vector3 edge = camera.WorldToScreenPoint((Vector3)center + Vector3.right * worldRadius);
        Vector3 origin = camera.WorldToScreenPoint(center);
        return Mathf.Max(8f, Vector2.Distance(origin, edge));
    }

    void RebuildCircle(float pixelRadius, Color fill, Color border)
    {
        if (circleTexture != null
            && Mathf.Approximately(builtRadius, pixelRadius)
            && Mathf.Approximately(builtBorder, borderWidth)
            && builtFill == fill
            && builtBorderColor == border)
            return;

        int size = Mathf.Clamp(Mathf.RoundToInt(pixelRadius * 2f), 8, 1024);
        if (circleTexture == null || circleTexture.width != size)
        {
            if (circleTexture != null)
                Destroy(circleTexture);

            circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var pixels = new Color32[size * size];
        float half = (size - 1) * 0.5f;
        float borderPixels = borderWidth * (size / Mathf.Max(1f, pixelRadius * 2f));

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, half));
                float outside = dist - half;
                if (outside > 1f)
                    continue;

                float fromEdge = half - dist;
                Color color = fromEdge <= borderPixels ? border : fill;
                if (outside > 0f)
                    color.a *= 1f - outside;

                pixels[y * size + x] = color;
            }
        }

        circleTexture.SetPixels32(pixels);
        circleTexture.Apply(false, false);
        builtRadius = pixelRadius;
        builtBorder = borderWidth;
        builtFill = fill;
        builtBorderColor = border;
    }

    void RebuildCharge(float fill)
    {
        float stepped = Mathf.Round(fill * 48f) / 48f;
        if (chargeTexture != null && Mathf.Approximately(builtCharge, stepped))
            return;

        const int size = 128;
        if (chargeTexture == null)
        {
            chargeTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var pixels = new Color32[size * size];
        float half = (size - 1) * 0.5f;
        float sweep = stepped * Mathf.PI * 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half;
                float dy = y - half;
                if (dx * dx + dy * dy > half * half)
                    continue;

                float angle = Mathf.Atan2(dx, dy);
                if (angle < 0f)
                    angle += Mathf.PI * 2f;
                if (angle > sweep)
                    continue;

                pixels[y * size + x] = chargeColor;
            }
        }

        chargeTexture.SetPixels32(pixels);
        chargeTexture.Apply(false, false);
        builtCharge = stepped;
    }
}
