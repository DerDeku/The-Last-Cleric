using UnityEngine;

public class SlotsScreen : IMenuScreen
{
    public void Draw(MenuHost host)
    {
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.22f, 480f, 460f));
        GUILayout.Label(Loc.Text("menu.slots"), MenuText.Title());
        GUILayout.Space(16f);
        for (int slot = 1; slot <= SaveGame.SlotCount; slot++)
            DrawSlot(host, slot);
        GUILayout.Space(12f);
        if (GUILayout.Button(Loc.Text("menu.back"), GUILayout.Height(40f)))
            host.ShowMain();
        GUILayout.EndArea();
    }

    static void DrawSlot(MenuHost host, int slot)
    {
        GUILayout.BeginHorizontal();
        if (SaveGame.Exists(slot))
        {
            string label = Loc.Format("menu.continue", slot, SaveGame.GoldTextIn(slot));
            if (GUILayout.Button(label, GUILayout.Height(48f)))
            {
                SaveGame.LoadOrCreate(slot);
                host.ShowPrepare();
            }

            if (GUILayout.Button(Loc.Text("menu.delete"), GUILayout.Width(110f), GUILayout.Height(48f)))
                SaveGame.Delete(slot);
        }
        else if (GUILayout.Button(Loc.Format("menu.new", slot), GUILayout.Height(48f)))
        {
            SaveGame.LoadOrCreate(slot);
            host.ShowPrepare();
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
    }
}
