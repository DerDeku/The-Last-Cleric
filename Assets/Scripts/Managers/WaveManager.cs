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
        while (true)
        {
            while (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
                yield return null;

            if (waveTable == null || waveTable.waves == null || waveTable.waves.Length == 0)
            {
                Debug.LogWarning("WaveManager hat keine WaveTable.");
                ReturnToPrepare();
                continue;
            }

            int index = CampaignProgress.Selected(waveTable);
            int chapterNumber;
            int waveNumber;
            CampaignProgress.Locate(waveTable, index, out chapterNumber, out waveNumber);
            int chapter = chapterNumber - 1;
            int end = CampaignProgress.ChapterStart(waveTable, chapter) + CampaignProgress.ChapterLength(waveTable, chapter);
            end = Mathf.Min(end, waveTable.waves.Length);
            bool cleared = index < end;
            for (int i = index; i < end; i++)
            {
                yield return RunWave(waveTable.waves[i]);
                if (!AnyHeroAlive())
                {
                    cleared = false;
                    break;
                }

                if (i < end - 1 && waveTable.timeBetweenWaves > 0f)
                    yield return new WaitForSeconds(waveTable.timeBetweenWaves);
            }

            CampaignProgress.NoteChapter(waveTable, chapter, cleared);
            ReturnToPrepare();
        }
    }

    void ReturnToPrepare()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i] != null)
                Destroy(alive[i].gameObject);
        }

        alive.Clear();
        foreach (Enemy enemy in FindObjectsByType<Enemy>())
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        foreach (Projectile arrow in FindObjectsByType<Projectile>())
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }

        foreach (HitFeedback hit in FindObjectsByType<HitFeedback>())
        {
            if (hit != null)
                Destroy(hit.gameObject);
        }

        foreach (Hero hero in FindObjectsByType<Hero>())
        {
            if (hero != null)
                hero.ResetForMenu();
        }

        PrepareRoster roster = MenuHost.Instance != null ? MenuHost.Instance.Roster : Resources.Load<PrepareRoster>("Menu/Roster");
        Profile.PlaceParty(roster);
        ClericAvatar.EnsureBehindRanger();
        if (SpellController.Instance != null)
            SpellController.Instance.ResetBattle();
        if (MenuHost.Instance != null)
            MenuHost.Instance.ShowPrepare();
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Menu);
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
            if (hero.IsAlive && hero.IsDeployed)
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
