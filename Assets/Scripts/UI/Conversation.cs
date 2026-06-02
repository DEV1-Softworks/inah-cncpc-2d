using System.Collections.Generic;
using UnityEngine;

// A reusable, authored conversation asset. Drop one into an NPC's
// `NpcInteractable._conversation` and the NPC plays it through.
//
// Right-click in the Project window -> Create -> Dialogue -> Conversation.
//
// For now this is a LINEAR sequence of DialogueLines (turns). Branching
// choices are the natural next step — when added, this will grow into a
// node-graph where each node carries lines + optional Choice[] edges. The
// `Lines` accessor will stay; the graph version will add a `Choices` shape
// alongside it.
[CreateAssetMenu(menuName = "Dialogue/Conversation", fileName = "NewConversation")]
public class Conversation : ScriptableObject
{
    [SerializeField]
    private DialogueLine[] _lines =
    {
        new DialogueLine { speaker = "NPC", text = "Hi." },
    };

    public IList<DialogueLine> Lines => _lines;
}
