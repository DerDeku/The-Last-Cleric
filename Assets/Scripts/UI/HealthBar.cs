using UnityEngine;

public class HealthBar : MonoBehaviour
{
    Unit unit;
    Transform fill;

    public static void Attach(Unit target, Color fillColor)
    {
        var root = new GameObject("Health Bar");
        root.transform.SetParent(target.transform, false);
        root.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        var bar = root.AddComponent<HealthBar>();
        bar.unit = target;
        CreatePiece(root.transform, "Background", RuntimeSprites.Square(), new Vector3(0f, 0f, 0f), new Vector3(1.2f, 0.18f, 1f), new Color(0.05f, 0.05f, 0.05f, 0.9f), 20);
        bar.fill = CreatePiece(root.transform, "Fill", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(1f, 0.1f, 1f), fillColor, 21);
    }

    void LateUpdate()
    {
        if (unit == null || fill == null)
            return;

        float amount = unit.MaxHealth <= 0 ? 0f : (float)unit.Health / unit.MaxHealth;
        amount = Mathf.Clamp01(amount);
        fill.localScale = new Vector3(amount, 0.1f, 1f);
    }

    static Transform CreatePiece(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
    {
        var piece = new GameObject(name);
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = localPosition;
        piece.transform.localScale = localScale;

        var renderer = piece.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return piece.transform;
    }
}
