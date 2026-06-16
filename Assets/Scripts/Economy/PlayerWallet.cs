using UnityEngine;

// The single concrete wallet. Goes on the Player root (or any GameObject
// under Persistent — the wallet must survive scene transitions to carry the
// player's pesos across the world).
public class PlayerWallet : MonoBehaviour, IWallet
{
    [Tooltip("Starting balance for new runs. For dev / playtests you may want a non-zero starting amount to skip the early grind.")]
    [SerializeField] private int _startingPesos = 0;

    public int Pesos { get; private set; }

    private void Awake()
    {
        Pesos = Mathf.Max(0, _startingPesos);
    }

    private void OnEnable()
    {
        Wallet.Register(this);
        // Broadcast current balance so any listener that woke up before us
        // (e.g. WalletHudView) updates with the real value. Critical at start
        // and after scene reloads where Unity's OnEnable order isn't guaranteed.
        Wallet.NotifyChanged(Pesos);
    }

    private void OnDisable() => Wallet.Unregister(this);

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Pesos += amount;
        Wallet.NotifyChanged(Pesos);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return false;
        if (Pesos < amount) return false;
        Pesos -= amount;
        Wallet.NotifyChanged(Pesos);
        return true;
    }
}
