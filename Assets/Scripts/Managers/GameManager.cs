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
        State = GameState.Menu;
    }

    public void SetState(GameState state)
    {
        State = state;
    }
}
