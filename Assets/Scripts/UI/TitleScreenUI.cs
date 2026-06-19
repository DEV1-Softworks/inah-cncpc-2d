using UnityEngine;

// Tiny controller for the TitleScreen scene. Holds nothing — every button
// just calls into SceneFlow. Lives on any GameObject in the TitleScreen
// scene; wire the buttons' onClick events to its public methods.
//
// Once a save system exists, this is also where the "Continuar" button
// will live (calling SceneFlow.ContinueGame() or equivalent).
public class TitleScreenUI : MonoBehaviour
{
    // -------- Button targets (wire via Inspector onClick) --------

    public void OnStartClick() => SceneFlow.StartNewGame();
    public void OnQuitClick()  => SceneFlow.Quit();
}
