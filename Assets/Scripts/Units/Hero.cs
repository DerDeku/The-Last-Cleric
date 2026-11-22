using UnityEngine;

public class Hero : Unit
{
    Vector3 anchor;
    Unit focus;

    protected override void Awake()
    {
        base.Awake();
        anchor = transform.position;
    }

    void Update()
    {
        if (!IsAlive || !IsDeployed || !HoldPosition || AttackDelivery != AttackDelivery.Melee || MoveSpeed <= 0f)
        {
            focus = null;
            SetAttackFocus(null);
            return;
        }

        if (focus != null && !focus.IsAlive)
            focus = null;

        if (focus == null)
            focus = NearestShootingRanged();

        SetAttackFocus(focus);
        if (focus != null)
        {
            if (!IsInAttackPosition(focus))
                MoveTo(focus.transform.position);
            return;
        }

        MoveTo(anchor);
    }

    Unit NearestShootingRanged()
    {
        Unit threat = null;
        float threatDistance = float.MaxValue;
        Vector2 self = transform.position;
        Unit[] units = FindObjectsByType<Unit>();
        foreach (Unit other in units)
        {
            if (other == null || other == this || !other.IsAlive || other.Team == Team)
                continue;

            if (other.AttackDelivery != AttackDelivery.Ranged || !IsShooting(other, units))
                continue;

            float distance = (self - (Vector2)other.transform.position).sqrMagnitude;
            if (distance >= threatDistance)
                continue;

            threat = other;
            threatDistance = distance;
        }

        return threat;
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
