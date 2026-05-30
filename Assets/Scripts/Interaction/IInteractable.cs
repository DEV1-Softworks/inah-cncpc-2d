// The contract any "interactable" world object implements. Speaks game terms
// (Interact), so the player-side code never depends on a specific type — same
// pattern as IMovementInput, IMovementGate, ICharacterAnimator.
public interface IInteractable
{
    void Interact();
}
