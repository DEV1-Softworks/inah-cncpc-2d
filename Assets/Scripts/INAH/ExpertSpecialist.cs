using UnityEngine;

// Authored definition of one hireable specialist that the INAH camp can
// contract. Lives in the Project as a ScriptableObject; instances are
// referenced by HireOfficeInteractable._availableExperts and used by the
// HireOfficeUI to render the list / detail panel.
//
// Right-click in the Project window -> Create -> INAH -> Expert Specialist.
[CreateAssetMenu(menuName = "INAH/Expert Specialist", fileName = "NewExpert")]
public class ExpertSpecialist : ScriptableObject
{
    [Tooltip("Stable identifier. Must match the ExpertNpc on the NPC GameObject in the scene. Keep unique across the project.")]
    [SerializeField] private string _expertId;

    [Tooltip("Shown in the list and in the detail header.")]
    [SerializeField] private string _displayName;

    [Tooltip("Specialty title shown under the name (e.g. \"Espeleobuza\", \"Geóloga\").")]
    [SerializeField] private string _specialty;

    [Tooltip("Portrait shown both in the list row and in the detail panel.")]
    [SerializeField] private Sprite _portrait;

    [TextArea(3, 8)]
    [Tooltip("Detail description shown in the right column.")]
    [SerializeField] private string _description;

    [Tooltip("Cost in pesos to hire this specialist. Subtracted from the wallet on confirm.")]
    [SerializeField] private int _hireCost = 2000;

    public string ExpertId    => _expertId;
    public string DisplayName => _displayName;
    public string Specialty   => _specialty;
    public Sprite Portrait    => _portrait;
    public string Description => _description;
    public int    HireCost    => _hireCost;
}
