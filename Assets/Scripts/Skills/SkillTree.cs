using System;
using UnityEngine;

public enum SkillEffectKind
{
    None,
    Damage,
    MoveSpeed,
    Range,
    Health,
    Interval
}

[Serializable]
public class SkillNode
{
    public string id;
    public string titleKey;
    public string bodyKey;
    public Vector2 position;
    public string[] links;
    public string revealEvent;
    public SkillEffectKind effect;
    public float amount;
    public SpellData spell;
    [Tooltip("Gold price of the first rank. The root stays free. Further ranks cost a multiple of this.")]
    public int cost = 25;
    [Tooltip("How many times this node can be bought. 1 is a normal node.")]
    public int stacks = 1;
    public bool root;
}

[CreateAssetMenu(fileName = "SkillTree", menuName = "Skills/Skill Tree")]
public class SkillTree : ScriptableObject
{
    public string id;
    public SkillNode[] nodes;
}
