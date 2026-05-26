using UnityEngine;

public class PlayerInputReader : MonoBehaviour, IMovementInput
{
    private InputSystem_Actions _actions;
    public Vector2 Move => _actions.Player.Move.ReadValue<Vector2>();
    public bool SprintHeld => _actions.Player.Sprint.IsPressed();
    public bool InteractPressed => _actions.Player.Interact.WasPressedThisFrame();

    private void Awake()
    {
        _actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _actions.Enable();
    }

    private void OnDisable()
    {
        _actions.Disable();
    }

    private void OnDestroy()
    {
        _actions.Dispose();
    }
}