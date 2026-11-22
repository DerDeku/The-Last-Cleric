using UnityEngine;

public class ClericAvatar : MonoBehaviour
{
    public enum Pose
    {
        Idle,
        Heal,
        Yield,
        Flee
    }

    Pose pose = Pose.Idle;
    Transform hands;
    Hero ranger;

    public Pose Current => pose;

    public void Play(Pose next)
    {
        pose = next;
    }

    public static void EnsureBehindRanger()
    {
        if (FindAnyObjectByType<ClericAvatar>() != null)
            return;

        Hero ranger = FindRanger();
        Vector3 spot = ranger != null ? ranger.transform.position : new Vector3(-5.4f, 0f, 0f);
        spot.x -= 1.25f;
        spot.z = 0f;

        var body = new GameObject("Cleric");
        body.transform.position = spot;
        ClericAvatar avatar = body.AddComponent<ClericAvatar>();
        avatar.ranger = ranger;
        avatar.Build();
    }

    void Build()
    {
        AddPart("Robe", RuntimeSprites.Square(), new Vector3(0f, 0.05f, 0f), new Vector3(0.62f, 1.05f, 1f), new Color(0.86f, 0.8f, 0.62f, 1f), 6);
        AddPart("Head", RuntimeSprites.Circle(), new Vector3(0f, 0.72f, 0f), Vector3.one * 0.36f, new Color(0.95f, 0.84f, 0.72f, 1f), 7);
        hands = AddPart("Hands", RuntimeSprites.Square(), new Vector3(0f, 0.2f, 0f), Vector3.one * 0.2f, new Color(0.95f, 0.84f, 0.72f, 1f), 7);
    }

    void LateUpdate()
    {
        if (pose == Pose.Flee)
        {
            transform.position += Vector3.left * (1.6f * Time.deltaTime);
            return;
        }

        if (ranger == null)
            ranger = FindRanger();

        if (ranger != null)
        {
            Vector3 spot = ranger.transform.position;
            spot.x -= 1.25f;
            spot.z = 0f;
            transform.position = spot;
        }

        if (hands == null)
            return;

        hands.localPosition = pose == Pose.Heal || pose == Pose.Yield
            ? new Vector3(0f, 0.85f, 0f)
            : new Vector3(0f, 0.2f, 0f);
    }

    Transform AddPart(string partName, Sprite sprite, Vector3 local, Vector3 scale, Color color, int order)
    {
        var part = new GameObject(partName);
        part.transform.SetParent(transform, false);
        part.transform.localPosition = local;
        part.transform.localScale = scale;
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return part.transform;
    }

    static Hero FindRanger()
    {
        PrepareRoster roster = Resources.Load<PrepareRoster>("Menu/Roster");
        UnitDefinition ranger = null;
        if (roster != null && roster.heroes != null)
        {
            foreach (RosterEntry entry in roster.heroes)
            {
                if (entry != null && entry.id == "ranger")
                    ranger = entry.unit;
            }
        }

        foreach (Hero hero in FindObjectsByType<Hero>())
        {
            if (hero != null && hero.Definition != null && (ranger == null ? hero.Definition.displayName == "Ranger" : hero.Definition == ranger))
                return hero;
        }

        return null;
    }
}
