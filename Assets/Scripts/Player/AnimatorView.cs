using UnityEngine;

// The ONLY place that touches Unity's Animator and its parameter names.
// Mirror of PlayerInputReader: that class is the single owner of the input
// actions; this is the single owner of the Animator. Swapping this out for a
// different presentation (sprite-swap, Spine, etc.) leaves everyone else alone.
[RequireComponent(typeof(Animator))]
public class AnimatorView : MonoBehaviour, ICharacterAnimator
{
    private Animator _animator;

    // Hash the parameter names once. Cheaper than passing strings every frame,
    // and it keeps the "magic strings" confined to this one file.
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Moving = Animator.StringToHash("Moving"); // matches the controller's bool

    private void Awake() => _animator = GetComponent<Animator>();

    public void SetLocomotion(Vector2 facing, bool isMoving)
    {
        _animator.SetFloat(MoveX, facing.x);
        _animator.SetFloat(MoveY, facing.y);
        _animator.SetBool(Moving, isMoving);
    }
}
