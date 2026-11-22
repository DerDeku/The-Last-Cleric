using UnityEngine;

public class Enemy : Unit
{
    Transform target;

    public void SetTarget(Transform hero)
    {
        target = hero;
    }

    void Update()
    {
        if (!IsAlive || target == null)
            return;

        Vector3 destination = target.position;
        destination.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) > 0.55f)
            return;

        Hero hero = target.GetComponent<Hero>();
        if (hero != null && hero.IsAlive)
            hero.Damage(ContactDamage);

        Damage(Health);
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}
