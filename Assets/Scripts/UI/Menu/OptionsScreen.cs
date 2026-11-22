using UnityEngine;

public class OptionsScreen : IMenuScreen
{
    public void Draw(MenuHost host)
    {
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.28f, 440f, 360f));
        GUILayout.Label(Loc.Text("menu.options"), MenuText.Title());
        GUILayout.Space(16f);
        GUILayout.Label(Loc.Text("options.language"), MenuText.Body());
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(Loc.Text("options.english"), GUILayout.Height(36f)))
            GameSettings.SetLanguage("en");
        if (GUILayout.Button(Loc.Text("options.german"), GUILayout.Height(36f)))
            GameSettings.SetLanguage("de");
        GUILayout.EndHorizontal();
        GUILayout.Space(18f);
        GUILayout.Label(Loc.Text("options.sfx"), MenuText.Body());
        float volume = GUILayout.HorizontalSlider(GameSettings.SfxVolume, 0f, 1f, GUILayout.Height(24f));
        if (!Mathf.Approximately(volume, GameSettings.SfxVolume))
            GameSettings.SetSfxVolume(volume);
        GUILayout.Space(18f);
        string chain = GameSettings.ChainCasting ? Loc.Text("options.on") : Loc.Text("options.off");
        if (GUILayout.Button(Loc.Text("options.chain") + ": " + chain, GUILayout.Height(36f)))
            GameSettings.SetChainCasting(!GameSettings.ChainCasting);
        GUILayout.Space(24f);
        if (GUILayout.Button(Loc.Text("menu.back"), GUILayout.Height(40f)))
            host.ShowMain();
        GUILayout.EndArea();
    }
}

public static class MenuText
{
    public static GUIStyle Title()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28
        };
        style.normal.textColor = Color.white;
        return style;
    }

    public static GUIStyle Body()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16
        };
        style.normal.textColor = Color.white;
        return style;
    }

    public static GUIStyle Wrap()
    {
        GUIStyle style = Body();
        style.wordWrap = true;
        return style;
    }

    public static GUIStyle Mark()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            alignment = TextAnchor.UpperLeft
        };
        style.normal.textColor = new Color(1f, 0.9f, 0.55f, 1f);
        return style;
    }
}
