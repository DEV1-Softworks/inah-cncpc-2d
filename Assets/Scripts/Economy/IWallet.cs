// Abstract storage for the player's currency (pesos). Implemented today by
// PlayerWallet. Mirrors the IInventory + Inventories pattern so that systems
// that need to know "how much money does the player have?" don't depend on
// the concrete component.
public interface IWallet
{
    int Pesos { get; }

    // Always succeeds; negative amounts are ignored. Use for sale proceeds,
    // rewards, gifts.
    void Add(int amount);

    // Returns true only if the player had enough. Use for purchases, donations
    // to the research fund, any outflow.
    bool TrySpend(int amount);
}

// Static locator + change event. Concrete wallet (PlayerWallet) registers
// itself in OnEnable; HUD / vendor UI / future shop systems read through here.
//
// OnChanged fires after every successful Add or TrySpend with the new balance,
// so HUD subscribers can refresh without polling.
public static class Wallet
{
    public static IWallet Active { get; private set; }

    public static event System.Action<int> OnChanged;

    public static void Register(IWallet w)   => Active = w;
    public static void Unregister(IWallet w)
    {
        if (Active == w) Active = null;
    }

    public static void NotifyChanged(int newBalance) => OnChanged?.Invoke(newBalance);

    public static int  Pesos => Active?.Pesos ?? 0;
    public static void Add(int amount) => Active?.Add(amount);
    public static bool TrySpend(int amount) => Active?.TrySpend(amount) ?? false;
}
