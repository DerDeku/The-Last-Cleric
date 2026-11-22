using UnityEngine;

public static class HealEffect
{
    public static void HealFriends(Vector2 center, float radius, int power)
    {
        AffectFriends(center, radius, unit => unit.Heal(power));
    }

    public static void HealFriends(Vector2 center, float radius, System.Func<Unit, int> amount)
    {
        AffectFriends(center, radius, unit =>
        {
            int heal = amount != null ? amount(unit) : 0;
            if (heal > 0)
                unit.Heal(heal);
        });
    }

    public static bool Hits(Vector2 center, float radius, Unit unit)
    {
        if (unit == null || radius < 0f)
            return false;
        return Covers(center, radius * radius, unit);
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
            if (!unit.IsAlive || !unit.IsDeployed || unit.Team != UnitTeam.Friendly)
                continue;

            if (Covers(center, radiusSqr, unit))
                effect(unit);
        }
    }

    static bool Covers(Vector2 center, float radiusSqr, Unit unit)
    {
        Vector3 scale = unit.transform.lossyScale;
        Vector2 half = new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) * 0.5f;
        Vector2 position = unit.transform.position;
        float dx = Mathf.Max(Mathf.Abs(center.x - position.x) - half.x, 0f);
        float dy = Mathf.Max(Mathf.Abs(center.y - position.y) - half.y, 0f);
        return dx * dx + dy * dy <= radiusSqr;
    }
}
