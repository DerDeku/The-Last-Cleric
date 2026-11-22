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
    [Tooltip("Off: this wave starts after the previous one is defeated. On: the timer starts when the previous wave starts.")]
    public bool useTimer;
    [Tooltip("Seconds from the start of the previous wave until this one spawns.")]
    public float timer = 30f;
    public WaveSpawn[] spawns = Array.Empty<WaveSpawn>();
}
