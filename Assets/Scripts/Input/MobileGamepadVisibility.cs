using UnityEngine;

// Toggles the visibility of an on-screen mobile gamepad canvas based on the
// runtime platform. Put on the canvas itself (or any parent of it) and wire
// _gamepadRoot to the canvas root GameObject.
//
// Default behaviour:
//   - On Android / iOS / mobile WebGL touch: shown.
//   - On Windows / macOS / Linux: hidden.
//   - In the Unity Editor: hidden unless _showInEditor is checked, so you can
//     iterate on the canvas layout without juggling builds.
public class MobileGamepadVisibility : MonoBehaviour
{
    [SerializeField] private GameObject _gamepadRoot;

    [Tooltip("Show the on-screen gamepad in the editor for layout iteration. Has no effect in built players.")]
    [SerializeField] private bool _showInEditor;

    private void Start()
    {
        if (_gamepadRoot == null)
        {
            Debug.LogWarning($"{name}: _gamepadRoot is not assigned; nothing to toggle.");
            return;
        }

        bool show = Application.isMobilePlatform;
#if UNITY_EDITOR
        show = show || _showInEditor;
#endif
        _gamepadRoot.SetActive(show);
    }
}
