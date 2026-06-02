using System.Collections.Generic;
using UnityEngine;

// Bridges player input and IInteractable. It watches what overlaps THIS
// GameObject's trigger Collider2D (the player's "interaction zone") and fires
// the closest one when the Interact button is pressed.
//
// Put this on a CHILD of the Player with an isTrigger Collider2D — separate
// from the body's solid collider so the zone can overlap interactables without
// blocking movement.
[RequireComponent(typeof(Collider2D))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _inputSource; // must implement IMovementInput

    [Tooltip("Stick / D-pad needs to cross this magnitude to register as a discrete choice nav press.")]
    [SerializeField] private float _choiceNavThreshold = 0.5f;

    private IMovementInput _input;
    private readonly List<IInteractable> _candidates = new();

    // For choice navigation: edge-detect Move.y so holding a direction selects
    // one option at a time, not "scrolls" the menu at framerate.
    private float _prevMoveY;

    private void Awake()
    {
        _input = _inputSource as IMovementInput;
        if (_input == null && _inputSource != null)
            Debug.LogError($"{name}: {_inputSource.GetType().Name} does not implement IMovementInput.");

        if (!GetComponent<Collider2D>().isTrigger)
            Debug.LogWarning($"{name}: Collider2D should be 'Is Trigger' so it overlaps without blocking.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var it) && !_candidates.Contains(it))
            _candidates.Add(it);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var it))
            _candidates.Remove(it);
    }

    private void Update()
    {
        if (_input == null) return;

        // Choice navigation: while the dialogue is waiting for a pick, treat
        // vertical Move input as discrete Up/Down keystrokes. Edge-detected
        // against the previous frame so holding a direction selects exactly
        // one option per push, not 60 per second.
        if (Dialogue.IsAwaitingChoice)
        {
            float y = _input.Move.y;
            if (y >  _choiceNavThreshold && _prevMoveY <=  _choiceNavThreshold) Dialogue.MoveSelection(-1); // up
            if (y < -_choiceNavThreshold && _prevMoveY >= -_choiceNavThreshold) Dialogue.MoveSelection(+1); // down
            _prevMoveY = y;
        }
        else
        {
            _prevMoveY = 0f;
        }

        if (!_input.InteractPressed) return;

        // Interact button doubles as "advance the dialogue" while one is open.
        // The dialogue service decides what that means in context: confirm the
        // highlighted choice, show the next page, or auto-close after the last.
        // Every IInteractable stays ignorant of dialogue state.
        if (Dialogue.IsShowing)
        {
            Dialogue.Advance();
            return;
        }

        if (_candidates.Count == 0) return;

        // Pick the closest candidate so the right one fires when several overlap.
        // Also guards against destroyed interactables (Unity fake-null) that
        // never sent OnTriggerExit2D.
        IInteractable best = null;
        float bestSqr = float.MaxValue;
        Vector2 here = transform.position;

        foreach (var it in _candidates)
        {
            if (it is Component c && c != null)
            {
                float d = ((Vector2)c.transform.position - here).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = it; }
            }
        }

        best?.Interact();
    }
}
