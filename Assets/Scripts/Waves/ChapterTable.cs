using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterTable", menuName = "Waves/Chapter Table")]
public class ChapterTable : ScriptableObject
{
    public WaveDefinition[] waves = Array.Empty<WaveDefinition>();
}
