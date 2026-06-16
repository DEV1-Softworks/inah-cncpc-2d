using System.Collections.Generic;

// Static registry of which expert specialists the player has contracted in
// the current session. Same locator-style pattern as the other game-state
// systems (Wallet, GameTime, etc.).
//
// For the jam this is in-memory only. When a save system arrives it gets
// serialized alongside Wallet, GameTime and the per-NPC story flags.
public static class HiredExperts
{
    private static readonly HashSet<string> _hired = new();

    // Fires AFTER the expert id is added. SceneExpertActivator listens to
    // this to flip on the matching NPC in the world the same frame the
    // player confirms the hire — no scene reload required.
    public static event System.Action<string> OnHired;

    public static bool IsHired(string expertId) =>
        !string.IsNullOrEmpty(expertId) && _hired.Contains(expertId);

    public static void Hire(string expertId)
    {
        if (string.IsNullOrEmpty(expertId)) return;
        if (!_hired.Add(expertId)) return; // already hired — no-op
        OnHired?.Invoke(expertId);
    }
}
