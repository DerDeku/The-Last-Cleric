using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Menu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetState(GameState.Menu);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetState(GameState state)
    {
        State = state;
        bool paused = state == GameState.Menu || state == GameState.GameOver;
        Time.timeScale = paused ? 0f : 1f;
        if (paused)
            SpellSounds.Stop();
    }
}
