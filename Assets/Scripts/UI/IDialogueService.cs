// The contract any "dialogue presenter" implements. Callers (interactables,
// cutscenes, NPCs) depend ONLY on this interface and the static Dialogue
// locator — never on the concrete UI. Swap CanvasDialogueService for a
// fancier presenter later (typewriter, portraits, branching) without
// touching a single interactable.
public interface IDialogueService
{
    void Show(string text);
    void Hide();
    bool IsShowing { get; }
}

// Tiny static service-locator: the concrete dialogue service registers itself
// here in OnEnable, and the rest of the game calls Dialogue.Show("...") /
// Dialogue.Hide() without ever needing a serialized reference per caller.
//
// Why not pure DI like IMovementInput? Because dialogue is a *singleton-ish*
// system (one active presenter at a time, app-wide). Wiring a reference into
// every sign in the world would be tedious — and the locator means signs in
// prefabs Just Work the moment a DialogueUI exists in the scene.
public static class Dialogue
{
    public static IDialogueService Active { get; private set; }

    // Events fired AFTER the active service's state actually changes. Anyone
    // that cares about "dialogue is now open / closed" (the player, a cutscene
    // director, audio ducking, etc.) subscribes here instead of polling.
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
        Active.Show(text);
        OnShown?.Invoke(text);
    }

    public static void Hide()
    {
        if (Active == null) return;
        bool wasShowing = Active.IsShowing;
        Active.Hide();
        if (wasShowing) OnHidden?.Invoke(); // only fire on a real transition
    }

    public static bool IsShowing => Active?.IsShowing ?? false;
}
