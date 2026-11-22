using System;
using UnityEngine;

[Serializable]
public class RosterEntry
{
    public string id;
    public string nameKey;
    public string unlockEvent;
    public bool startsActive;
    public UnitDefinition unit;
    public SkillTree tree;
}

[CreateAssetMenu(fileName = "Roster", menuName = "Skills/Prepare Roster")]
public class PrepareRoster : ScriptableObject
{
    public int partySize = 4;
    public RosterEntry[] heroes;
    public RosterEntry cleric;
    public SpellData[] spells;
}
