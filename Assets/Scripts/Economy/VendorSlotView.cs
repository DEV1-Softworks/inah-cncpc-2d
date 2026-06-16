using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One slot in the vendor modal. Reusable for both panels:
//   - Sell panel: bound from the player's inventory; price = item.SellPrice;
//                 slotIndex >= 0 so the UI can call TryRemoveFromSlot on click.
//   - Buy panel:  bound from the vendor's stock; price = item.BuyPrice;
//                 slotIndex = -1 (not applicable, vendor stock is infinite).
//
// Wire (Inspector):
//   _icon       -> child Image where the item icon renders
//   _countText  -> child TMP_Text for the inventory count (hidden when 1 or empty)
//   _priceText  -> child TMP_Text for the per-unit price (hidden when empty)
//   _disabledTint -> optional GameObject (an overlay) shown when this slot
//                    can't be acted on (e.g. trying to buy without enough pesos)
//
// Emits OnClicked when the player taps the slot. The owning VendorUI decides
// what that means: sell one unit, buy one unit, or no-op if disabled.
public class VendorSlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image      _icon;
    [SerializeField] private TMP_Text   _countText;
    [SerializeField] private TMP_Text   _priceText;
    [SerializeField] private GameObject _disabledTint;

    public event System.Action<VendorSlotView> OnClicked;

    public Item Item       { get; private set; }
    public int  SlotIndex  { get; private set; } = -1;
    public int  Count      { get; private set; }
    public int  Price      { get; private set; }
    public bool IsEmpty    => Item == null;
    public bool IsDisabled { get; private set; }

    public void SetSell(Item item, int count, int sellPrice, int slotIndex)
    {
        Item       = item;
        Count      = count;
        Price      = sellPrice;
        SlotIndex  = slotIndex;
        IsDisabled = item == null || sellPrice <= 0;
        Repaint();
    }

    public void SetBuy(Item item, int buyPrice, bool affordable)
    {
        Item       = item;
        Count      = 1;
        Price      = buyPrice;
        SlotIndex  = -1;
        IsDisabled = item == null || buyPrice <= 0 || !affordable;
        Repaint();
    }

    public void SetEmpty()
    {
        Item       = null;
        Count      = 0;
        Price      = 0;
        SlotIndex  = -1;
        IsDisabled = true;
        Repaint();
    }

    private void Repaint()
    {
        bool empty = IsEmpty;

        if (_icon != null)
        {
            _icon.enabled = !empty;
            if (!empty) _icon.sprite = Item.Icon;
        }

        if (_countText != null)
        {
            bool showCount = !empty && Count > 1;
            _countText.enabled = showCount;
            if (showCount) _countText.text = Count.ToString();
        }

        if (_priceText != null)
        {
            bool showPrice = !empty && Price > 0;
            _priceText.enabled = showPrice;
            if (showPrice) _priceText.text = $"${Price}";
        }

        if (_disabledTint != null && _disabledTint.activeSelf != IsDisabled)
            _disabledTint.SetActive(IsDisabled);
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (IsDisabled) return;
        OnClicked?.Invoke(this);
    }
}
