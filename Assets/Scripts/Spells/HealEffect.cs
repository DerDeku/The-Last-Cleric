using UnityEngine;

public class HealEffect : MonoBehaviour
{
    [SerializeField] SpellData spell;

    public void Apply(Vector2 worldCenter, float worldRadius)
    {
        if (spell == null)
            return;

        float radiusSqr = worldRadius * worldRadius;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in units)
        {
            Vector2 position = unit.transform.position;
            if ((position - worldCenter).sqrMagnitude <= radiusSqr)
                unit.Heal(spell.power);
        }
    }
}
