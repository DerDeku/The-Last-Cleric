using System.Collections.Generic;
using UnityEngine;

public static class BattleLines
{
    static readonly List<Unit> living = new List<Unit>();
    static int frame = -1;
    static Unit front;
    static Unit back;

    public static Unit Pick(BattleLine line, Unit seeker)
    {
        if (Time.frameCount != frame)
            Refresh();

        if (line == BattleLine.Closest)
            return Closest(seeker);

        return line == BattleLine.Backline ? back : front;
    }

    static Unit Closest(Unit seeker)
    {
        if (seeker == null)
            return null;

        Unit best = null;
        float bestDistance = float.MaxValue;
        Vector2 from = seeker.transform.position;
        for (int i = 0; i < living.Count; i++)
        {
            Unit unit = living[i];
            if (unit == null || unit == seeker || unit.Team == seeker.Team)
                continue;

            float distance = ((Vector2)unit.transform.position - from).sqrMagnitude;
            if (distance >= bestDistance)
                continue;

            best = unit;
            bestDistance = distance;
        }

        return best;
    }

    static void Refresh()
    {
        frame = Time.frameCount;
        living.Clear();
        front = null;
        back = null;
        float frontDepth = float.MaxValue;
        float backDepth = float.MinValue;
        float spawnX = 8f;
        WaveManager waves = Object.FindAnyObjectByType<WaveManager>();
        if (waves != null)
            spawnX = waves.SpawnX;

        Unit[] units = Object.FindObjectsByType<Unit>();
        foreach (Unit unit in units)
        {
            if (unit == null || !unit.IsAlive || !unit.IsDeployed)
                continue;

            living.Add(unit);
            if (unit.Team != UnitTeam.Friendly)
                continue;

            float depth = Mathf.Abs(unit.transform.position.x - spawnX);
            if (IsBetterFront(unit, depth, frontDepth))
            {
                front = unit;
                frontDepth = depth;
            }

            if (IsBetterBack(unit, depth, backDepth))
            {
                back = unit;
                backDepth = depth;
            }
        }
    }

    static bool IsBetterFront(Unit unit, float depth, float bestDepth)
    {
        if (front == null)
            return true;
        if (depth < bestDepth - 0.02f)
            return true;
        return Mathf.Abs(depth - bestDepth) <= 0.02f && unit.transform.position.y < front.transform.position.y;
    }

    static bool IsBetterBack(Unit unit, float depth, float bestDepth)
    {
        if (back == null)
            return true;
        if (depth > bestDepth + 0.02f)
            return true;
        return Mathf.Abs(depth - bestDepth) <= 0.02f && unit.transform.position.y < back.transform.position.y;
    }
}
