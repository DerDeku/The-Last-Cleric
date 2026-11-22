using UnityEngine;

public class Projectile : MonoBehaviour
{
    const float Speed = 9f;
    const float Life = 3f;

    Unit target;
    Vector3 aim;
    int damage;
    float age;

    public static void Launch(Unit from, Unit to, int amount)
    {
        if (from == null || to == null || amount <= 0)
            return;

        var body = new GameObject("Arrow");
        body.transform.position = from.transform.position;
        body.transform.localScale = new Vector3(0.52f, 0.52f, 1f);

        var renderer = body.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.Arrow();
        renderer.sortingOrder = 25;
        renderer.color = from.Team == UnitTeam.Friendly
            ? new Color(0.9f, 0.95f, 0.75f, 1f)
            : new Color(1f, 0.42f, 0.28f, 1f);

        var arrow = body.AddComponent<Projectile>();
        arrow.target = to;
        arrow.aim = to.transform.position;
        arrow.damage = amount;
        arrow.Face(arrow.aim - body.transform.position);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (target != null && target.IsAlive)
            aim = target.transform.position;

        Vector3 next = Vector3.MoveTowards(transform.position, aim, Speed * Time.deltaTime);
        Face(next - transform.position);
        transform.position = next;

        if (age < Life && (transform.position - aim).sqrMagnitude > 0.008f)
            return;

        if (target != null && target.IsAlive)
            target.Damage(damage);

        Destroy(gameObject);
    }

    void Face(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
