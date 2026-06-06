using UnityEngine;

// World-side NPC interactable: plays its authored Conversation when the
// player interacts. Depends only on Dialogue + the Conversation asset — no
// knowledge of UI internals, choices, or playback bookkeeping. The dialogue
// service owns the playhead.
//
// First-encounter support: an optional second Conversation may be wired into
// _firstEncounter. The first time the player interacts, that one plays; every
// time after, _conversation plays. Leave _firstEncounter empty for a stable
// NPC whose line never changes.
//
// _hasSpokenBefore lives in memory only — once a save system exists it will
// be serialized along with the rest of per-NPC world state.
public class NpcInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Optional. Played the first time the player interacts. Leave empty to always play the default conversation.")]
    [SerializeField] private Conversation _firstEncounter;

    [Tooltip("Played on every interaction after the first (or every interaction, if no first-encounter is set).")]
    [SerializeField] private Conversation _conversation;

    private bool _hasSpokenBefore;

    public void Interact()
    {
        // First-encounter takes precedence the first time and only the first time.
        Conversation toPlay = (!_hasSpokenBefore && _firstEncounter != null)
            ? _firstEncounter
            : _conversation;

        if (toPlay == null || toPlay.Lines == null || toPlay.Lines.Count == 0)
            return;

        Dialogue.PlayConversation(toPlay.Lines);
        _hasSpokenBefore = true;
    }
}
