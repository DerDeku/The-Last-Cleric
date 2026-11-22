using UnityEngine;

public enum UnitShape
{
    Square,
    Circle
}

[CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    public string displayName = "Unit";
    public int maxHealth = 10;
    public float moveSpeed = 0f;
    public int contactDamage = 0;
    public float bodySize = 1f;
    public UnitShape shape = UnitShape.Square;
    public Color bodyColor = Color.white;
    public Color healthBarColor = new Color(0.35f, 0.95f, 0.4f, 1f);
}
