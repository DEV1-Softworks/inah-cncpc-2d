using UnityEngine;

// World-side pickup: drop this on any GameObject with a Collider2D in the
// world to make it harvestable. On Interact, the item flows into the
// player's inventory via the static Inventories locator.
//
// Behavior:
//  - If the inventory has room for some of the count, that portion is added
//    and `_count` is reduced. The GameObject is destroyed when _count hits 0
//    (unless _destroyOnPickup is off — handy for "infinite" sources).
//  - If the inventory has zero room for the item right now, nothing happens
//    and the pickup remains as-is.
//
// `_destroyOnPickup` off is useful for things like a hopper or a free-sample
// crate — Interact gives 1, the pickup stays put for the next press.
public class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Item _item;
    [SerializeField] private int  _count = 1;
    [SerializeField] private bool _destroyOnPickup = true;

    // Runtime configuration entry point. Used when something (e.g.
    // PlayerDropHandler) instantiates a generic DroppedItem prefab and needs
    // to tell it what item and how many to be. Also syncs the GameObject's
    // visible sprite to the item's icon if a SpriteRenderer is present.
    public void Configure(Item item, int count)
    {
        _item  = item;
        _count = count;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && item != null && item.Icon != null) sr.sprite = item.Icon;
    }

    public void Interact()
    {
        if (_item == null || _count <= 0) return;

        var inv = Inventories.Player;
        if (inv == null) return; // no player inventory in scene — silently no-op

        int added = inv.TryAdd(_item, _count);
        if (added <= 0) return;  // inventory full of this kind right now — leave us alone

        _count -= added;

        if (_count <= 0 && _destroyOnPickup)
            Destroy(gameObject);
    }
}
