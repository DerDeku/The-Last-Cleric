using System;
using UnityEngine;

[Serializable]
public class WaveSpawn
{
    public UnitDefinition unit;
    public int count = 1;
    public float spawnInterval = 0.5f;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Waves/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    public WaveSpawn[] spawns = Array.Empty<WaveSpawn>();
}
