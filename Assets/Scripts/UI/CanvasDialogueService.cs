using TMPro;
using UnityEngine;

// Concrete dialogue service that drives the DialogueUI prefab.
//
// Goes on the DialogueUI prefab ROOT. Wires:
//   _visualRoot  -> the Panel GameObject (the visible dialogue box)
//   _text        -> the TMP_Text child where the message renders
//
// Registers itself with the static Dialogue locator in OnEnable, so every
// interactable in the game can just call Dialogue.Show("...") and reach this.
public class CanvasDialogueService : MonoBehaviour, IDialogueService
{
    [SerializeField] private GameObject _visualRoot; // the dialogue Panel — toggled on/off
    [SerializeField] private TMP_Text _text;         // the message renderer inside the panel

    public bool IsShowing => _visualRoot != null && _visualRoot.activeSelf;

    private void OnEnable()
    {
        Dialogue.Register(this);
        Hide(); // start hidden regardless of how the prefab was authored
    }

    private void OnDisable()
    {
        Dialogue.Unregister(this);
    }

    public void Show(string text)
    {
        if (_text != null) _text.text = text;
        if (_visualRoot != null) _visualRoot.SetActive(true);
    }

    public void Hide()
    {
        if (_visualRoot != null) _visualRoot.SetActive(false);
    }
}
