using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _inputSource;
    [SerializeField] private MonoBehaviour _gateSource;
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintMultiplier = 1.5f;

    private IMovementInput _input;
    private IMovementGate _gate;
    private Rigidbody2D _body;
    private bool _injected;

    // What the motor ACTUALLY moved this tick, in units/second. Read-only to the
    // outside (the animator reads this). Zero while idle or gated — so "hiding"
    // correctly reads as "not moving" without the animator knowing about Hide.
    public Vector2 Velocity { get; private set; }

    public void Construct(IMovementInput input, IMovementGate gate)
    {
        _input = input;
        _gate = gate;
        _injected = true;
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();

        if (_injected) return;

        if (_inputSource != null)
        {
            _input = _inputSource as IMovementInput;

            if (_input == null)
                Debug.LogError($"{name}: {_inputSource.GetType().Name} does not implement IMovementInput.");
        }

        if (_gateSource != null)
        {
            _gate = _gateSource as IMovementGate;

            if (_gate == null)
                Debug.LogError($"{name}: {_gateSource.GetType().Name} does not implement IMovementGate.");
        }
    }

    private void FixedUpdate()
    {
        // No input or movement is gated -> we're not moving. Publish that and bail.
        if (_input == null || (_gate != null && !_gate.CanMove))
        {
            Velocity = Vector2.zero;
            return;
        }

        Vector2 moveInput = _input.Move;

        if (moveInput.magnitude > 1f)
            moveInput = moveInput.normalized;

        float sprint = (_input.SprintHeld) ? _sprintMultiplier : 1f;
        Velocity = moveInput * _walkSpeed * sprint;            // units/second

        _body.MovePosition(_body.position + Velocity * Time.fixedDeltaTime);
    }
}