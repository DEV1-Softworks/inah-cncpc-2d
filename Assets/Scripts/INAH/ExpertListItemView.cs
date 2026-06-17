using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One row in the hire desk's left column. Renders a portrait, the
// specialist's name, and a status badge ("Disponible" / "Contratado").
// Emits OnClicked so the HireOfficeUI selects this expert for the detail
// panel on the right.
//
// Wire (Inspector):
//   _portrait        -> child Image where the expert's portrait renders
//   _nameText        -> child TMP_Text for the display name
//   _specialtyText   -> child TMP_Text for the specialty (e.g. "Espeleobuza")
//   _statusText      -> child TMP_Text for the live status
//   _selectedFrame   -> optional child shown when this row is the selected one
public class ExpertListItemView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image      _portrait;
    [SerializeField] private TMP_Text   _nameText;
    [SerializeField] private TMP_Text   _specialtyText;
    [SerializeField] private TMP_Text   _statusText;
    [SerializeField] private TMP_Text   _costText;
    [SerializeField] private GameObject _selectedFrame;

    [Tooltip("Format string for the cost in the list. {0} is the amount.")]
    [SerializeField] private string _costFormat = "${0:N0}";

    [SerializeField] private string _statusAvailable = "Disponible";
    [SerializeField] private string _statusHired     = "Contratado";

    public ExpertSpecialist Expert { get; private set; }
    public bool IsEmpty => Expert == null;

    public event System.Action<ExpertListItemView> OnClicked;

    public void Bind(ExpertSpecialist expert)
    {
        Expert = expert;

        if (_portrait != null)
        {
            _portrait.enabled = expert.Portrait != null;
            if (_portrait.enabled) _portrait.sprite = expert.Portrait;
        }

        if (_nameText      != null) _nameText.text      = expert.DisplayName;
        if (_specialtyText != null) _specialtyText.text = expert.Specialty;
        if (_costText      != null) _costText.text      = string.Format(_costFormat, expert.HireCost);

        RefreshStatus();
    }

    public void SetEmpty()
    {
        Expert = null;
        if (_portrait      != null) _portrait.enabled  = false;
        if (_nameText      != null) _nameText.text     = "";
        if (_specialtyText != null) _specialtyText.text = "";
        if (_statusText    != null) _statusText.text   = "";
        if (_costText      != null) _costText.text     = "";
        SetSelected(false);
        // Hide the whole row when empty so the list looks clean with fewer
        // specialists than slots.
        gameObject.SetActive(false);
    }

    public void RefreshStatus()
    {
        if (Expert == null || _statusText == null) return;
        bool hired = HiredExperts.IsHired(Expert.ExpertId);
        _statusText.text = hired ? _statusHired : _statusAvailable;
    }

    public void SetSelected(bool selected)
    {
        if (_selectedFrame != null) _selectedFrame.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (Expert == null) return;
        OnClicked?.Invoke(this);
    }
}
