using System.Collections.Generic;

// The contract any "dialogue presenter" implements. Callers (interactables,
// cutscenes, NPCs) depend ONLY on this interface and the static Dialogue
// locator — never on the concrete UI. Swap CanvasDialogueService for a
// fancier presenter later (typewriter, portraits, branching) without
// touching a single interactable.
public interface IDialogueService
{
    // One-off caption. Equivalent to ShowSequence with a single unnamed line.
    void Show(string text);

    // Start a paginated sequence. A no-choice line ADVANCES TO THE NEXT INDEX
    // when the player presses Advance — use this for signs and captions.
    void ShowSequence(IList<DialogueLine> lines);

    // Start a conversation graph. A no-choice line is a LEAF — pressing Advance
    // on it ENDS the conversation instead of falling through to the next
    // array index. Use this for NPC dialogues built from a Conversation asset.
    void PlayConversation(IList<DialogueLine> lines);

    // Move to the next line, OR — if the current line has choices and one is
    // selected — jump to that choice's target. Behavior on a no-choice line
    // depends on whether playback was started with ShowSequence or
    // PlayConversation (see above).
    void Advance();

    // Navigate up/down through the choices on the current line. No-op when
    // the current line has no choices.
    void MoveSelection(int delta);

    // Force-close the dialogue regardless of where we are in the sequence.
    void Hide();

    bool IsShowing { get; }

    // True when the current line offers choices that the player must pick.
    // PlayerInteractor uses this to decide whether to read Up/Down as choice
    // navigation vs ignore it.
    bool IsAwaitingChoice { get; }
}

// Static service-locator: the concrete service registers itself here in
// OnEnable, and the rest of the game calls Dialogue.* without ever needing a
// serialized reference per caller. See `CanvasDialogueService`.
public static class Dialogue
{
    public static IDialogueService Active { get; private set; }

    // Events fire ONLY on real transitions:
    //   OnShown  -> hidden -> showing
    //   OnHidden -> showing -> hidden
    // So advancing through a sequence — or making a choice — does NOT re-fire
    // OnShown. Subscribers like PlayerController treat dialogue as one event.
    public static event System.Action<string> OnShown;
    public static event System.Action OnHidden;

    public static void Register(IDialogueService service) => Active = service;
    public static void Unregister(IDialogueService service)
    {
        if (Active == service) Active = null;
    }

    public static void Show(string text)
    {
        if (Active == null) return;
        bool wasShowing = Active.IsShowing;
        Active.Show(text);
        if (!wasShowing && Active.IsShowing) OnShown?.Invoke(text);
    }

    public static void ShowSequence(IList<DialogueLine> lines)
    {
        if (Active == null || lines == null || lines.Count == 0) return;
        bool wasShowing = Active.IsShowing;
        Active.ShowSequence(lines);
        if (!wasShowing && Active.IsShowing) OnShown?.Invoke(lines[0].text);
    }

    public static void PlayConversation(IList<DialogueLine> lines)
    {
        if (Active == null || lines == null || lines.Count == 0) return;
        bool wasShowing = Active.IsShowing;
        Active.PlayConversation(lines);
        if (!wasShowing && Active.IsShowing) OnShown?.Invoke(lines[0].text);
    }

    public static void Advance()
    {
        if (Active == null) return;
        bool wasShowing = Active.IsShowing;
        Active.Advance();
        if (wasShowing && !Active.IsShowing) OnHidden?.Invoke();
    }

    public static void MoveSelection(int delta)
    {
        Active?.MoveSelection(delta);
    }

    public static void Hide()
    {
        if (Active == null) return;
        bool wasShowing = Active.IsShowing;
        Active.Hide();
        if (wasShowing) OnHidden?.Invoke();
    }

    public static bool IsShowing       => Active?.IsShowing       ?? false;
    public static bool IsAwaitingChoice => Active?.IsAwaitingChoice ?? false;
}
