using UnityEngine;

public class HitFeedback : MonoBehaviour
{
    const float Life = 0.26f;
    const int BitCount = 4;

    static readonly Color HitColor = new Color(1f, 0.9f, 0.72f, 1f);
    static readonly Color ShieldColor = new Color(0.72f, 0.88f, 1f, 1f);

    Transform[] bits;
    Vector2[] velocities;
    SpriteRenderer flash;
    Color color;
    float age;

    public static void Play(Unit unit, bool shieldOnly)
    {
        var effect = new GameObject("Hit");
        effect.transform.position = unit.transform.position;
        effect.AddComponent<HitFeedback>().Begin(shieldOnly ? ShieldColor : HitColor);
    }

    void Begin(Color tint)
    {
        color = tint;
        bits = new Transform[BitCount];
        velocities = new Vector2[BitCount];
        flash = CreateSprite("Flash", 0.28f);

        for (int i = 0; i < BitCount; i++)
        {
            bits[i] = CreateSprite("Spark", Random.Range(0.06f, 0.1f)).transform;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0.9f, 1.7f);
        }
    }

    SpriteRenderer CreateSprite(string name, float scale)
    {
        var piece = new GameObject(name);
        piece.transform.SetParent(transform, false);
        piece.transform.localScale = Vector3.one * scale;

        var renderer = piece.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.Circle();
        renderer.color = color;
        renderer.sortingOrder = 31;
        return renderer;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / Life);
        float alpha = 1f - t;

        if (flash != null)
        {
            Color tint = color;
            tint.a = alpha;
            flash.color = tint;
            flash.transform.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.36f, t);
        }

        for (int i = 0; i < bits.Length; i++)
        {
            bits[i].localPosition += (Vector3)(velocities[i] * Time.deltaTime);
            SpriteRenderer renderer = bits[i].GetComponent<SpriteRenderer>();
            Color tint = color;
            tint.a = alpha;
            renderer.color = tint;
        }

        if (age >= Life)
            Destroy(gameObject);
    }
}
