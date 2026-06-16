using System.Collections.Generic;
using UnityEngine;

// World-side interactable inside the INAH office (or wherever you site the
// hire desk). When triggered, opens the HireOfficeUI with the configured
// roster of available specialists.
//
// The same modal can serve multiple hire desks if you scale up later
// (different desks could offer different subsets of experts), but for the
// jam one desk with all experts is enough.
public class HireOfficeInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Title shown in the modal header.")]
    [SerializeField] private string _officeName = "Oficina del INAH";

    [Tooltip("Roster of specialists available at this desk. Order is the order shown in the list.")]
    [SerializeField] private List<ExpertSpecialist> _availableExperts = new();

    public void Interact()
    {
        var session = new HireOfficeSession
        {
            Title   = _officeName,
            Experts = _availableExperts,
        };
        HireOffices.Open(session);
    }
}
