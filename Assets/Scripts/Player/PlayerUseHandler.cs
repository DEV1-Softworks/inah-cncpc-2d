using UnityEngine;

// Reads the Use input and runs the selected hotbar slot's ItemUseBehavior.
//
// Wires (Inspector):
//   _inputSource -> the PlayerInputReader (must implement IMovementInput;
//                   reads its UsePressed property)
//
// Resolves at runtime via the locator pattern:
//   - Hotbar.SelectedSlot      (which slot to act on)
//   - Inventories.Player       (where to read / consume from)
//   - stack.item.UseBehavior   (what to actually do)
//
// If any of those isn't present (no hotbar in scene, no inventory, empty
// slot, item has no UseBehavior assigned), Use is a silent no-op. That's
// intentional — pressing Use on nothing should do nothing, not crash.
public class PlayerUseHandler : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _inputSource; // must implement IMovementInput

    private IMovementInput _input;

    private void Awake()
    {
        _input = _inputSource as IMovementInput;
        if (_input == null && _inputSource != null)
            Debug.LogError($"{name}: {_inputSource.GetType().Name} does not implement IMovementInput.");
    }

    private void Update()
    {
        if (_input == null || !_input.UsePressed) return;

        // While a blocking overlay is open (chest, dialogue, vendor, future
        // crafting / menu), the click belongs to that UI — NOT to the world /
        // hotbar use loop. Skip silently so e.g. clicking a slot in the vendor
        // panel to sell it doesn't also consume one of it as a side effect.
        if (Chests.IsOpen || Dialogue.IsShowing || Vendors.IsOpen || HireOffices.IsOpen || PauseMenu.IsOpen) return;

        var inv = Inventories.Player;
        if (inv == null) return;

        int slot = Hotbar.SelectedSlot;
        var stack = inv.GetSlot(slot);
        if (stack.IsEmpty) return;

        var behavior = stack.item.UseBehavior;
        if (behavior == null) return; // item has no Use behavior assigned

        bool consume = behavior.Use(gameObject, stack);
        if (consume) inv.TryRemoveFromSlot(slot, 1);
    }
}
