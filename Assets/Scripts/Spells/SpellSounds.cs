using UnityEngine;

public static class SpellSounds
{
    static AudioSource source;
    static AudioClip tick;
    static AudioClip burst;
    static AudioClip shield;

    public static void Bind(AudioSource audio)
    {
        source = audio;
    }

    public static void Play(SpellKind kind)
    {
        if (source == null)
            return;

        switch (kind)
        {
            case SpellKind.BurstHeal:
                source.PlayOneShot(Burst(), 0.5f);
                break;
            case SpellKind.Shield:
                source.PlayOneShot(Shield(), 0.55f);
                break;
            default:
                source.PlayOneShot(Tick(), 0.32f);
                break;
        }
    }

    static AudioClip Tick()
    {
        if (tick == null)
            tick = Make("Heal Tick", 17, 0.12f, 740f, 460f, 0.22f);
        return tick;
    }

    static AudioClip Burst()
    {
        if (burst == null)
            burst = Make("Burst Whoosh", 29, 0.2f, 540f, 190f, 0.4f);
        return burst;
    }

    static AudioClip Shield()
    {
        if (shield == null)
            shield = Make("Shield Whoosh", 41, 0.24f, 260f, 90f, 0.55f);
        return shield;
    }

    static AudioClip Make(string name, int seed, float seconds, float fromHz, float toHz, float noiseMix)
    {
        const int rate = 22050;
        int count = Mathf.Max(2, Mathf.RoundToInt(rate * seconds));
        var data = new float[count];
        var random = new System.Random(seed);
        float phase = 0f;
        float noise = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            float envelope = Mathf.Clamp01(t / 0.1f) * (1f - t) * (1f - t);
            float hz = Mathf.Lerp(fromHz, toHz, t);
            phase += hz / rate;
            float tone = Mathf.Sin(phase * Mathf.PI * 2f);
            float white = (float)random.NextDouble() * 2f - 1f;
            noise = noise * 0.8f + white * 0.2f;
            data[i] = (tone * (1f - noiseMix) + noise * noiseMix) * envelope * 0.75f;
        }

        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        clip.hideFlags = HideFlags.HideAndDontSave;
        return clip;
    }
}
