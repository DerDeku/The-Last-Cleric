using UnityEngine;

public class Hero : Unit
{
    Vector3 anchor;

    protected override void Awake()
    {
        base.Awake();
        anchor = transform.position;
    }

    void Update()
    {
        if (!IsAlive || !HoldPosition || AttackDelivery != AttackDelivery.Melee || MoveSpeed <= 0f)
            return;

        Unit threat = null;
        float threatDistance = float.MaxValue;
        bool engaged = false;
        Vector2 self = transform.position;
        Unit[] units = FindObjectsByType<Unit>();
        foreach (Unit other in units)
        {
            if (other == null || other == this || !other.IsAlive || other.Team == Team)
                continue;

            if (CanHit(other) || other.CanHit(this))
                engaged = true;

            if (other.AttackDelivery != AttackDelivery.Ranged || CanHit(other) || !IsShooting(other, units))
                continue;

            float distance = (self - (Vector2)other.transform.position).sqrMagnitude;
            if (distance >= threatDistance)
                continue;

            threat = other;
            threatDistance = distance;
        }

        if (threat != null)
        {
            MoveTo(threat.transform.position);
            return;
        }

        if (!engaged)
            MoveTo(anchor);
    }

    static bool IsShooting(Unit ranged, Unit[] units)
    {
        foreach (Unit ally in units)
        {
            if (ally != null && ally.IsAlive && ally.Team == UnitTeam.Friendly && ranged.CanHit(ally))
                return true;
        }

        return false;
    }

    void MoveTo(Vector3 destination)
    {
        destination.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);
    }

    protected override void OnDeath()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        enabled = false;
    }
}
