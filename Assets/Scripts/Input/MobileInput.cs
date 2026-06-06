using UnityEngine;

// Static "one-frame edge" bus for on-screen mobile buttons. Each Press*()
// stamps the current frame number; the matching property returns true ONLY
// during that same frame, mirroring the WasPressedThisFrame semantics that
// PlayerInputReader exposes elsewhere.
//
// Why a static class instead of a per-button event: PlayerInputReader is
// already a static-locator-style aggregator of input intents, and consumers
// (PlayerInteractor, PlayerUseHandler, PlayerDropHandler) only ask "was X
// pressed this frame?". A static one-frame flag is the smallest thing that
// fits that contract from a UI button click.
//
// Trade-off accepted: we bypass the Input System action map for mobile.
// Cleaner long-term path = add Drop / Inventory / Hotbar-slot actions to
// InputSystem_Actions.inputactions and use Unity's OnScreenButton + action
// references. Captured as tech debt for the post-jam rebinding pass.
public static class MobileInput
{
    private static int _interactFrame = -1;
    private static int _useFrame      = -1;
    private static int _dropFrame     = -1;

    public static void PressInteract() => _interactFrame = Time.frameCount;
    public static void PressUse()      => _useFrame      = Time.frameCount;
    public static void PressDrop()     => _dropFrame     = Time.frameCount;

    public static bool InteractPressedThisFrame => _interactFrame == Time.frameCount;
    public static bool UsePressedThisFrame      => _useFrame      == Time.frameCount;
    public static bool DropPressedThisFrame     => _dropFrame     == Time.frameCount;
}
