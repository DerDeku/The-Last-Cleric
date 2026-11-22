using UnityEngine;

public static class GameSettings
{
    const string LanguageKey = "settings.language";
    const string SfxKey = "settings.sfx";

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
}
