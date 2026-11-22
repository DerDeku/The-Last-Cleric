using UnityEngine;

public class Enemy : Unit
{
    Unit targetUnit;

    public void SetTarget(Unit unit)
    {
        targetUnit = unit;
    }

    void Update()
    {
        if (!IsAlive || targetUnit == null || !targetUnit.IsAlive)
            return;

        if (IsInAttackPosition(targetUnit))
            return;

        Vector3 destination = targetUnit.transform.position;
        destination.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}
