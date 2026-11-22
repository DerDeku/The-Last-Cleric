using System;
using UnityEngine;

[Serializable]
public class SkillNode
{
    public string id;
    public string titleKey;
    public string bodyKey;
    public Vector2 position;
    public string[] links;
    public string revealEvent;
    public string effectId;
    public int cost = 1;
    public bool root;
}

[CreateAssetMenu(fileName = "SkillTree", menuName = "Skills/Skill Tree")]
public class SkillTree : ScriptableObject
{
    public string id;
    public SkillNode[] nodes;
}
