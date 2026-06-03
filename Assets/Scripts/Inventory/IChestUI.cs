// The contract any "chest presenter" implements. The chest UI shows two
// IInventory side by side (the chest's contents and the player's) and lets
// the player move items between them.
//
// Same shape as IDialogueService — a tiny static locator + events tells the
// rest of the game when a chest is open without coupling them to the concrete
// implementation.
public interface IChestUI
{
    void Open(IInventory chestInventory);
    void Close();
    bool IsOpen { get; }
}

// Static locator + events for the chest UI. ChestInteractables call
// Chests.Open(myInventory). Listeners (PlayerController, audio, etc.)
// subscribe to OnOpened / OnClosed to react to "a chest is open."
//
// Events fire ONLY on real transitions: opening an already-open UI doesn't
// re-fire OnOpened; closing an already-closed UI doesn't fire OnClosed.
public static class Chests
{
    public static IChestUI Active { get; private set; }

    public static event System.Action OnOpened;
    public static event System.Action OnClosed;

    public static void Register(IChestUI ui) => Active = ui;
    public static void Unregister(IChestUI ui)
    {
        if (Active == ui) Active = null;
    }

    public static void Open(IInventory chestInventory)
    {
        if (Active == null || chestInventory == null) return;
        bool wasOpen = Active.IsOpen;
        Active.Open(chestInventory);
        if (!wasOpen && Active.IsOpen) OnOpened?.Invoke();
    }

    public static void Close()
    {
        if (Active == null) return;
        bool wasOpen = Active.IsOpen;
        Active.Close();
        if (wasOpen && !Active.IsOpen) OnClosed?.Invoke();
    }

    public static bool IsOpen => Active?.IsOpen ?? false;
}
