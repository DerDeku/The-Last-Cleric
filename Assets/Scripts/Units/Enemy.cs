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
        float distance = Vector2.Distance(transform.position, destination);
        if (distance <= AttackRange)
            return;

        transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}
