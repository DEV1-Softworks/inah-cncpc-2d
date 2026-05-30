using UnityEngine;

// A signpost / readable object: when interacted with, asks the dialogue
// service to show a message. Owns its own message per-instance, depends only
// on the static Dialogue locator — it doesn't know which concrete UI is
// presenting the text. Replace with NpcInteractable, ChestInteractable, etc.
// as those systems come online; the player-side code never changes.
public class TextInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, TextArea] private string _message = "Hello.";

    public void Interact()
    {
        Dialogue.Show(_message);
    }
}
