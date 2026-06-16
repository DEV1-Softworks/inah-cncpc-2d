using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Concrete hire desk modal. Two columns: a list of available specialists on
// the left, a detail panel + Hire CTA on the right. Live wallet + status.
// Place once per scene (or under Persistent so it survives transitions).
//
// Wires (Inspector):
//   _visualRoot           -> the Panel child GameObject. Toggled active/inactive.
//
//   Header:
//   _titleText            -> TMP_Text for the modal title
//   _walletText           -> TMP_Text for the player's pesos (live)
//
//   Left column (list):
//   _listSlots            -> pre-created ExpertListItemView rows. Excess slots
//                            auto-hide. Size to the max expected roster.
//
//   Right column (detail):
//   _detailRoot           -> container shown when an expert is selected
//   _detailEmptyRoot      -> placeholder shown before any selection
//   _detailPortrait       -> Image for the large portrait
//   _detailName           -> TMP_Text for the name
//   _detailSpecialty      -> TMP_Text for the specialty
//   _detailDescription    -> TMP_Text for the long description
//   _detailCost           -> TMP_Text for the cost ("$2,000")
//   _hireButton           -> Unity UI Button. Wire its onClick to HireSelected().
//   _hireButtonLabel      -> TMP_Text label inside the button (e.g. "Contratar")
//   _hiredButtonLabel     -> text shown when the selected expert is already hired
public class HireOfficeUI : MonoBehaviour, IHireOfficeUI
{
    [Header("Root & header")]
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private TMP_Text   _titleText;
    [SerializeField] private TMP_Text   _walletText;

    [Header("Left column — list")]
    [SerializeField] private ExpertListItemView[] _listSlots;

    [Header("Right column — detail")]
    [SerializeField] private GameObject _detailRoot;
    [SerializeField] private GameObject _detailEmptyRoot;
    [SerializeField] private Image      _detailPortrait;
    [SerializeField] private TMP_Text   _detailName;
    [SerializeField] private TMP_Text   _detailSpecialty;
    [SerializeField] private TMP_Text   _detailDescription;
    [SerializeField] private TMP_Text   _detailCost;
    [SerializeField] private Button     _hireButton;
    [SerializeField] private TMP_Text   _hireButtonLabel;

    [Header("Copy")]
    [SerializeField] private string _hireLabel  = "Contratar";
    [SerializeField] private string _hiredLabel = "Contratado";

    private HireOfficeSession _session;
    private ExpertSpecialist  _selected;
    private bool              _wiredSlots;

    public bool IsOpen => _visualRoot != null && _visualRoot.activeSelf;

    private void OnEnable()
    {
        HireOffices.Register(this);
        Close();
    }

    private void OnDisable() => HireOffices.Unregister(this);

    public void Open(HireOfficeSession session)
    {
        if (session == null) return;

        _session  = session;
        _selected = null;

        WireSlotsOnce();

        if (_titleText != null) _titleText.text = session.Title;

        BindList();
        ShowEmptyDetail();

        Wallet.OnChanged       += HandleWalletChanged;
        HiredExperts.OnHired   += HandleHired;

        RefreshWallet();

        if (_visualRoot != null) _visualRoot.SetActive(true);
    }

    public void Close()
    {
        Wallet.OnChanged     -= HandleWalletChanged;
        HiredExperts.OnHired -= HandleHired;

        _session  = null;
        _selected = null;
        if (_visualRoot != null) _visualRoot.SetActive(false);
    }

    // -------- Hire (called from the Hire Button's onClick in the Inspector) --------

    public void HireSelected()
    {
        if (_selected == null) return;
        if (HiredExperts.IsHired(_selected.ExpertId)) return;
        if (!Wallet.TrySpend(_selected.HireCost)) return;

        HiredExperts.Hire(_selected.ExpertId);
        // Refresh paths handle the rest:
        //   HiredExperts.OnHired -> HandleHired -> RefreshList + RefreshDetail
        //   Wallet.OnChanged     -> HandleWalletChanged -> RefreshWallet + RefreshDetailAffordability
    }

    // -------- Wiring & subscriptions --------

    private void WireSlotsOnce()
    {
        if (_wiredSlots || _listSlots == null) return;
        for (int i = 0; i < _listSlots.Length; i++)
        {
            if (_listSlots[i] == null) continue;
            _listSlots[i].OnClicked += HandleSlotClicked;
        }
        _wiredSlots = true;
    }

    private void HandleSlotClicked(ExpertListItemView slot)
    {
        Select(slot.Expert);
    }

    private void HandleWalletChanged(int _) { RefreshWallet(); RefreshDetailAffordability(); }

    private void HandleHired(string expertId)
    {
        // Status text on the list rows + button state on the detail panel.
        RefreshList();
        if (_selected != null && _selected.ExpertId == expertId)
            RefreshDetail();
    }

    // -------- Selection --------

    private void Select(ExpertSpecialist expert)
    {
        _selected = expert;

        // Highlight in the list.
        if (_listSlots != null)
        {
            for (int i = 0; i < _listSlots.Length; i++)
            {
                if (_listSlots[i] == null || _listSlots[i].IsEmpty) continue;
                _listSlots[i].SetSelected(_listSlots[i].Expert == expert);
            }
        }

        RefreshDetail();
    }

    // -------- Repaint --------

    private void BindList()
    {
        if (_listSlots == null) return;

        var experts = _session?.Experts;

        for (int i = 0; i < _listSlots.Length; i++)
        {
            if (_listSlots[i] == null) continue;

            if (experts == null || i >= experts.Count || experts[i] == null)
            {
                _listSlots[i].SetEmpty();
                continue;
            }

            _listSlots[i].gameObject.SetActive(true);
            _listSlots[i].Bind(experts[i]);
            _listSlots[i].SetSelected(false);
        }
    }

    private void RefreshList()
    {
        if (_listSlots == null) return;
        for (int i = 0; i < _listSlots.Length; i++)
        {
            if (_listSlots[i] == null || _listSlots[i].IsEmpty) continue;
            _listSlots[i].RefreshStatus();
        }
    }

    private void ShowEmptyDetail()
    {
        if (_detailRoot      != null) _detailRoot.SetActive(false);
        if (_detailEmptyRoot != null) _detailEmptyRoot.SetActive(true);
    }

    private void RefreshDetail()
    {
        if (_selected == null) { ShowEmptyDetail(); return; }

        if (_detailRoot      != null) _detailRoot.SetActive(true);
        if (_detailEmptyRoot != null) _detailEmptyRoot.SetActive(false);

        if (_detailPortrait != null)
        {
            _detailPortrait.enabled = _selected.Portrait != null;
            if (_detailPortrait.enabled) _detailPortrait.sprite = _selected.Portrait;
        }

        if (_detailName        != null) _detailName.text        = _selected.DisplayName;
        if (_detailSpecialty   != null) _detailSpecialty.text   = _selected.Specialty;
        if (_detailDescription != null) _detailDescription.text = _selected.Description;
        if (_detailCost        != null) _detailCost.text        = $"${_selected.HireCost}";

        RefreshDetailAffordability();
    }

    private void RefreshDetailAffordability()
    {
        if (_selected == null || _hireButton == null) return;

        bool hired      = HiredExperts.IsHired(_selected.ExpertId);
        bool affordable = Wallet.Pesos >= _selected.HireCost;

        _hireButton.interactable = !hired && affordable;

        if (_hireButtonLabel != null)
            _hireButtonLabel.text = hired ? _hiredLabel : _hireLabel;
    }

    private void RefreshWallet()
    {
        if (_walletText != null) _walletText.text = $"${Wallet.Pesos}";
    }
}
