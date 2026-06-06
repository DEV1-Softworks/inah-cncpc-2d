using UnityEngine;

// A world-side interactable that gates entry behind an authorization flag.
// Plays one conversation when access is denied (the default), and a different
// one when access has been granted.
//
// First use case: the cenote entrance. The player tries to enter and the
// world tells them this is restricted to INAH personnel. Later in the arc,
// once the protagonist has integrated with the research team, story logic
// can flip _accessGranted and the player gets the "granted" conversation —
// from which a SceneTransition or cutscene can take them further.
//
// The "granted" slot is optional. Leave it null until the arc that unlocks
// the place is wired; until then Interact() with access=true safely no-ops.
public class RestrictedAccessInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Played when the player tries to enter without authorization. Required.")]
    [SerializeField] private Conversation _denied;

    [Tooltip("Optional. Played once access has been granted by story progression. Leave empty until that arc is built.")]
    [SerializeField] private Conversation _granted;

    [Tooltip("Whether the player currently has authorization. For the jam this stays false; a future progression system flips it via GrantAccess().")]
    [SerializeField] private bool _accessGranted;

    public bool AccessGranted => _accessGranted;

    public void Interact()
    {
        var toPlay = _accessGranted ? _granted : _denied;
        if (toPlay == null || toPlay.Lines == null || toPlay.Lines.Count == 0) return;

        Dialogue.PlayConversation(toPlay.Lines);
    }

    // Public seams so a future story / progression system can flip access
    // without writing into a private serialized field. Nothing calls these
    // yet — they exist for the day they will.
    public void GrantAccess()  => _accessGranted = true;
    public void RevokeAccess() => _accessGranted = false;
}
