using UnityEngine;
using UnityEngine.InputSystem;

public class CursorIndicator : MonoBehaviour
{
    [SerializeField] float pixelRadius = 100f;
    [SerializeField] float borderWidth = 4f;
    [SerializeField] Color fillColor = new Color(0.2f, 1f, 0.45f, 0.28f);
    [SerializeField] Color borderColor = new Color(1f, 1f, 1f, 1f);

    Texture2D circleTexture;
    float builtRadius = -1f;
    float builtBorder = -1f;
    Color builtFill;
    Color builtBorderColor;

    public float PixelRadius => pixelRadius;

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        var pointer = UnityEngine.InputSystem.Mouse.current;
        if (pointer == null)
            return;

        RebuildIfNeeded();
        if (circleTexture == null)
            return;

        Vector2 screen = pointer.position.ReadValue();
        float x = screen.x - pixelRadius;
        float y = Screen.height - screen.y - pixelRadius;
        float diameter = pixelRadius * 2f;
        GUI.DrawTexture(new Rect(x, y, diameter, diameter), circleTexture, ScaleMode.StretchToFill, true);
    }

    void OnValidate()
    {
        pixelRadius = Mathf.Max(8f, pixelRadius);
        borderWidth = Mathf.Clamp(borderWidth, 1f, pixelRadius * 0.5f);
    }

    void OnDestroy()
    {
        if (circleTexture != null)
            Destroy(circleTexture);
    }

    void RebuildIfNeeded()
    {
        if (circleTexture != null
            && Mathf.Approximately(builtRadius, pixelRadius)
            && Mathf.Approximately(builtBorder, borderWidth)
            && builtFill == fillColor
            && builtBorderColor == borderColor)
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
        float border = borderWidth * (size / (pixelRadius * 2f));

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, half));
                float outside = dist - half;
                if (outside > 1f)
                    continue;

                float fromEdge = half - dist;
                Color color = fromEdge <= border ? borderColor : fillColor;
                if (outside > 0f)
                    color.a *= 1f - outside;

                pixels[y * size + x] = color;
            }
        }

        circleTexture.SetPixels32(pixels);
        circleTexture.Apply(false, false);
        builtRadius = pixelRadius;
        builtBorder = borderWidth;
        builtFill = fillColor;
        builtBorderColor = borderColor;
    }
}
