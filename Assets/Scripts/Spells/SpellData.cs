using UnityEngine;

public enum SpellKind
{
    PeriodicHeal,
    BurstHeal,
    Shield
}

[CreateAssetMenu(fileName = "Spell", menuName = "Spells/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName = "Heal";
    public SpellKind kind = SpellKind.PeriodicHeal;
    public float radius = 2f;
    public int power = 8;
    public float cooldown = 0f;
    public float castTime = 0f;
    public float tickRate = 0.8f;
    public Color fillColor = new Color(0.2f, 1f, 0.45f, 0.28f);
    public Color borderColor = new Color(1f, 1f, 1f, 1f);
}
