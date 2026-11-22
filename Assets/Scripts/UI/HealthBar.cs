using UnityEngine;

public class HealthBar : MonoBehaviour
{
    Unit unit;
    Transform healthFill;
    Transform shieldFill;
    SpriteRenderer shieldRenderer;

    public static void Attach(Unit target, Color fillColor)
    {
        var root = new GameObject("Health Bar");
        root.transform.SetParent(target.transform, false);
        root.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        var bar = root.AddComponent<HealthBar>();
        bar.unit = target;
        CreatePiece(root.transform, "Background", RuntimeSprites.Square(), new Vector3(0f, 0f, 0f), new Vector3(1.2f, 0.24f, 1f), new Color(0.05f, 0.05f, 0.05f, 0.9f), 20);
        bar.healthFill = CreatePiece(root.transform, "Health", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(1f, 0.14f, 1f), fillColor, 21);
        bar.shieldFill = CreatePiece(root.transform, "Shield", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(0f, 0.14f, 1f), new Color(0.35f, 0.7f, 1f, 0.55f), 22);
        bar.shieldRenderer = bar.shieldFill.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (unit == null || healthFill == null)
            return;

        float health = unit.MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)unit.Health / unit.MaxHealth);
        healthFill.localScale = new Vector3(health, 0.14f, 1f);

        float shield = unit.MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)unit.Shield / unit.MaxHealth);
        shieldFill.localScale = new Vector3(shield, 0.14f, 1f);
        if (shieldRenderer != null)
            shieldRenderer.enabled = shield > 0.001f;
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
