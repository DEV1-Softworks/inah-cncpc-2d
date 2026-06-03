using UnityEngine;

// Concrete chest UI: drives a panel showing the chest's slots above the
// player's slots, with clicking a slot transferring its stack to the other
// side.
//
// Goes on the ChestUI prefab ROOT. Wires:
//   _visualRoot   -> the Panel GameObject (the visible UI; toggled on/off)
//   _playerSlots  -> the player's slot views (typically 12, one per player slot)
//   _chestSlots   -> the chest's slot views (typically 12, one per chest slot)
//
// Slots reuse HotbarSlotView (same view, same wiring shape) and emit
// OnClicked; ChestUI matches the clicked view back to its index and runs the
// transfer.
public class ChestUI : MonoBehaviour, IChestUI
{
    [SerializeField] private GameObject        _visualRoot;
    [SerializeField] private HotbarSlotView[]  _playerSlots;
    [SerializeField] private HotbarSlotView[]  _chestSlots;

    private IInventory _chest;
    private IInventory _player;

    public bool IsOpen => _visualRoot != null && _visualRoot.activeSelf;

    private void OnEnable()
    {
        Chests.Register(this);
        Close();
        SubscribeClicks(_playerSlots, HandlePlayerSlotClicked, true);
        SubscribeClicks(_chestSlots,  HandleChestSlotClicked,  true);
    }

    private void OnDisable()
    {
        Chests.Unregister(this);
        SubscribeClicks(_playerSlots, HandlePlayerSlotClicked, false);
        SubscribeClicks(_chestSlots,  HandleChestSlotClicked,  false);
    }

    public void Open(IInventory chestInventory)
    {
        _chest  = chestInventory;
        _player = Inventories.Player;
        if (_chest == null || _player == null) return;

        _chest.OnChanged  += Refresh;
        _player.OnChanged += Refresh;

        if (_visualRoot != null) _visualRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (_chest  != null) _chest.OnChanged  -= Refresh;
        if (_player != null) _player.OnChanged -= Refresh;
        _chest  = null;
        _player = null;
        if (_visualRoot != null) _visualRoot.SetActive(false);
    }

    private void Refresh()
    {
        RefreshGrid(_playerSlots, _player);
        RefreshGrid(_chestSlots,  _chest);
    }

    private static void RefreshGrid(HotbarSlotView[] views, IInventory inv)
    {
        if (views == null) return;
        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] == null) continue;
            views[i].SetStack(inv != null && i < inv.Capacity ? inv.GetSlot(i) : ItemStack.Empty);
            // Selection state is intentionally NOT touched here: the chest UI
            // has no concept of a selected slot, so we leave each view's
            // highlight in whatever state the prefab authored. (The previous
            // version forced it off, which had the side effect of hiding the
            // entire slot when the highlight doubled as the slot's frame.)
        }
    }

    private static void SubscribeClicks(HotbarSlotView[] views,
                                        System.Action<HotbarSlotView> handler,
                                        bool subscribe)
    {
        if (views == null) return;
        for (int i = 0; i < views.Length; i++)
        {
            if (views[i] == null) continue;
            if (subscribe) views[i].OnClicked += handler;
            else           views[i].OnClicked -= handler;
        }
    }

    private static int IndexOf(HotbarSlotView[] views, HotbarSlotView view)
    {
        for (int i = 0; i < views.Length; i++)
            if (views[i] == view) return i;
        return -1;
    }

    private void HandlePlayerSlotClicked(HotbarSlotView view)
    {
        if (_player == null || _chest == null) return;
        int i = IndexOf(_playerSlots, view);
        if (i >= 0) Transfer(_player, _chest, i);
    }

    private void HandleChestSlotClicked(HotbarSlotView view)
    {
        if (_player == null || _chest == null) return;
        int i = IndexOf(_chestSlots, view);
        if (i >= 0) Transfer(_chest, _player, i);
    }

    // Move as much of the slot's stack as the destination will accept; leave
    // any overflow in the source slot.
    private static void Transfer(IInventory from, IInventory to, int slotIndex)
    {
        var stack = from.GetSlot(slotIndex);
        if (stack.IsEmpty) return;
        int added = to.TryAdd(stack.item, stack.count);
        if (added > 0) from.TryRemoveFromSlot(slotIndex, added);
    }
}
