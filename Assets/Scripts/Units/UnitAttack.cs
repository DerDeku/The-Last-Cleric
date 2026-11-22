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

        Unit target = FindTarget();
        if (target == null)
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
            target.Damage(self.ContactDamage);
        }

        chargeBar.SetProgress(progress);
    }

    Unit FindTarget()
    {
        Unit best = null;
        float bestDistance = self.AttackRange * self.AttackRange;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit candidate in units)
        {
            if (candidate == self || !candidate.IsAlive || candidate.Team == self.Team)
                continue;

            float distance = ((Vector2)candidate.transform.position - (Vector2)self.transform.position).sqrMagnitude;
            if (distance > bestDistance)
                continue;

            best = candidate;
            bestDistance = distance;
        }

        return best;
    }
}
