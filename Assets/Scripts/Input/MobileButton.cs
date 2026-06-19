using UnityEngine;
using UnityEngine.EventSystems;

// On-screen tappable button. Goes on a UI element (Image / Button) with a
// Graphic that has Raycast Target enabled. Maps a touch / click to one of the
// game's input intents via MobileInput.
//
// Why IPointerDownHandler (and not Click): on mobile we want the action to
// fire the instant the player presses, mirroring keyboard WasPressedThisFrame
// behavior. Pointer-up timing would feel laggy.
public class MobileButton : MonoBehaviour, IPointerDownHandler
{
    public enum ButtonAction { Interact, Use, Drop, Pause }

    [SerializeField] private ButtonAction _action = ButtonAction.Interact;

    public void OnPointerDown(PointerEventData _)
    {
        switch (_action)
        {
            case ButtonAction.Interact: MobileInput.PressInteract(); break;
            case ButtonAction.Use:      MobileInput.PressUse();      break;
            case ButtonAction.Drop:     MobileInput.PressDrop();     break;
            // Pause talks to PauseMenu directly — there's no per-frame intent
            // flag because nothing else polls a "pause this frame" signal.
            // PauseMenu does its own toggle internally; we just open.
            case ButtonAction.Pause:    PauseMenu.Open();            break;
        }
    }
}
