using System;
using UnityEngine;

[Serializable]
public class CampaignChapter
{
    public int waveCount = 1;
}

[CreateAssetMenu(fileName = "WaveTable", menuName = "Waves/Wave Table")]
public class WaveTable : ScriptableObject
{
    public float timeBetweenWaves = 2f;
    public WaveDefinition[] waves = Array.Empty<WaveDefinition>();
    public CampaignChapter[] chapters = Array.Empty<CampaignChapter>();
}
