using UnityEngine;

// Simple 2D follow camera. Goes on the Main Camera under Persistent.
//
// Why this exists in a DDOL setup: the camera survives scene transitions, but
// its world position does NOT auto-track the player. After a scene change the
// camera is still wherever it was in the previous scene — typically far from
// the SpawnPoint we just teleported the player to. This component lerps the
// camera toward the player every late-update, and re-acquires the target on
// scene change.
//
// Drop it on the Main Camera. _target auto-binds to the PlayerController at
// start; if you want a different anchor (e.g. a focus marker for cutscenes),
// just set _target manually.
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform the camera tracks. Auto-binds to PlayerController if left empty.")]
    [SerializeField] private Transform _target;

    [Tooltip("Smaller = snappier follow. 0 = instant.")]
    [SerializeField] private float _smoothTime = 0.12f;

    [Tooltip("Camera Z must stay negative for a 2D ortho camera. XY add a framing offset.")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (_target == null) TryBindTarget();
        if (_target == null) return;

        Vector3 desired = _target.position + _offset;
        // Preserve the offset's Z (camera depth) exactly; lerp only XY.
        desired.z = _offset.z;

        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, _smoothTime);
    }

    private void TryBindTarget()
    {
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) _target = player.transform;
    }
}
