using UnityEngine;

// Consumable: pressing Use logs a message and reports "yes, deduct 1." Drop
// onto berries, potions, eaten items.
//
// Real game effects (heal HP, restore energy, status buff, etc.) hook in
// here later by extending or composing — e.g., a HealthSystem reference +
// an int "amount restored."
[CreateAssetMenu(menuName = "Inventory/Use Behaviors/Consumable",
                 fileName = "ConsumableUseBehavior")]
public class ConsumableUseBehavior : ItemUseBehavior
{
    [SerializeField, TextArea] private string _onUseLog = "consumed.";

    public override bool Use(GameObject user, ItemStack stack)
    {
        Debug.Log($"[Use] {stack.item.DisplayName} {_onUseLog}");
        return true;
    }
}
