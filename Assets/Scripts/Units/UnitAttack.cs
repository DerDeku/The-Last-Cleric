using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    const float LooseGrace = 0.6f;

    Unit self;
    AttackChargeBar chargeBar;
    float progress;
    float looseTime;

    void Awake()
    {
        self = GetComponent<Unit>();
        chargeBar = AttackChargeBar.Attach(self);
    }

    void Update()
    {
        if (self == null || !self.IsAlive || !self.IsDeployed || self.ContactDamage <= 0)
        {
            progress = 0f;
            chargeBar.SetVisible(false);
            return;
        }

        List<Unit> targets = FindTargets();
        if (targets.Count == 0)
        {
            looseTime += Time.deltaTime;
            if (progress <= 0f || looseTime >= LooseGrace)
            {
                progress = 0f;
                looseTime = 0f;
                chargeBar.SetVisible(false);
                return;
            }

            chargeBar.SetVisible(true);
            chargeBar.SetProgress(progress);
            return;
        }

        looseTime = 0f;
        chargeBar.SetVisible(true);
        progress += Time.deltaTime / self.AttackInterval;
        if (progress >= 1f)
        {
            progress = 0f;
            int damage = self.ContactDamage;
            foreach (Unit target in targets)
            {
                if (target == null || !target.IsAlive)
                    continue;

                if (self.AttackDelivery == AttackDelivery.Ranged)
                    Projectile.Launch(self, target, damage);
                else
                    target.Damage(damage);
            }
        }

        chargeBar.SetProgress(progress);
    }

    List<Unit> FindTargets()
    {
        var targets = new List<Unit>();
        Unit focus = self.AttackFocus;
        if (focus != null && focus.IsAlive)
        {
            if (self.CanHit(focus))
                targets.Add(focus);
            return targets;
        }

        Unit nearest = null;
        float nearestDistance = float.MaxValue;
        Unit[] units = FindObjectsByType<Unit>();
        foreach (Unit candidate in units)
        {
            if (candidate == self || !candidate.IsAlive || !candidate.IsDeployed || candidate.Team == self.Team)
                continue;

            if (!self.CanHit(candidate))
                continue;

            float distance = ((Vector2)candidate.transform.position - (Vector2)self.transform.position).sqrMagnitude;

            if (self.AttackShape == AttackShape.Area)
            {
                targets.Add(candidate);
                continue;
            }

            if (distance > nearestDistance)
                continue;

            nearest = candidate;
            nearestDistance = distance;
        }

        if (self.AttackShape == AttackShape.Single && nearest != null)
            targets.Add(nearest);

        return targets;
    }
}
