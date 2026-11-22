using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Spells/Spell Data")]
public class SpellData : ScriptableObject
{
    public string spellName = "Heal";
    public float radius = 100f;
    public int power = 5;
    public Color fillColor = new Color(0.2f, 1f, 0.45f, 0.28f);
    public Color borderColor = new Color(1f, 1f, 1f, 1f);
}
