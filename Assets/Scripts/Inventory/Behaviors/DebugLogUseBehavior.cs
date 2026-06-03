using UnityEngine;

// Debug-log: pressing Use logs a message and DOES NOT consume the item.
// Useful as a placeholder while wiring tools (axes, watering cans) — swap to
// a real ToolSwingBehavior later without changing any Item asset references
// (they all just point to this slot until the real behavior lands).
[CreateAssetMenu(menuName = "Inventory/Use Behaviors/Debug Log",
                 fileName = "DebugLogUseBehavior")]
public class DebugLogUseBehavior : ItemUseBehavior
{
    [SerializeField] private string _message = "used (placeholder).";

    public override bool Use(GameObject user, ItemStack stack)
    {
        Debug.Log($"[Use] {stack.item.DisplayName} {_message}");
        return false;
    }
}
