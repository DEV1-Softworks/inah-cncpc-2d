using UnityEngine;

// A single page/beat of dialogue: who's speaking (optional), what they say,
// and — when the conversation branches — what choices the player can pick.
//
// Reused across signs (choices is empty, lines advance linearly), NPC
// conversations (some lines have choices, picking one jumps to a target line
// instead of going to the next index), and future cutscenes.
//
// Struct because it's pure data and often authored as inspector arrays.
[System.Serializable]
public struct DialogueLine
{
    public string speaker;                  // empty -> name plate hides
    [TextArea] public string text;
    public DialogueChoice[] choices;        // empty/null -> linear advance to next index
}

// One selectable response on a branching dialogue line.
// targetLine is an index into the same array that holds this line; -1 ends
// the conversation. (Out-of-range targets also end safely.)
[System.Serializable]
public struct DialogueChoice
{
    public string text;
    public int    targetLine;   // -1 = end conversation; otherwise jump to that index
}
