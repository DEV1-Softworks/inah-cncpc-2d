using UnityEngine;

// A reusable item definition. Right-click in the Project window ->
// Create -> Inventory -> Item to make one.
//
// Items are pure data — name, icon, max stack. Behavior comes from systems
// that consume items (the inventory stacks them, the future hotbar shows
// them, the future "use item" path does something with the selected one).
//
// MaxStack of 1 means non-stackable (tools, unique items); >1 means
// consumables stack up to that count.
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "NewItem")]
public class Item : ScriptableObject
{
    [SerializeField] private string _displayName;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int    _maxStack = 99;

    [Tooltip("Optional. Assign an ItemUseBehavior asset to make pressing Use on this item do something. Leave null for purely decorative / collectible items.")]
    [SerializeField] private ItemUseBehavior _useBehavior;

    public string           DisplayName  => _displayName;
    public string           Description  => _description;
    public Sprite           Icon         => _icon;
    public int              MaxStack     => Mathf.Max(1, _maxStack);
    public ItemUseBehavior  UseBehavior  => _useBehavior;
}
