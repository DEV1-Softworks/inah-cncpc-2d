using UnityEngine;

// The contract the game writes its "how should the character look" intent to.
// Mirror of IMovementInput: it hides a Unity subsystem (here, the Animator)
// behind game-language methods, so callers never touch Animator or its
// parameter strings directly.
public interface ICharacterAnimator
{
    // facing:   a cardinal direction (up/down/left/right) the character faces.
    // isMoving: true -> play the walk cycle, false -> play idle.
    void SetLocomotion(Vector2 facing, bool isMoving);
}
