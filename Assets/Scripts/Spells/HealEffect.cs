using UnityEngine;

public static class HealEffect
{
    public static void HealFriends(Vector2 center, float radius, int power)
    {
        AffectFriends(center, radius, unit => unit.Heal(power));
    }

    public static void ShieldFriends(Vector2 center, float radius, int power)
    {
        AffectFriends(center, radius, unit => unit.AddShield(power));
    }

    static void AffectFriends(Vector2 center, float radius, System.Action<Unit> effect)
    {
        if (effect == null || radius < 0f)
            return;

        float radiusSqr = radius * radius;
        Unit[] units = Object.FindObjectsByType<Unit>();
        foreach (Unit unit in units)
        {
            if (!unit.IsAlive || unit.Team != UnitTeam.Friendly)
                continue;

            Vector2 position = unit.transform.position;
            if ((position - center).sqrMagnitude <= radiusSqr)
                effect(unit);
        }
    }
}
