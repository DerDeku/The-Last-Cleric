using UnityEngine;

[CreateAssetMenu(fileName = "WaveTable", menuName = "Waves/Wave Table")]
public class WaveTable : ScriptableObject
{
    public float timeBetweenWaves = 2f;
    public WaveDefinition[] waves = System.Array.Empty<WaveDefinition>();
}
