using UnityEngine;

// Reads the Drop input and ejects the currently-selected hotbar stack as a
// world-space PickupInteractable.
//
// Wires (Inspector):
//   _inputSource -> the PlayerInputReader (must implement IMovementInput)
//   _dropPrefab  -> a generic "DroppedItem" prefab (SpriteRenderer + trigger
//                   Collider2D + PickupInteractable). The handler clones it
//                   per drop and calls Configure(item, count) on the clone so
//                   one prefab serves every item.
//   _dropOffset  -> world-units offset from the player so the drop doesn't
//                   land right under the body collider (and immediately
//                   re-enters the InteractorZone).
//
// Drop semantics:
//   Q pressed → take ALL of the selected slot, spawn one pickup with that
//   item + count, remove the stack from the player's inventory.
//
// Future polish: "drop one" (Shift+Q), throw arc / bounce VFX, drop position
// based on player facing instead of a flat offset.
public class PlayerDropHandler : MonoBehaviour
{
    [SerializeField] private MonoBehaviour       _inputSource; // IMovementInput
    [SerializeField] private PickupInteractable  _dropPrefab;
    [SerializeField] private Vector2             _dropOffset = new Vector2(0f, -0.5f);

    private IMovementInput _input;

    private void Awake()
    {
        _input = _inputSource as IMovementInput;
        if (_input == null && _inputSource != null)
            Debug.LogError($"{name}: {_inputSource.GetType().Name} does not implement IMovementInput.");
    }

    private void Update()
    {
        if (_input == null || !_input.DropPressed) return;
        if (_dropPrefab == null) return;

        // Same rule as PlayerUseHandler: don't drop while a blocking overlay is
        // active. Q while chest / dialogue / vendor is open should be inert, not
        // throw the player's selected stack onto the ground.
        if (Chests.IsOpen || Dialogue.IsShowing || Vendors.IsOpen || HireOffices.IsOpen) return;

        var inv = Inventories.Player;
        if (inv == null) return;

        int slot = Hotbar.SelectedSlot;
        var stack = inv.GetSlot(slot);
        if (stack.IsEmpty) return;

        // Spawn first so we keep a reference to the stack we're about to clear.
        Vector3 spawnPos = transform.position + (Vector3)_dropOffset;
        var pickup = Instantiate(_dropPrefab, spawnPos, Quaternion.identity);
        pickup.Configure(stack.item, stack.count);

        inv.TryRemoveFromSlot(slot, stack.count);
    }
}
