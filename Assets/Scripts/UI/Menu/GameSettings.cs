using UnityEngine;

public static class GameSettings
{
    const string LanguageKey = "settings.language";
    const string SfxKey = "settings.sfx";
    const string ChainKey = "settings.chain";

    public static string Language
    {
        get { return PlayerPrefs.GetString(LanguageKey, "en"); }
    }

    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat(SfxKey, 0.32f); }
    }

    public static void SetLanguage(string language)
    {
        PlayerPrefs.SetString(LanguageKey, language == "de" ? "de" : "en");
        PlayerPrefs.Save();
    }

    public static void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static bool ChainCasting
    {
        get { return PlayerPrefs.GetInt(ChainKey, 1) == 1; }
    }

    public static void SetChainCasting(bool enabled)
    {
        PlayerPrefs.SetInt(ChainKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
