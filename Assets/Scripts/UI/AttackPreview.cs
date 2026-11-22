using UnityEngine;

public class AttackPreview : MonoBehaviour
{
    Unit unit;
    SpriteRenderer areaRenderer;

    public static void Attach(Unit target)
    {
        var root = new GameObject("Attack Preview");
        root.transform.SetParent(target.transform, false);

        var preview = root.AddComponent<AttackPreview>();
        preview.unit = target;
        preview.areaRenderer = root.AddComponent<SpriteRenderer>();
        preview.areaRenderer.color = new Color(1f, 0.85f, 0.2f, 0.16f);
        preview.areaRenderer.sortingOrder = 1;
    }

    void LateUpdate()
    {
        if (unit == null)
            return;

        bool square = unit.AreaForm == AreaForm.Square;
        areaRenderer.sprite = square ? RuntimeSprites.Square() : RuntimeSprites.Circle();

        float parentX = Mathf.Max(0.0001f, unit.transform.lossyScale.x);
        float parentY = Mathf.Max(0.0001f, unit.transform.lossyScale.y);
        float width = unit.AttackRange * 2f;
        float height = square ? unit.AttackHeight * 2f : width;
        transform.localScale = new Vector3(width / parentX, height / parentY, 1f);
    }
}
