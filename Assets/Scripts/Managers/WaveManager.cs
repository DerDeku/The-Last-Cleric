using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] WaveTable waveTable;
    [SerializeField] float spawnX = 8f;
    [SerializeField] float spawnYMin = -3.1f;
    [SerializeField] float spawnYMax = 3.1f;

    readonly List<Enemy> alive = new List<Enemy>();

    public int Wave { get; private set; }
    public float SpawnX => spawnX;
    public WaveTable WaveTable => waveTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExists()
    {
        if (FindAnyObjectByType<WaveManager>() != null)
            return;

        var systems = new GameObject("Game");
        systems.AddComponent<GameManager>();
        systems.AddComponent<WaveManager>();
    }

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        if (waveTable == null || waveTable.waves == null)
        {
            Debug.LogWarning("WaveManager hat keine WaveTable.");
            yield break;
        }

        foreach (WaveDefinition wave in waveTable.waves)
        {
            if (!AnyHeroAlive())
                break;

            yield return RunWave(wave);

            if (!AnyHeroAlive())
                break;

            yield return new WaitForSeconds(waveTable.timeBetweenWaves);
        }

        if (GameManager.Instance == null)
            yield break;

        GameManager.Instance.SetState(AnyHeroAlive() ? GameState.Victory : GameState.GameOver);
    }

    IEnumerator RunWave(WaveDefinition wave)
    {
        Wave++;
        if (wave == null || wave.spawns == null)
            yield break;

        foreach (WaveSpawn spawn in wave.spawns)
        {
            if (spawn == null || spawn.unit == null)
                continue;

            int count = Mathf.Max(0, spawn.count);
            for (int i = 0; i < count; i++)
            {
                if (!AnyHeroAlive())
                    yield break;

                SpawnEnemy(spawn.unit);
                if (spawn.spawnInterval > 0f)
                    yield return new WaitForSeconds(spawn.spawnInterval);
            }
        }

        while (AliveCount() > 0)
        {
            if (!AnyHeroAlive())
                yield break;

            yield return null;
        }
    }

    void SpawnEnemy(UnitDefinition definition)
    {
        var body = new GameObject(definition.displayName);
        float y = Random.Range(spawnYMin, spawnYMax);
        body.transform.position = new Vector3(spawnX, y, 0f);

        Enemy enemy = body.AddComponent<Enemy>();
        enemy.ApplyDefinition(definition);
        alive.Add(enemy);
    }

    static bool AnyHeroAlive()
    {
        Hero[] heroes = FindObjectsByType<Hero>();
        foreach (Hero hero in heroes)
        {
            if (hero.IsAlive)
                return true;
        }

        return false;
    }

    int AliveCount()
    {
        alive.RemoveAll(enemy => enemy == null);
        return alive.Count;
    }
}
