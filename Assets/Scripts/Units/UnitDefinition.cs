using UnityEngine;

public enum UnitShape
{
    Square,
    Circle
}

public enum UnitTeam
{
    Friendly,
    Hostile
}

public enum AttackShape
{
    Single,
    Area
}

public enum AreaForm
{
    Circle,
    Square
}

public enum AttackDelivery
{
    Melee,
    Ranged
}

public enum BattleLine
{
    Frontline,
    Backline,
    Closest
}

[CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    public string displayName = "Unit";
    public int maxHealth = 10;
    public float moveSpeed = 0f;
    public int contactDamage = 0;
    public float attackRange = 1f;
    public float attackHeight = 0f;
    public float attackInterval = 1.2f;
    public AttackShape attackShape = AttackShape.Single;
    public AreaForm areaForm = AreaForm.Circle;
    public AttackDelivery attackDelivery = AttackDelivery.Melee;
    public bool previewAttack = false;
    public UnitTeam team = UnitTeam.Friendly;
    public float bodySize = 1f;
    public UnitShape shape = UnitShape.Square;
    public Color bodyColor = Color.white;
    public Color healthBarColor = new Color(0.35f, 0.95f, 0.4f, 1f);
    public BattleLine targetLine = BattleLine.Frontline;
    public bool holdPosition = false;
    [Range(0f, 1f)] public float attackMoveMultiplier = 0f;
}
