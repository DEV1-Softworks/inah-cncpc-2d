using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One slot view. Tiny dumb component: the owning UI tells it which ItemStack
// to draw and whether it's selected; the view emits OnClicked when the player
// clicks it. The hotbar ignores the click event; the chest UI listens for it.
//
// Wire (Inspector):
//   _icon                -> child Image where the item icon renders
//   _countText           -> child TMP_Text for the count number (auto-hidden
//                           when the slot is empty or holds exactly 1)
//   _selectionHighlight  -> optional child GameObject (a brighter frame, a
//                           tinted overlay, whatever) that's only active
//                           while this slot is the selected one
//
// For clicks to register, this GameObject (or one of its children) must have
// a Graphic component with Raycast Target ON. The slot's background Image
// satisfies that by default.
public class HotbarSlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image      _icon;
    [SerializeField] private TMP_Text   _countText;
    [SerializeField] private GameObject _selectionHighlight;

    // Fired when the player clicks this slot. The owning UI passes itself as
    // the sender so it can match the view back to a slot index.
    public event System.Action<HotbarSlotView> OnClicked;

    public void SetStack(ItemStack stack)
    {
        bool empty = stack.IsEmpty;

        if (_icon != null)
        {
            _icon.enabled = !empty;
            if (!empty) _icon.sprite = stack.item.Icon;
        }

        if (_countText != null)
        {
            // Convention: hide the count for empty slots and stacks of 1, so
            // tools / single-stack items don't read "Axe x1" everywhere.
            bool showCount = !empty && stack.count > 1;
            _countText.enabled = showCount;
            if (showCount) _countText.text = stack.count.ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        if (_selectionHighlight != null) _selectionHighlight.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
