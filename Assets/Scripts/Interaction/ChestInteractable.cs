using UnityEngine;

// World-side chest: when the player interacts, opens the chest UI with THIS
// chest's inventory.
//
// Sits on a GameObject with a SpriteRenderer + Collider2D (the visible
// chest). A ChestInventory MUST be on the same GameObject — RequireComponent
// auto-adds one if you forget.
[RequireComponent(typeof(ChestInventory))]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    private IInventory _inv;

    private void Awake() => _inv = GetComponent<IInventory>();

    public void Interact()
    {
        if (_inv == null) return;
        Chests.Open(_inv);
    }
}
