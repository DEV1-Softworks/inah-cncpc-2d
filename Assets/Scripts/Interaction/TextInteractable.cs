using System.Collections.Generic;
using UnityEngine;

// A signpost / readable world object: when interacted with, plays a sequence
// of text pages through the dialogue service. Each page is its own "Interact
// to continue" beat; after the last page the dialogue closes automatically.
//
// _speaker is an OPTIONAL title — e.g. a sign about a building can set
// "Bakery" so the dialogue name-plate reads "Bakery"; leave empty for a plain
// unnamed sign. Every page uses the same speaker (signs don't switch mid-text).
//
// Depends only on the static Dialogue locator — does not know which concrete
// UI is presenting the text.
public class TextInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string _speaker = "";
    [SerializeField, TextArea] private string[] _pages = { "Hello." };

    // Reused list so each Interact() doesn't allocate a fresh DialogueLine[].
    private readonly List<DialogueLine> _buffer = new();

    public void Interact()
    {
        if (_pages == null || _pages.Length == 0) return;

        _buffer.Clear();
        for (int i = 0; i < _pages.Length; i++)
            _buffer.Add(new DialogueLine { speaker = _speaker, text = _pages[i] });

        Dialogue.ShowSequence(_buffer);
    }
}
