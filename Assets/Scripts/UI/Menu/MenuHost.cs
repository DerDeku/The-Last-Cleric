using UnityEngine;

[DefaultExecutionOrder(50)]
public class MenuHost : MonoBehaviour
{
    public static MenuHost Instance { get; private set; }

    PrepareRoster roster;
    IMenuScreen screen;
    readonly MainMenuScreen main = new MainMenuScreen();
    readonly OptionsScreen options = new OptionsScreen();
    readonly PrepareScreen prepare = new PrepareScreen();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureExists()
    {
        if (FindAnyObjectByType<MenuHost>() != null)
            return;

        GameManager host = FindAnyObjectByType<GameManager>();
        GameObject body = host != null ? host.gameObject : new GameObject("Game");
        if (host == null)
            body.AddComponent<GameManager>();
        body.AddComponent<MenuHost>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        roster = Resources.Load<PrepareRoster>("Menu/Roster");
        screen = main;
    }

    void OnGUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Menu)
            return;
        if (screen == null)
            return;

        GUI.color = new Color(0.07f, 0.08f, 0.09f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        screen.Draw(this);
    }

    public PrepareRoster Roster => roster;

    public void ShowMain()
    {
        screen = main;
    }

    public void ShowOptions()
    {
        screen = options;
    }

    public void ShowPrepare()
    {
        screen = prepare;
    }

    public void BeginBattle()
    {
        Profile.PlaceParty(roster);
        SkillEffects.ApplyRoster(roster);
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(GameState.Playing);
    }
}

public interface IMenuScreen
{
    void Draw(MenuHost host);
}
