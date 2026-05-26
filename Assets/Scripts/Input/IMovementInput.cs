using UnityEngine;

public interface IMovementInput
{
    Vector2 Move { get; }
    bool SprintHeld { get; }
    bool InteractPressed { get; }
}
