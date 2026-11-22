using System;
using UnityEngine;

[Serializable]
public class RosterEntry
{
    public string id;
    public string nameKey;
    public string unlockEvent;
    public bool startsActive;
    public Vector2 home;
    public UnitDefinition unit;
    public SkillTree tree;
}

[CreateAssetMenu(fileName = "Roster", menuName = "Skills/Prepare Roster")]
public class PrepareRoster : ScriptableObject
{
    public int partySize = 4;
    public int lockedSlot = 4;
    public string lockedNode = "fourth";
    public RosterEntry[] heroes;
    public RosterEntry cleric;
    public SpellData[] spells;
}
