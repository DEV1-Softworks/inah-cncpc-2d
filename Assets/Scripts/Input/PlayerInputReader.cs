using UnityEngine;

public class PlayerInputReader : MonoBehaviour, IMovementInput
{
    private InputSystem_Actions _actions;
    public Vector2 Move => _actions.Player.Move.ReadValue<Vector2>();
    public bool SprintHeld => _actions.Player.Sprint.IsPressed();

    // Each "pressed" intent OR-s the action map (keyboard / gamepad) with the
    // mobile on-screen bus (MobileInput). Either source firing this frame
    // satisfies the intent; consumers don't need to know where the press came
    // from.
    public bool InteractPressed =>
        _actions.Player.Interact.WasPressedThisFrame() ||
        MobileInput.InteractPressedThisFrame;

    // The "Use selected hotbar item" intent. Action map side: Attack
    // (left mouse + gamepad RT). Mobile side: virtual Use button.
    public bool UsePressed =>
        _actions.Player.Attack.WasPressedThisFrame() ||
        MobileInput.UsePressedThisFrame;

    // "Drop the selected hotbar stack." Action map side stays as the direct
    // Q-key read until the post-jam rebinding pass adds a Drop action to the
    // Input Actions asset. Mobile side: virtual Drop button.
    public bool DropPressed =>
        (UnityEngine.InputSystem.Keyboard.current?.qKey.wasPressedThisFrame ?? false) ||
        MobileInput.DropPressedThisFrame;

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