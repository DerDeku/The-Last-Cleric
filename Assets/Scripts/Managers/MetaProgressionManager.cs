using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    const string CoinsKey = "meta.coins";
    const string XpKey = "meta.xp";

    public static MetaProgressionManager Instance { get; private set; }

    public int Coins { get; private set; }
    public int Xp { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Coins = PlayerPrefs.GetInt(CoinsKey, 0);
        Xp = PlayerPrefs.GetInt(XpKey, 0);
    }

    public void AddCoins(int amount)
    {
        Coins += Mathf.Max(0, amount);
        Save();
    }

    public void AddXp(int amount)
    {
        Xp += Mathf.Max(0, amount);
        Save();
    }

    void Save()
    {
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.SetInt(XpKey, Xp);
        PlayerPrefs.Save();
    }
}
