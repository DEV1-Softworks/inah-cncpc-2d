using UnityEngine;
using UnityEngine.InputSystem;

// Concrete pause menu. Goes on a Canvas under Persistent so it's available
// across all world scenes (SampleScene, House, INAH, ...).
//
// Wires (Inspector):
//   _visualRoot -> the Panel child GameObject. Toggled active/inactive.
//
// Button onClick targets (wired in the Inspector):
//   OnResumeClick()        -> resume gameplay
//   OnReturnToTitleClick() -> SceneFlow.ReturnToTitle()
//   OnQuitClick()          -> SceneFlow.Quit()
//
// Lifecycle:
//   - Opening sets Time.timeScale = 0, freezing physics, animations and the
//     TimeService (which reads Time.deltaTime).
//   - Closing restores Time.timeScale = 1.
//   - PlayerController locks movement automatically via the OnOpened event,
//     same pattern as Vendor / Chest / HireOffice overlays.
//
// Input:
//   - Escape (keyboard) toggles the menu open/closed.
//   - Direct Keyboard.current read for now; will move into the action map
//     during the post-jam rebinding pass.
public class PauseMenuUI : MonoBehaviour, IPauseMenu
{
    [SerializeField] private GameObject _visualRoot;

    [Tooltip("If true, Escape toggles the menu. Disable if you want input from a virtual button only (mobile).")]
    [SerializeField] private bool _allowKeyboardToggle = true;

    public bool IsOpen => _visualRoot != null && _visualRoot.activeSelf;

    private void OnEnable()
    {
        PauseMenu.Register(this);
        Close(); // start hidden
    }

    private void OnDisable() => PauseMenu.Unregister(this);

    private void Update()
    {
        if (!_allowKeyboardToggle) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen) Close();
            else        Open();
        }
    }

    public void Open()
    {
        if (_visualRoot != null) _visualRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (_visualRoot != null) _visualRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // -------- Button targets (wire via Inspector onClick) --------

    public void OnResumeClick()        => PauseMenu.Close();
    public void OnReturnToTitleClick() => SceneFlow.ReturnToTitle();
    public void OnQuitClick()          => SceneFlow.Quit();
}
