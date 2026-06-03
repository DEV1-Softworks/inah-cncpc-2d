// Read-only view of the hotbar's selection. The player-use code uses this to
// know which slot to act on without depending on the concrete HotbarUI class.
public interface IHotbarUI
{
    int SelectedSlot { get; }
}

// Static locator: the active hotbar registers itself; the rest of the game
// reads Hotbar.SelectedSlot directly. Same singleton-ish pattern as
// Dialogue / Inventories / Chests.
//
// When no hotbar is in the scene (cutscene, splash, etc.), SelectedSlot
// safely returns 0 — callers either find an empty slot or operate on whatever
// happens to be in slot 0; either is harmless for the current systems.
public static class Hotbar
{
    public static IHotbarUI Active { get; private set; }

    public static int SelectedSlot => Active?.SelectedSlot ?? 0;

    public static void Register(IHotbarUI ui) => Active = ui;
    public static void Unregister(IHotbarUI ui)
    {
        if (Active == ui) Active = null;
    }
}
