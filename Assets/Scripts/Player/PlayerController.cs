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
}
