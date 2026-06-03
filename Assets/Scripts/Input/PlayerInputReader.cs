using UnityEngine;

public class PlayerInputReader : MonoBehaviour, IMovementInput
{
    private InputSystem_Actions _actions;
    public Vector2 Move => _actions.Player.Move.ReadValue<Vector2>();
    public bool SprintHeld => _actions.Player.Sprint.IsPressed();
    public bool InteractPressed => _actions.Player.Interact.WasPressedThisFrame();

    // The "Use selected hotbar item" intent. Currently backed by the Attack
    // action (default binding: left mouse button + gamepad RT), which fits
    // Stardew-style "click to swing tool / use item."
    public bool UsePressed     => _actions.Player.Attack.WasPressedThisFrame();

    // "Drop the selected hotbar stack." Direct keyboard read for now (Q key)
    // — same pragmatic shortcut HotbarUI uses for number-key slot selection.
    // When the project rebinding pass happens, add a Drop action to the
    // Input Actions asset and replace this with a generated reader.
    public bool DropPressed    => UnityEngine.InputSystem.Keyboard.current?.qKey.wasPressedThisFrame ?? false;

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