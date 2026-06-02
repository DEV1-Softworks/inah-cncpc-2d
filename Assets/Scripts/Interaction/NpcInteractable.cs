using UnityEngine;

// World-side NPC interactable: plays its authored Conversation when the
// player interacts. Depends only on Dialogue + the Conversation asset — no
// knowledge of UI internals, choices, or playback bookkeeping. The dialogue
// service owns the playhead.
public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Conversation _conversation;

    public void Interact()
    {
        if (_conversation == null || _conversation.Lines == null || _conversation.Lines.Count == 0)
            return;

        Dialogue.ShowSequence(_conversation.Lines);
    }
}
