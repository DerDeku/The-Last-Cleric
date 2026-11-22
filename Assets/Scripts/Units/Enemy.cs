using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    const float Comfort = 0.72f;

    static readonly List<Enemy> active = new List<Enemy>();
    static int nextSpread;

    Unit targetUnit;
    int spread;

    public void SetTarget(Unit unit)
    {
        targetUnit = unit;
    }

    void OnEnable()
    {
        active.Add(this);
        spread = nextSpread++;
    }

    void OnDisable()
    {
        active.Remove(this);
    }

    void Update()
    {
        if (!IsAlive)
            return;

        targetUnit = BattleLines.Pick(TargetLine, this);
        float speed = MoveSpeed;
        if (targetUnit != null && targetUnit.IsAlive && IsInAttackPosition(targetUnit))
            speed = AttackDelivery == AttackDelivery.Ranged && !targetUnit.CanHit(this)
                ? MoveSpeed * AttackMoveMultiplier
                : 0f;

        if (targetUnit != null && targetUnit.IsAlive && speed > 0f)
        {
            Vector3 destination = targetUnit.transform.position;
            destination.z = transform.position.z;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        }

        Separate();
    }

    void Separate()
    {
        Vector2 origin = transform.position;
        float radius = Mathf.Max(0.2f, transform.lossyScale.x * 0.5f);
        Vector2 push = Vector2.zero;

        for (int i = 0; i < active.Count; i++)
        {
            Enemy other = active[i];
            if (other == null || other == this || !other.IsAlive)
                continue;

            Vector2 away = origin - (Vector2)other.transform.position;
            float distance = away.magnitude;
            float otherRadius = Mathf.Max(0.2f, other.transform.lossyScale.x * 0.5f);
            float comfort = (radius + otherRadius) * Comfort;
            if (distance >= comfort)
                continue;

            if (distance < 0.001f)
            {
                float angle = (spread & 1023) * (Mathf.PI * 2f / 1024f);
                away = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
            else
            {
                away /= distance;
            }

            push += away * ((comfort - distance) / comfort);
        }

        if (push.sqrMagnitude < 0.0001f)
            return;

        if (push.sqrMagnitude > 1f)
            push.Normalize();

        float slide = Mathf.Max(MoveSpeed, 0.6f) * 0.45f;
        Vector3 next = transform.position + (Vector3)(push * slide * Time.deltaTime);
        next.z = transform.position.z;
        transform.position = next;
    }

    protected override void OnDeath()
    {
        Destroy(gameObject);
    }
}
