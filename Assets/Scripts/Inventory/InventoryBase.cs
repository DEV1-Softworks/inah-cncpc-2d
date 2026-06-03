using UnityEngine;

// Shared implementation of IInventory for fixed-size slot inventories.
// PlayerInventory and ChestInventory both inherit this; the only thing the
// derived classes add is their own lifecycle hooks (e.g. registering with the
// Inventories.Player locator).
//
// Why a base class instead of a static helper? The OnChanged event needs a
// per-instance owner, Unity serializes `_capacity` cleanly on a class, and the
// instance-method shape matches how callers use the interface.
public abstract class InventoryBase : MonoBehaviour, IInventory
{
    [SerializeField] private int _capacity = 12;

    private ItemStack[] _slots;

    public int Capacity => _slots?.Length ?? 0;

    public event System.Action OnChanged;

    protected virtual void Awake()
    {
        _slots = new ItemStack[Mathf.Max(0, _capacity)];
    }

    public ItemStack GetSlot(int index)
    {
        if (_slots == null || index < 0 || index >= _slots.Length) return ItemStack.Empty;
        return _slots[index];
    }

    public int TryAdd(Item item, int count)
    {
        if (item == null || count <= 0 || _slots == null) return 0;

        int added = 0;
        int max = item.MaxStack;

        // 1) Top up existing stacks of the same item first.
        for (int i = 0; i < _slots.Length && added < count; i++)
        {
            if (_slots[i].item != item || _slots[i].count >= max) continue;
            int room = max - _slots[i].count;
            int put  = Mathf.Min(room, count - added);
            _slots[i].count += put;
            added += put;
        }

        // 2) Then fill empty slots, creating new stacks.
        for (int i = 0; i < _slots.Length && added < count; i++)
        {
            if (!_slots[i].IsEmpty) continue;
            int put = Mathf.Min(max, count - added);
            _slots[i] = new ItemStack(item, put);
            added += put;
        }

        if (added > 0) OnChanged?.Invoke();
        return added;
    }

    public int TryRemove(Item item, int count)
    {
        if (item == null || count <= 0 || _slots == null) return 0;

        int removed = 0;
        for (int i = 0; i < _slots.Length && removed < count; i++)
        {
            if (_slots[i].item != item) continue;
            int take = Mathf.Min(_slots[i].count, count - removed);
            _slots[i].count -= take;
            removed += take;
            if (_slots[i].count <= 0) _slots[i] = ItemStack.Empty;
        }

        if (removed > 0) OnChanged?.Invoke();
        return removed;
    }

    public int TryRemoveFromSlot(int slotIndex, int count)
    {
        if (count <= 0 || _slots == null) return 0;
        if (slotIndex < 0 || slotIndex >= _slots.Length) return 0;
        if (_slots[slotIndex].IsEmpty) return 0;

        int take = Mathf.Min(_slots[slotIndex].count, count);
        _slots[slotIndex].count -= take;
        if (_slots[slotIndex].count <= 0) _slots[slotIndex] = ItemStack.Empty;
        if (take > 0) OnChanged?.Invoke();
        return take;
    }

    public bool HasSpace(Item item, int count)
    {
        if (item == null || count <= 0 || _slots == null) return false;

        int max  = item.MaxStack;
        int room = 0;
        for (int i = 0; i < _slots.Length && room < count; i++)
        {
            if (_slots[i].IsEmpty)            room += max;
            else if (_slots[i].item == item)  room += (max - _slots[i].count);
        }
        return room >= count;
    }
}
