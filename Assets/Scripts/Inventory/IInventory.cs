// Abstract storage for ItemStacks. Implemented today by PlayerInventory; in
// Phase 3 by ChestInventory; later by any container that needs to hold
// stacks. The contract is intentionally minimal — callers that need richer
// queries (find first match, total count, etc.) build them on top of these
// primitives.
public interface IInventory
{
    int Capacity { get; }

    ItemStack GetSlot(int index);

    // Try to add `count` of `item`. Returns the count actually added (0 if
    // there was no room). Stacks into existing slots first, then fills
    // empties.
    int TryAdd(Item item, int count);

    // Try to remove `count` of `item`. Returns the count actually removed
    // (0 if the inventory had none of it). Searches all slots; useful for
    // "consume any berry I have, anywhere".
    int TryRemove(Item item, int count);

    // Try to remove `count` from THIS specific slot. Returns the count
    // actually removed. Useful when the caller has a slot index in hand
    // (chest UI clicks, drop-from-slot, etc.) and shouldn't pull from
    // other matching stacks elsewhere in the inventory.
    int TryRemoveFromSlot(int slotIndex, int count);

    // True if the inventory can accept `count` of `item` right now.
    bool HasSpace(Item item, int count);

    // Fired after any add/remove that changed contents. UI subscribes to
    // this so it can repaint without polling.
    event System.Action OnChanged;
}

// Static locator: every system in the game finds the player's inventory
// through Inventories.Player without needing a serialized reference. Same
// pattern we use for Dialogue. PlayerInventory registers itself; pickups,
// the future hotbar UI, chests, shops, etc. all read it from here.
//
// Why a locator instead of pure DI like IMovementInput? Because the player's
// inventory is a true singleton-ish system — wiring it into every pickup in
// the world would be tedious and brittle.
public static class Inventories
{
    public static IInventory Player { get; private set; }

    public static void RegisterPlayer(IInventory inv) => Player = inv;
    public static void UnregisterPlayer(IInventory inv)
    {
        if (Player == inv) Player = null;
    }
}
