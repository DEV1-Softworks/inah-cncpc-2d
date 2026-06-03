using UnityEngine;

// Abstract "what happens when the player uses this item." Authored as
// ScriptableObjects so designers can pick a behavior from the Item's
// Inspector without writing code per item.
//
// Concrete subclasses (Consumable, DebugLog today; ToolSwing, Placeable,
// HealsHp, PlantsSeed, etc. later) implement Use(). Return true if one count
// should be deducted from the stack ("eaten"), false if the item should stay
// (tools, placement that failed).
//
// Why ScriptableObjects and not a polymorphic Item subclass? Reuse — one
// "Consumable that logs +5 HP" behavior asset can be referenced by many Item
// assets (different berries / potions). And we can swap behavior without
// recompiling, just by changing the inspector reference.
public abstract class ItemUseBehavior : ScriptableObject
{
    // user  : the GameObject driving Use (typically the Player).
    // stack : the slot's current ItemStack (item + count). Read-only as far
    //         as this method is concerned; the caller does any TryRemove
    //         based on the return value.
    // returns: true -> caller should deduct 1 from the stack.
    public abstract bool Use(GameObject user, ItemStack stack);
}
