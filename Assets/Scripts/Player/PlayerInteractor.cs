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

    private IMovementInput _input;
    private readonly List<IInteractable> _candidates = new();

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
        if (_input == null || !_input.InteractPressed) return;

        // Interact button doubles as "close dialogue" while one is open. We do
        // this here (not in TextInteractable) so every IInteractable stays
        // ignorant of dialogue state — only the player's interaction loop
        // knows that pressing Interact during dialogue means "dismiss it."
        if (Dialogue.IsShowing)
        {
            Dialogue.Hide();
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
