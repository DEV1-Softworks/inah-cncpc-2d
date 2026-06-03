using UnityEngine;

// TEMPORARY: prints the player inventory contents to the Console every time
// it changes. Drop on the Player while testing Phase 1; remove (or disable)
// once the hotbar UI lands in Phase 2.
//
// Subscribing happens in Start (not OnEnable) so PlayerInventory has had a
// chance to register itself with Inventories.Player first.
public class InventoryDebugLogger : MonoBehaviour
{
    private IInventory _inv;

    private void Start()
    {
        _inv = Inventories.Player;
        if (_inv == null)
        {
            Debug.LogWarning($"{name}: no PlayerInventory found in scene.");
            return;
        }
        _inv.OnChanged += LogState;
        LogState();
    }

    private void OnDestroy()
    {
        if (_inv != null) _inv.OnChanged -= LogState;
    }

    private void LogState()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("[Inv]");
        bool any = false;
        for (int i = 0; i < _inv.Capacity; i++)
        {
            var s = _inv.GetSlot(i);
            if (s.IsEmpty) continue;
            sb.Append($" [{i}]{s.item.DisplayName}×{s.count}");
            any = true;
        }
        if (!any) sb.Append(" (empty)");
        Debug.Log(sb.ToString());
    }
}
