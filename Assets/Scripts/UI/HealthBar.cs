using UnityEngine;

public class HealthBar : MonoBehaviour
{
    const float LifeWidth = 1f;
    const float BarHeight = 0.14f;
    const float Pad = 0.1f;
    const float MaxShown = 1.5f;

    static readonly Color ShieldOnLife = new Color(0.35f, 0.7f, 1f, 0.55f);
    static readonly Color ShieldExtra = new Color(0.45f, 0.82f, 1f, 0.92f);
    static readonly Color ShieldHot = new Color(0.9f, 0.97f, 1f, 1f);

    Unit unit;
    Transform background;
    Transform healthFill;
    Transform shieldFill;
    Transform overflowFill;
    Transform previewFill;
    SpriteRenderer shieldRenderer;
    SpriteRenderer previewRenderer;
    SpriteRenderer overflowRenderer;
    GameObject lifeMark;

    public static void Attach(Unit target, Color fillColor)
    {
        var root = new GameObject("Health Bar");
        root.transform.SetParent(target.transform, false);
        root.transform.localPosition = new Vector3(0f, 0.72f, 0f);

        var bar = root.AddComponent<HealthBar>();
        bar.unit = target;
        bar.background = CreatePiece(root.transform, "Background", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.6f, 0f, 0f), new Vector3(LifeWidth + Pad * 2f, 0.24f, 1f), new Color(0.05f, 0.05f, 0.05f, 0.9f), 20);
        bar.healthFill = CreatePiece(root.transform, "Health", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(LifeWidth, BarHeight, 1f), fillColor, 21);
        bar.shieldFill = CreatePiece(root.transform, "Shield", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(0f, BarHeight, 1f), ShieldOnLife, 22);
        bar.overflowFill = CreatePiece(root.transform, "Shield Extra", RuntimeSprites.SquareLeftPivot(), new Vector3(0.5f, 0f, 0f), new Vector3(0f, BarHeight, 1f), ShieldExtra, 22);
        bar.previewFill = CreatePiece(root.transform, "Preview", RuntimeSprites.SquareLeftPivot(), new Vector3(-0.5f, 0f, 0f), new Vector3(0f, BarHeight, 1f), new Color(0.78f, 1f, 0.55f, 0.9f), 24);
        bar.previewRenderer = bar.previewFill.GetComponent<SpriteRenderer>();
        bar.lifeMark = CreatePiece(root.transform, "Life Mark", RuntimeSprites.Square(), new Vector3(0.5f, 0f, 0f), new Vector3(0.035f, 0.2f, 1f), new Color(1f, 1f, 1f, 0.8f), 25).gameObject;
        bar.lifeMark.SetActive(false);
        bar.shieldRenderer = bar.shieldFill.GetComponent<SpriteRenderer>();
        bar.overflowRenderer = bar.overflowFill.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (unit == null || healthFill == null)
            return;

        float health = unit.MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)unit.Health / unit.MaxHealth);
        healthFill.localScale = new Vector3(health * LifeWidth, BarHeight, 1f);

        float ratio = unit.MaxHealth <= 0 ? 0f : Mathf.Max(0f, (float)unit.Shield / unit.MaxHealth);
        float shown = Mathf.Min(ratio, MaxShown);
        float excess = Mathf.Max(0f, ratio - MaxShown);
        float hot = excess <= 0f ? 0f : Mathf.Clamp01(Mathf.Log(1f + excess) / Mathf.Log(5f));
        float covered = Mathf.Min(shown, 1f) * LifeWidth;
        float extra = Mathf.Max(0f, shown - 1f) * LifeWidth;

        shieldFill.localScale = new Vector3(covered, BarHeight, 1f);
        overflowFill.localScale = new Vector3(extra, BarHeight, 1f);
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = covered > 0.001f;
            shieldRenderer.color = Color.Lerp(ShieldOnLife, ShieldHot, hot);
        }

        if (overflowRenderer != null)
        {
            overflowRenderer.enabled = extra > 0.001f;
            overflowRenderer.color = Color.Lerp(ShieldExtra, ShieldHot, hot);
        }

        if (background != null)
        {
            float span = Mathf.Max(LifeWidth, shown * LifeWidth);
            background.localScale = new Vector3(span + Pad * 2f, 0.24f, 1f);
        }

        if (lifeMark != null)
            lifeMark.SetActive(extra > 0.001f);

        int preview = 0;
        SpellController spells = SpellController.Instance;
        if (spells != null)
            preview = spells.PreviewHeal(unit);
        float gain = unit.MaxHealth <= 0 ? 0f : Mathf.Clamp01((float)preview / unit.MaxHealth);
        float room = Mathf.Max(0f, 1f - health);
        float previewWidth = Mathf.Min(room, gain) * LifeWidth;
        if (previewFill != null)
        {
            previewFill.localPosition = new Vector3(-0.5f + health * LifeWidth, 0f, 0f);
            previewFill.localScale = new Vector3(previewWidth, BarHeight, 1f);
        }

        if (previewRenderer != null)
            previewRenderer.enabled = previewWidth > 0.001f;
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
