using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    Unit self;
    AttackChargeBar chargeBar;
    float progress;

    void Awake()
    {
        self = GetComponent<Unit>();
        chargeBar = AttackChargeBar.Attach(self);
    }

    void Update()
    {
        if (self == null || !self.IsAlive || self.ContactDamage <= 0)
        {
            progress = 0f;
            chargeBar.SetVisible(false);
            return;
        }

        List<Unit> targets = FindTargets();
        if (targets.Count == 0)
        {
            progress = 0f;
            chargeBar.SetVisible(false);
            return;
        }

        chargeBar.SetVisible(true);
        progress += Time.deltaTime / self.AttackInterval;
        if (progress >= 1f)
        {
            progress = 0f;
            int damage = self.ContactDamage;
            foreach (Unit target in targets)
            {
                if (target != null && target.IsAlive)
                    target.Damage(damage);
            }
        }

        chargeBar.SetProgress(progress);
    }

    List<Unit> FindTargets()
    {
        var targets = new List<Unit>();
        Unit nearest = null;
        float nearestDistance = float.MaxValue;
        Unit[] units = FindObjectsByType<Unit>();
        foreach (Unit candidate in units)
        {
            if (candidate == self || !candidate.IsAlive || candidate.Team == self.Team)
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
