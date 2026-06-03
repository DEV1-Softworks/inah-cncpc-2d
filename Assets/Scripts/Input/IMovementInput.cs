using UnityEngine;

public interface IMovementInput
{
    Vector2 Move { get; }
    bool SprintHeld { get; }
    bool InteractPressed { get; }
    bool UsePressed { get; }      // fires the selected hotbar item's UseBehavior
    bool DropPressed { get; }     // drop the selected hotbar stack into the world
}
