using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] int coins = 1;
    [SerializeField] int xp = 1;

    public void Drop()
    {
        if (MetaProgressionManager.Instance == null)
            return;

        MetaProgressionManager.Instance.AddCoins(coins);
        MetaProgressionManager.Instance.AddXp(xp);
    }
}
