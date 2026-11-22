using UnityEngine;

public class AttackChargeBar : MonoBehaviour
{
    Transform fill;
    SpriteRenderer backgroundRenderer;
    SpriteRenderer fillRenderer;

    public static AttackChargeBar Attach(Unit unit)
    {
        var root = new GameObject("Attack Charge");
        root.transform.SetParent(unit.transform, false);
        root.transform.localPosition = new Vector3(0f, 0.98f, 0f);

        var bar = root.AddComponent<AttackChargeBar>();
        bar.backgroundRenderer = CreatePiece(root.transform, "Background", RuntimeSprites.Square(), new Vector3(0f, 0f, 0f), new Vector3(1.2f, 0.12f, 1f), new Color(0.12f, 0.1f, 0.02f, 0.9f), 22);
        bar.fill = CreatePiece(root.transform, "Fill", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(0f, 0.07f, 1f), new Color(1f, 0.82f, 0.2f, 1f), 23);
        bar.fillRenderer = bar.fill.GetComponent<SpriteRenderer>();
        bar.SetVisible(false);
        return bar;
    }

    public void SetVisible(bool visible)
    {
        backgroundRenderer.enabled = visible;
        fillRenderer.enabled = visible;
    }

    public void SetProgress(float amount)
    {
        fill.localScale = new Vector3(Mathf.Clamp01(amount), 0.07f, 1f);
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
