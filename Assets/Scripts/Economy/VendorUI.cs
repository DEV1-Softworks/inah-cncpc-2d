using TMPro;
using UnityEngine;

// Concrete vendor presenter. Goes on the root of a VendorUI prefab placed
// once per scene (or, preferably, under Persistent so it survives transitions).
//
// Wires (Inspector):
//   _visualRoot     -> the Panel child GameObject. The script toggles its
//                      activeSelf to show/hide; do NOT put this script on the
//                      Panel itself (it would deactivate its own GameObject).
//   _headerText     -> TMP_Text in the header showing the vendor's name.
//   _walletText     -> TMP_Text in the header showing the player's pesos.
//                      Updates live during the session.
//   _sellPanel      -> container holding the sell-side slots. Hidden when the
//                      vendor doesn't allow selling.
//   _buyPanel       -> container holding the buy-side slots. Hidden when the
//                      vendor doesn't allow buying.
//   _sellSlots      -> VendorSlotView components in the sell panel. Size the
//                      array to the player inventory capacity.
//   _buySlots       -> VendorSlotView components in the buy panel. Size to
//                      the expected max stock; extras auto-hide.
//
// Lifecycle:
//   - Open(session) configures the panels for the current vendor and shows
//     the root.
//   - Each click on a sell slot consumes one unit, credits SellPrice pesos.
//   - Each click on a buy slot deducts BuyPrice pesos and adds one unit to
//     the player's inventory (if it fits and they can afford it).
//   - Close hides the root; PlayerController re-enables movement via the
//     Vendors.OnClosed event.
public class VendorUI : MonoBehaviour, IVendorUI
{
    [Header("Root & header")]
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private TMP_Text   _headerText;
    [SerializeField] private TMP_Text   _walletText;

    [Header("Sell side")]
    [SerializeField] private GameObject        _sellPanel;
    [SerializeField] private VendorSlotView[]  _sellSlots;

    [Header("Buy side")]
    [SerializeField] private GameObject        _buyPanel;
    [SerializeField] private VendorSlotView[]  _buySlots;

    private VendorSession _session;
    private IInventory    _playerInv;
    private bool          _wiredSlots;

    public bool IsOpen => _visualRoot != null && _visualRoot.activeSelf;

    private void OnEnable()
    {
        Vendors.Register(this);
        Close(); // start hidden
    }

    private void OnDisable() => Vendors.Unregister(this);

    public void Open(VendorSession session)
    {
        if (session == null) return;

        _session   = session;
        _playerInv = Inventories.Player;

        WireSlotsOnce();

        if (_headerText != null) _headerText.text = session.VendorName;

        if (_sellPanel != null) _sellPanel.SetActive(session.CanSell);
        if (_buyPanel  != null) _buyPanel.SetActive(session.CanBuy);

        Subscribe();
        Refresh();

        if (_visualRoot != null) _visualRoot.SetActive(true);
    }

    public void Close()
    {
        Unsubscribe();
        _session = null;
        if (_visualRoot != null) _visualRoot.SetActive(false);
    }

    // -------- Wiring & subscriptions --------

    private void WireSlotsOnce()
    {
        if (_wiredSlots) return;

        if (_sellSlots != null)
        {
            for (int i = 0; i < _sellSlots.Length; i++)
            {
                if (_sellSlots[i] == null) continue;
                _sellSlots[i].OnClicked += HandleSellClicked;
            }
        }

        if (_buySlots != null)
        {
            for (int i = 0; i < _buySlots.Length; i++)
            {
                if (_buySlots[i] == null) continue;
                _buySlots[i].OnClicked += HandleBuyClicked;
            }
        }

        _wiredSlots = true;
    }

    private void Subscribe()
    {
        Wallet.OnChanged += HandleWalletChanged;
        if (_playerInv != null) _playerInv.OnChanged += Refresh;
    }

    private void Unsubscribe()
    {
        Wallet.OnChanged -= HandleWalletChanged;
        if (_playerInv != null) _playerInv.OnChanged -= Refresh;
    }

    // -------- Click handlers --------

    private void HandleSellClicked(VendorSlotView slot)
    {
        if (_session == null || !_session.CanSell || _playerInv == null) return;
        if (slot.SlotIndex < 0 || slot.Item == null) return;

        int removed = _playerInv.TryRemoveFromSlot(slot.SlotIndex, 1);
        if (removed <= 0) return;

        Wallet.Add(slot.Price * removed);
        // Refresh fires automatically via _playerInv.OnChanged. Wallet HUD
        // updates via Wallet.OnChanged.
    }

    private void HandleBuyClicked(VendorSlotView slot)
    {
        if (_session == null || !_session.CanBuy || _playerInv == null) return;
        if (slot.Item == null) return;
        if (!_playerInv.HasSpace(slot.Item, 1)) return;

        if (!Wallet.TrySpend(slot.Price)) return;

        int added = _playerInv.TryAdd(slot.Item, 1);
        if (added <= 0)
        {
            // Inventory rejected the add after we spent — refund. Shouldn't
            // happen given the HasSpace check above, but defensive.
            Wallet.Add(slot.Price);
        }
        // Refresh on inventory change.
    }

    // -------- Repaint --------

    private void HandleWalletChanged(int newBalance)
    {
        if (_walletText != null) _walletText.text = $"${newBalance}";
        // Buy-side affordability may have flipped; refresh slot states.
        if (_session != null && _session.CanBuy) RefreshBuySide();
    }

    private void Refresh()
    {
        if (_walletText != null) _walletText.text = $"${Wallet.Pesos}";
        if (_session == null) return;

        if (_session.CanSell) RefreshSellSide();
        if (_session.CanBuy)  RefreshBuySide();
    }

    private void RefreshSellSide()
    {
        if (_sellSlots == null || _playerInv == null) return;

        int capacity = _playerInv.Capacity;
        for (int i = 0; i < _sellSlots.Length; i++)
        {
            if (_sellSlots[i] == null) continue;

            if (i >= capacity)
            {
                _sellSlots[i].SetEmpty();
                continue;
            }

            var stack = _playerInv.GetSlot(i);
            if (stack.IsEmpty)
                _sellSlots[i].SetEmpty();
            else
                _sellSlots[i].SetSell(stack.item, stack.count, stack.item.SellPrice, i);
        }
    }

    private void RefreshBuySide()
    {
        if (_buySlots == null) return;
        var stock = _session?.Stock;

        for (int i = 0; i < _buySlots.Length; i++)
        {
            if (_buySlots[i] == null) continue;

            if (stock == null || i >= stock.Count || stock[i] == null)
            {
                _buySlots[i].SetEmpty();
                continue;
            }

            var item       = stock[i];
            int price      = item.BuyPrice;
            bool affordable = Wallet.Pesos >= price;
            _buySlots[i].SetBuy(item, price, affordable);
        }
    }
}
