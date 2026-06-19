using UnityEngine;

// Toggles the visibility of an on-screen mobile gamepad canvas based on the
// runtime platform AND the current overlay state. Put on the canvas itself
// (or any parent of it) and wire _gamepadRoot to the canvas root GameObject.
//
// Visibility rules:
//   - On Android / iOS / mobile WebGL touch: shown.
//   - On Windows / macOS / Linux: hidden.
//   - In the Unity Editor: hidden unless _showInEditor is checked.
//   - Whenever PauseMenu is open: hidden (so the pause UI isn't obscured).
//
// Implementation note: we drive a CanvasGroup (alpha + interactable +
// blocksRaycasts) instead of SetActive. If we toggled SetActive on the same
// GameObject this script lives on, the script would deactivate itself and
// couldn't react to a later "pause closed" event. CanvasGroup keeps the
// hierarchy alive while hiding it visually and disabling touch input.
public class MobileGamepadVisibility : MonoBehaviour
{
    [SerializeField] private GameObject _gamepadRoot;

    [Tooltip("Show the on-screen gamepad in the editor for layout iteration. Has no effect in built players.")]
    [SerializeField] private bool _showInEditor;

    [Tooltip("Alpha while the gamepad is visible. Below 1 for translucency over the world.")]
    [SerializeField, Range(0f, 1f)] private float _visibleAlpha = 0.6f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (_gamepadRoot == null)
        {
            Debug.LogWarning($"{name}: _gamepadRoot is not assigned; nothing to toggle.");
            return;
        }

        // Reuse an existing CanvasGroup if present (typical setup) or add one
        // so we always have a smooth hide/show path that doesn't disable the
        // script's own GameObject.
        _canvasGroup = _gamepadRoot.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = _gamepadRoot.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PauseMenu.OnOpened += Refresh;
        PauseMenu.OnClosed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        PauseMenu.OnOpened -= Refresh;
        PauseMenu.OnClosed -= Refresh;
    }

    private void Refresh()
    {
        if (_canvasGroup == null) return;

        bool platformWantsIt = Application.isMobilePlatform;
#if UNITY_EDITOR
        platformWantsIt = platformWantsIt || _showInEditor;
#endif

        bool shouldShow = platformWantsIt && !PauseMenu.IsOpen;

        _canvasGroup.alpha          = shouldShow ? _visibleAlpha : 0f;
        _canvasGroup.interactable   = shouldShow;
        _canvasGroup.blocksRaycasts = shouldShow;
    }
}
