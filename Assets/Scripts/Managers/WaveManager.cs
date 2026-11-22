using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] WaveTable waveTable;
    [SerializeField] float spawnX = 8f;
    [SerializeField] float spawnYMin = -3.1f;
    [SerializeField] float spawnYMax = 3.1f;

    class WaveRun
    {
        public WaveDefinition definition;
        public bool started;
        public bool spawnsDone;
        public bool timerOn;
        public bool noted;
        public float elapsed;
        public readonly List<Enemy> enemies = new List<Enemy>();
    }

    readonly List<Enemy> alive = new List<Enemy>();
    readonly List<Coroutine> spawning = new List<Coroutine>();

    bool wiped;
    bool showTimer;
    float timerTotal;
    float timerElapsed;
    float deathFade;
    GUIStyle timerStyle;
    GUIStyle deathTitle;
    GUIStyle deathBody;

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

            if (waveTable == null || CampaignProgress.Count(waveTable) == 0)
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
            end = Mathf.Min(end, CampaignProgress.Count(waveTable));
            wiped = false;
            bool cleared = false;
            if (index < end)
                yield return PlayChapter(index, end, value => cleared = value);
            if (wiped)
                yield return DeathScreen();

            CampaignProgress.NoteChapter(waveTable, chapter, cleared);
            ReturnToPrepare();
        }
    }

    void OnGUI()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.State == GameState.GameOver)
        {
            DrawDeath();
            return;
        }

        if (Event.current.type != EventType.Repaint || GameManager.Instance.State != GameState.Playing)
            return;

        EnsureTimerStyle();
        GUI.Label(new Rect(Screen.width * 0.5f - 80f, 12f, 160f, 28f), BattleLog.Stopwatch(), timerStyle);
        if (!showTimer)
            return;

        GUI.Label(new Rect(Screen.width * 0.5f - 180f, 40f, 360f, 28f), Clock(timerTotal) + " - " + Clock(timerElapsed), timerStyle);
    }

    void EnsureTimerStyle()
    {
        if (timerStyle != null)
            return;

        timerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22
        };
        timerStyle.normal.textColor = Color.white;
    }

    static string Clock(float seconds)
    {
        int whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
        if (whole >= 60)
        {
            int minutes = whole / 60;
            int rest = whole % 60;
            return minutes + ":" + rest.ToString("00") + "m";
        }

        return whole + "s";
    }

    IEnumerator PlayChapter(int index, int end, System.Action<bool> done)
    {
        var runs = new List<WaveRun>();
        for (int i = index; i < end; i++)
            runs.Add(new WaveRun { definition = CampaignProgress.Wave(waveTable, i) });

        wiped = false;
        showTimer = false;
        deathFade = 0f;
        BattleLog.Reset();
        StopSpawns();
        if (runs.Count > 0)
            BeginWave(runs, 0);

        while (!wiped && !ChapterDone(runs))
        {
            if (!AnyHeroAlive())
            {
                wiped = true;
                break;
            }

            float step = Time.deltaTime;
            BattleLog.Tick(step);
            NoteClears(runs, index);
            TickWaits(runs, step);
            RefreshTimer(runs);
            TryStart(runs);
            yield return null;
        }

        showTimer = false;
        StopSpawns();
        NoteClears(runs, index);
        done(done != null && !wiped && ChapterDone(runs));
    }

    void NoteClears(List<WaveRun> runs, int start)
    {
        for (int i = 0; i < runs.Count; i++)
        {
            WaveRun run = runs[i];
            if (run.noted || !run.started || !run.spawnsDone || AliveOf(run) > 0)
                continue;

            run.noted = true;
            GrantWave(start + i);
        }
    }

    void GrantWave(int globalIndex)
    {
        if (waveTable == null || globalIndex != SaveGame.Cleared)
            return;

        int selected = SaveGame.SelectedWave(CampaignProgress.Count(waveTable));
        bool firstWave = globalIndex == 0 && !SaveGame.HasEvent("wave.1");
        SaveGame.SetProgress(SaveGame.Cleared + 1, selected);
        CampaignProgress.Reveal(waveTable);
        if (!firstWave)
            return;

        SaveGame.AddEvent("wave.1");
        BattleLog.AddUnlock("unlock.shield");
    }

    void BeginWave(List<WaveRun> runs, int index)
    {
        WaveRun run = runs[index];
        run.started = true;
        spawning.Add(StartCoroutine(SpawnWave(run)));
        int next = index + 1;
        if (next < runs.Count && runs[next].definition != null && runs[next].definition.useTimer)
        {
            runs[next].timerOn = true;
            runs[next].elapsed = 0f;
        }
    }

    void TryStart(List<WaveRun> runs)
    {
        for (int i = 0; i < runs.Count; i++)
        {
            if (runs[i].started || !Ready(runs, i))
                continue;
            BeginWave(runs, i);
        }
    }

    bool Ready(List<WaveRun> runs, int index)
    {
        if (index <= 0)
            return true;

        WaveRun run = runs[index];
        WaveDefinition definition = run.definition;
        if (definition != null && definition.useTimer)
            return run.timerOn && run.elapsed >= Mathf.Max(0f, definition.timer);

        return PreviousClear(runs[index - 1]) && run.elapsed >= Mathf.Max(0f, waveTable.timeBetweenWaves);
    }

    void TickWaits(List<WaveRun> runs, float step)
    {
        for (int i = 1; i < runs.Count; i++)
        {
            WaveRun run = runs[i];
            if (run.started)
                continue;

            WaveDefinition definition = run.definition;
            if (definition != null && definition.useTimer)
            {
                if (run.timerOn)
                    run.elapsed += step;
                continue;
            }

            if (PreviousClear(runs[i - 1]))
                run.elapsed += step;
        }
    }

    void RefreshTimer(List<WaveRun> runs)
    {
        showTimer = false;
        for (int i = 0; i < runs.Count; i++)
        {
            WaveRun run = runs[i];
            if (run.started || !run.timerOn || run.definition == null || !run.definition.useTimer)
                continue;

            showTimer = true;
            timerTotal = Mathf.Max(0f, run.definition.timer);
            timerElapsed = run.elapsed;
            return;
        }
    }

    static bool PreviousClear(WaveRun run)
    {
        return run.started && run.spawnsDone && AliveOf(run) == 0;
    }

    bool ChapterDone(List<WaveRun> runs)
    {
        for (int i = 0; i < runs.Count; i++)
        {
            if (!runs[i].started || !runs[i].spawnsDone)
                return false;
        }

        return AliveCount() == 0;
    }

    static int AliveOf(WaveRun run)
    {
        run.enemies.RemoveAll(enemy => enemy == null);
        return run.enemies.Count;
    }

    void StopSpawns()
    {
        for (int i = 0; i < spawning.Count; i++)
        {
            if (spawning[i] != null)
                StopCoroutine(spawning[i]);
        }

        spawning.Clear();
    }

    IEnumerator DeathScreen()
    {
        BattleLog.Order();
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.GameOver);

        const float FadeTime = 1.15f;
        while (!BattleLog.DismissPressed())
        {
            deathFade = Mathf.MoveTowards(deathFade, 1f, Time.unscaledDeltaTime / FadeTime);
            yield return null;
        }
    }

    void DrawDeath()
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.04f, 0.05f, 0.07f, Mathf.Lerp(0f, 0.72f, deathFade));
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        if (deathFade < 0.45f)
        {
            GUI.color = previous;
            return;
        }

        GUI.color = new Color(1f, 1f, 1f, deathFade);
        EnsureDeathStyles();
        float width = Mathf.Min(560f, Screen.width - 80f);
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.16f;
        GUI.Label(new Rect(x, y, width, 42f), Loc.Text("death.title"), deathTitle);
        y += 44f;
        GUI.Label(new Rect(x, y, width, 24f), BattleLog.Stopwatch(), deathBody);
        y += 32f;

        if (BattleLog.Kills.Count == 0)
            y = Line(x, y, width, Loc.Text("death.empty"));
        else
        {
            for (int i = 0; i < BattleLog.Kills.Count; i++)
            {
                BattleLog.Row row = BattleLog.Kills[i];
                y = Line(x, y, width, row.Name + " (" + row.Amount + ")");
            }
        }

        y += 8f;
        y = Line(x, y, width, Loc.Text("death.damage"));
        y = Amounts(x, y, width, BattleLog.Damage, true);
        y += 8f;
        y = Line(x, y, width, Loc.Text("death.healing"));
        y = Amounts(x, y, width, BattleLog.Heals, false);
        y += 8f;
        y = Line(x, y, width, Loc.Text("death.shield"));
        y = Amounts(x, y, width, BattleLog.Shields, false);
        if (BattleLog.Unlocks.Count > 0)
        {
            y += 8f;
            y = Line(x, y, width, Loc.Text("death.unlocked"));
            for (int i = 0; i < BattleLog.Unlocks.Count; i++)
                y = Line(x, y, width, Loc.Text(BattleLog.Unlocks[i].Name));
        }

        y += 10f;
        y = Line(x, y, width, Loc.Format("death.gold", BattleLog.Gold));
        Line(x, y, width, Loc.Format("death.total", SaveGame.GoldText));
        GUI.Label(new Rect(x, Screen.height - 64f, width, 24f), Loc.Text("death.continue"), deathBody);
        GUI.color = previous;
    }

    float Amounts(float x, float y, float width, System.Collections.Generic.IReadOnlyList<BattleLog.Row> rows, bool withWho)
    {
        if (rows.Count == 0)
            return Line(x, y, width, Loc.Text("death.empty"));

        for (int i = 0; i < rows.Count; i++)
        {
            BattleLog.Row row = rows[i];
            string label = withWho ? row.Name + "  ·  " + Loc.Text(row.Source) : Loc.Text(row.Name);
            y = Line(x, y, width, label + "  " + row.Amount);
        }

        return y;
    }

    float Line(float x, float y, float width, string text)
    {
        GUI.Label(new Rect(x, y, width, 22f), text, deathBody);
        return y + 22f;
    }

    void EnsureDeathStyles()
    {
        if (deathTitle != null)
            return;

        deathTitle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32
        };
        deathTitle.normal.textColor = Color.white;
        deathBody = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16
        };
        deathBody.normal.textColor = Color.white;
    }

    void ReturnToPrepare()
    {
        showTimer = false;
        StopSpawns();
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

    IEnumerator SpawnWave(WaveRun run)
    {
        Wave++;
        WaveDefinition wave = run.definition;
        if (wave == null || wave.spawns == null)
        {
            run.spawnsDone = true;
            yield break;
        }

        foreach (WaveSpawn spawn in wave.spawns)
        {
            if (spawn == null || spawn.unit == null)
                continue;

            int count = Mathf.Max(0, spawn.count);
            for (int i = 0; i < count; i++)
            {
                if (!AnyHeroAlive())
                {
                    wiped = true;
                    run.spawnsDone = true;
                    yield break;
                }

                SpawnEnemy(spawn.unit, run);
                if (spawn.spawnInterval > 0f)
                    yield return new WaitForSeconds(spawn.spawnInterval);
            }
        }

        run.spawnsDone = true;
    }

    void SpawnEnemy(UnitDefinition definition, WaveRun run)
    {
        var body = new GameObject(definition.displayName);
        float y = Random.Range(spawnYMin, spawnYMax);
        body.transform.position = new Vector3(spawnX, y, 0f);

        Enemy enemy = body.AddComponent<Enemy>();
        enemy.ApplyDefinition(definition);
        alive.Add(enemy);
        run.enemies.Add(enemy);
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
