using UnityEngine;

// Hides this GameObject when the app is running in a WebGL build. The Quit
// flows in TitleScreenUI / PauseMenuUI call Application.Quit(), which is a
// no-op in the browser (and the button has no useful action), so we just
// remove it from the UI on that target.
//
// Runtime check rather than #if so the editor preview keeps the button
// visible — useful for designing the screen on any platform.
//
// Usage: drop on the Quit Button GameObject inside the TitleScreen and the
// PauseMenu.
public class HideOnWebGL : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            gameObject.SetActive(false);
    }
}
