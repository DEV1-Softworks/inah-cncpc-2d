// The player's inventory. Inherits the full slot-array implementation from
// InventoryBase; only adds locator registration so any system can call
// Inventories.Player.TryAdd(...) without serialized wiring.
//
// Exactly one PlayerInventory should be live in any scene; multiple instances
// would fight over `Inventories.Player`.
public class PlayerInventory : InventoryBase
{
    private void OnEnable()  => Inventories.RegisterPlayer(this);
    private void OnDisable() => Inventories.UnregisterPlayer(this);
}
