using UnityEngine;

// Reads what the motor ACTUALLY did and translates it into animation intent.
// This is the "consumer" in the pattern: it depends on the ICharacterAnimator
// interface, not on Unity's Animator. Note it reads motor.Velocity (not raw
// input), so whenever movement is gated (tool swing, cutscene -> Velocity is
// zero) it correctly shows idle without knowing anything about PlayerState.
[RequireComponent(typeof(PlayerMotor))]
public class PlayerAnimationDriver : MonoBehaviour
{
    [SerializeField] private PlayerMotor _motor;
    [SerializeField] private MonoBehaviour _animatorSource; // must implement ICharacterAnimator

    private ICharacterAnimator _view;

    // Cardinal facing direction the player is currently pointing — one of
    // (1,0), (-1,0), (0,1), (0,-1). Updated whenever the motor's velocity
    // changes direction, and held constant while idle. Exposed publicly so
    // tools (axe, watering can, ...) and any other system that needs
    // "where is the player looking?" can read it without recomputing.
    public Vector2 Facing { get; private set; } = Vector2.down;

    private void Awake()
    {
        if (_motor == null) _motor = GetComponent<PlayerMotor>();

        _view = _animatorSource as ICharacterAnimator;
        if (_view == null && _animatorSource != null)
            Debug.LogError($"{name}: {_animatorSource.GetType().Name} does not implement ICharacterAnimator.");
    }

    // Animation is visual, so drive it in Update (per rendered frame), reading
    // the velocity the motor published in FixedUpdate.
    private void Update()
    {
        if (_view == null) return;

        Vector2 velocity = _motor.Velocity;
        bool isMoving = velocity.sqrMagnitude > 0.0001f;

        // Only update facing WHILE moving, so idle keeps the last direction
        // you walked — that's the core trick behind top-down idle poses.
        if (isMoving)
            Facing = ToCardinal(velocity);

        _view.SetLocomotion(Facing, isMoving);
    }

    // Snap any direction to the nearest of 4 cardinals. The art is 4-directional,
    // so feeding raw diagonals would blend two clips into visual mush.
    // Ties (perfect diagonals) break toward HORIZONTAL via >=, so up/down-right
    // hold "right" and up/down-left hold "left" — matching the controller's table.
    private static Vector2 ToCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) >= Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
    }
}
