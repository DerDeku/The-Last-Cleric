using UnityEngine;

public static class RuntimeSprites
{
    static Sprite square;
    static Sprite squareLeft;
    static Sprite circle;

    public static Sprite Square()
    {
        if (square == null)
            square = MakeSquare(new Vector2(0.5f, 0.5f));
        return square;
    }

    public static Sprite SquareLeftPivot()
    {
        if (squareLeft == null)
            squareLeft = MakeSquare(new Vector2(0f, 0.5f));
        return squareLeft;
    }

    public static Sprite Circle()
    {
        if (circle != null)
            return circle;

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        var pixels = new Color32[size * size];
        float half = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(half, half));
                float alpha = Mathf.Clamp01(half - dist + 0.5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        circle = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
        circle.hideFlags = HideFlags.HideAndDontSave;
        return circle;
    }

    static Sprite MakeSquare(Vector2 pivot)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        var pixels = new Color32[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), pivot, 4f, 0, SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
