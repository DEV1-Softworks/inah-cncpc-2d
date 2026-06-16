using UnityEngine;

// The player's "brain": it OWNS the current PlayerState and answers the one
// question PlayerMotor cares about — "is the player allowed to move right now?"
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour, IMovementGate
{
    // Anyone can READ the state (public getter)...
    // ...but only this component can CHANGE it (private setter).
    // This "single owner" rule keeps state changes traceable.
    public PlayerState State { get; private set; } = PlayerState.Free;

    // IMovementGate contract: the motor pauses movement whenever this is false.
    // The player moves only under normal control; tool swings and events lock it.
    public bool CanMove => State == PlayerState.Free;

    // The ONLY door for changing state. Funnelling every change through one
    // method is what lets you later add transition rules, fire events, play
    // sounds, etc. — all in one place instead of scattered everywhere.
    public void SetState(PlayerState next)
    {
        if (State == next) return; // ignore no-op changes
        State = next;
    }

    // While any blocking UI is open (dialogue, chest, future: shop / pause /
    // map), lock the player into InEvent so CanMove returns false and the
    // motor zeroes velocity. Subscribing via events keeps the brain
    // authoritative over its own state — no system calls SetState directly.
    private void OnEnable()
    {
        Dialogue.OnShown      += HandleDialogueShown;
        Dialogue.OnHidden     += HandleOverlayClosed;
        Chests.OnOpened       += HandleOverlayOpened;
        Chests.OnClosed       += HandleOverlayClosed;
        Vendors.OnOpened      += HandleOverlayOpened;
        Vendors.OnClosed      += HandleOverlayClosed;
        HireOffices.OnOpened  += HandleOverlayOpened;
        HireOffices.OnClosed  += HandleOverlayClosed;
    }

    private void OnDisable()
    {
        Dialogue.OnShown      -= HandleDialogueShown;
        Dialogue.OnHidden     -= HandleOverlayClosed;
        Chests.OnOpened       -= HandleOverlayOpened;
        Chests.OnClosed       -= HandleOverlayClosed;
        Vendors.OnOpened      -= HandleOverlayOpened;
        Vendors.OnClosed      -= HandleOverlayClosed;
        HireOffices.OnOpened  -= HandleOverlayOpened;
        HireOffices.OnClosed  -= HandleOverlayClosed;
    }

    // Dialogue passes the first line's text; we ignore it and just lock.
    private void HandleDialogueShown(string _) => SetState(PlayerState.InEvent);
    private void HandleOverlayOpened()         => SetState(PlayerState.InEvent);
    private void HandleOverlayClosed()         => SetState(PlayerState.Free);
}
