using UnityEngine;

public class MainMenuScreen : IMenuScreen
{
    public void Draw(MenuHost host)
    {
        GUILayout.BeginArea(new Rect(0f, Screen.height * 0.28f, Screen.width, 320f));
        GUILayout.BeginVertical();
        GUILayout.Label(Loc.Text("menu.title"), Title());
        GUILayout.Space(28f);
        if (CenteredButton(Loc.Text("menu.play")))
            host.ShowSlots();
        if (CenteredButton(Loc.Text("menu.options")))
            host.ShowOptions();
        if (CenteredButton(Loc.Text("menu.quit")))
            Quit();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    static bool CenteredButton(string label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool pressed = GUILayout.Button(label, GUILayout.Width(220f), GUILayout.Height(42f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        return pressed;
    }

    static GUIStyle Title()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32
        };
        style.normal.textColor = Color.white;
        return style;
    }

    static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
