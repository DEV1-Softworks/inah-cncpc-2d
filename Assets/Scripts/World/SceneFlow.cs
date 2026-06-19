using UnityEngine;
using UnityEngine.SceneManagement;

// High-level navigation between the title screen and the world. Lives outside
// the SceneTransition fade-and-teleport flow because these transitions don't
// teleport a player — they tear down the world or boot it from scratch.
//
// All three actions go through here so any UI (TitleScreenUI, PauseMenuUI,
// future "Game Over" screen, etc.) can call the same entry point.
public static class SceneFlow
{
    public const string TitleSceneName = "TitleScreen";
    public const string WorldSceneName = "SampleScene";

    public static void StartNewGame()
    {
        // No save system yet: starting a new game always boots the world fresh.
        // Reset time scale in case we're coming from the pause menu.
        Time.timeScale = 1f;
        ResetPersistent();
        SceneManager.LoadScene(WorldSceneName, LoadSceneMode.Single);
    }

    public static void ReturnToTitle()
    {
        Time.timeScale = 1f;
        ResetPersistent();
        SceneManager.LoadScene(TitleSceneName, LoadSceneMode.Single);
    }

    public static void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // The Persistent GameObject is DDOL after the first time SampleScene loads.
    // It survives even a Single-mode scene change. To make "Iniciar" act as
    // "new game" (not "resume"), we destroy it before loading any scene that
    // shouldn't inherit player state.
    //
    // PersistentRoot.ResetInstance() must clear its static singleton field; if
    // we only destroyed the GameObject, the static field would point to a
    // destroyed Unity reference and duplicate-guards on next load would think
    // an instance still exists.
    private static void ResetPersistent()
    {
        var existing = Object.FindAnyObjectByType<PersistentRoot>();
        if (existing != null) Object.Destroy(existing.gameObject);
        PersistentRoot.ResetInstance();
    }
}
