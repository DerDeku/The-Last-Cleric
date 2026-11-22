using UnityEngine;

public class Hero : Unit
{
    protected override void OnDeath()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = new Color(0.35f, 0.35f, 0.35f, 1f);

        enabled = false;
    }
}
