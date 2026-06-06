using UnityEngine;
using UnityEngine.InputSystem;

// Drives a row of HotbarSlotView components, kept in sync with the player
// inventory via Inventories.Player.OnChanged. Owns the "currently selected
// slot" concept — future "use selected item" code reads SelectedSlot.
//
// Wire (Inspector):
//   _slots -> the HotbarSlotView components, in display order (slot 0 first).
//             Size the array to match the player inventory capacity (12 by
//             default). If the array is shorter than the inventory, only the
//             first N slots render; that's fine for a small visible hotbar
//             over a larger backpack later.
//
// Selection input: number keys 1-9 select slots 0-8; 0 / - / = select 9 / 10
// / 11. Kept simple by reading the keyboard directly — there is no input
// abstraction for hotbar selection yet (signpost: when we want gamepad
// support and rebinding, this moves into PlayerInputReader and an
// IPlayerInput interface).
public class HotbarUI : MonoBehaviour, IHotbarUI
{
    [SerializeField] private HotbarSlotView[] _slots;

    private IInventory _inv;
    public int SelectedSlot { get; private set; }

    private void OnEnable()  => Hotbar.Register(this);
    private void OnDisable() => Hotbar.Unregister(this);

    private void Start()
    {
        _inv = Inventories.Player;
        if (_inv == null)
        {
            Debug.LogWarning($"{name}: no PlayerInventory in scene — hotbar will stay empty.");
            return;
        }

        _inv.OnChanged += Refresh;

        // Subscribe to slot clicks so tapping a slot selects it. Works for
        // mobile touch and for desktop mouse alike — HotbarSlotView already
        // fires OnClicked via IPointerClickHandler.
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;
            int captured = i; // capture loop var for the lambda
            _slots[i].OnClicked += _ => SetSelectedSlot(captured);
        }

        Refresh();
        SetSelectedSlot(0); // highlight slot 0 by default
    }

    private void OnDestroy()
    {
        if (_inv != null) _inv.OnChanged -= Refresh;
    }

    private void Update()
    {
        int pressed = ReadSlotKey();
        if (pressed >= 0) SetSelectedSlot(pressed);
    }

    public void SetSelectedSlot(int index)
    {
        if (_slots == null || _slots.Length == 0) return;
        if (index < 0 || index >= _slots.Length) return;

        if (_slots[SelectedSlot] != null) _slots[SelectedSlot].SetSelected(false);
        SelectedSlot = index;
        if (_slots[SelectedSlot] != null) _slots[SelectedSlot].SetSelected(true);
    }

    private void Refresh()
    {
        if (_inv == null || _slots == null) return;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) continue;
            _slots[i].SetStack(_inv.GetSlot(i));
        }
    }

    // -1 if no slot key was pressed this frame. New Input System direct read —
    // see signpost in the class comment.
    private static int ReadSlotKey()
    {
        var kb = Keyboard.current;
        if (kb == null) return -1;
        if (kb.digit1Key.wasPressedThisFrame) return 0;
        if (kb.digit2Key.wasPressedThisFrame) return 1;
        if (kb.digit3Key.wasPressedThisFrame) return 2;
        if (kb.digit4Key.wasPressedThisFrame) return 3;
        if (kb.digit5Key.wasPressedThisFrame) return 4;
        if (kb.digit6Key.wasPressedThisFrame) return 5;
        if (kb.digit7Key.wasPressedThisFrame) return 6;
        if (kb.digit8Key.wasPressedThisFrame) return 7;
        if (kb.digit9Key.wasPressedThisFrame) return 8;
        if (kb.digit0Key.wasPressedThisFrame) return 9;
        if (kb.minusKey.wasPressedThisFrame)  return 10;
        if (kb.equalsKey.wasPressedThisFrame) return 11;
        return -1;
    }
}
