using UnityEngine;

public class HealFeedback : MonoBehaviour
{
    const float Life = 0.55f;
    const int BitCount = 6;

    static readonly Color HealBit = new Color(0.45f, 1f, 0.5f, 1f);
    static readonly Color HealFlash = new Color(0.65f, 1f, 0.7f, 1f);
    static readonly Color ShieldBit = new Color(0.45f, 0.78f, 1f, 1f);
    static readonly Color ShieldFlash = new Color(0.55f, 0.82f, 1f, 1f);

    SpriteRenderer body;
    Color bodyColor;
    Color bitColor;
    Color flashColor;
    Transform[] bits;
    Vector2[] velocities;
    float age;

    public static void Play(Unit unit)
    {
        Begin(unit, HealBit, HealFlash);
    }

    public static void PlayShield(Unit unit)
    {
        Begin(unit, ShieldBit, ShieldFlash);
    }

    static void Begin(Unit unit, Color bits, Color flash)
    {
        var effect = unit.GetComponent<HealFeedback>();
        if (effect == null)
            effect = unit.gameObject.AddComponent<HealFeedback>();
        effect.Restart(bits, flash);
    }

    void Restart(Color bitsTint, Color flash)
    {
        if (body == null)
        {
            body = GetComponent<SpriteRenderer>();
            if (body != null)
                bodyColor = body.color;
            CreateBits();
        }

        bitColor = bitsTint;
        flashColor = new Color(flash.r, flash.g, flash.b, bodyColor.a);
        age = 0f;
        enabled = true;
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i].gameObject.SetActive(true);
            bits[i].localPosition = new Vector3(Random.Range(-0.28f, 0.28f), 0.2f, 0f);
            velocities[i] = new Vector2(Random.Range(-0.45f, 0.45f), Random.Range(0.9f, 1.7f));
            SpriteRenderer renderer = bits[i].GetComponent<SpriteRenderer>();
            renderer.color = bitColor;
        }
    }

    void CreateBits()
    {
        bits = new Transform[BitCount];
        velocities = new Vector2[BitCount];
        for (int i = 0; i < BitCount; i++)
        {
            var bit = new GameObject("Heal Bit");
            bit.transform.SetParent(transform, false);
            bit.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

            var renderer = bit.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSprites.Circle();
            renderer.sortingOrder = 30;
            bits[i] = bit.transform;
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / Life);
        if (body != null)
            body.color = Color.Lerp(bodyColor, flashColor, Mathf.Sin(t * Mathf.PI));

        for (int i = 0; i < bits.Length; i++)
        {
            bits[i].localPosition += (Vector3)(velocities[i] * Time.deltaTime);
            SpriteRenderer renderer = bits[i].GetComponent<SpriteRenderer>();
            Color color = renderer.color;
            color.a = 1f - t;
            renderer.color = color;
        }

        if (age < Life)
            return;

        if (body != null)
            body.color = bodyColor;
        for (int i = 0; i < bits.Length; i++)
            bits[i].gameObject.SetActive(false);
        enabled = false;
    }
}
