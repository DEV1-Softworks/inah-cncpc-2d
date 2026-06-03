// A chest's inventory. Same slot-array behavior as PlayerInventory but lives
// on the chest GameObject and does NOT register with Inventories.Player —
// each chest is its own independent storage that systems reach via the
// chest's ChestInteractable, not via a global locator.
//
// Empty class body is intentional: it's a separate type so future chest-only
// behavior (locked chests, themed-only contents, etc.) can attach to this
// class without polluting PlayerInventory.
public class ChestInventory : InventoryBase
{
}
