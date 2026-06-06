using UnityEngine;

// World-side door: implements IInteractable. When the player interacts, asks
// SceneTransition to take them to _targetScene and drop them on the SpawnPoint
// whose id matches _targetSpawnId.
//
// The same door pattern is used both ways: the door of a house in the world
// scene points at the house interior; the "back door" inside the interior
// points at the world scene with a spawn id like "from_house_abuela".
//
// Authoring:
//   - Drop on any GameObject with a Collider2D (set "Is Trigger" so the door
//     enters the PlayerInteractor's awareness without blocking movement).
//   - _targetScene must be the EXACT scene name (must be added to Build
//     Settings, File -> Build Settings -> Scenes In Build).
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Exact name of the scene to load. Must be in Build Settings.")]
    [SerializeField] private string _targetScene;

    [Tooltip("Id of the SpawnPoint in the destination scene where the player lands.")]
    [SerializeField] private string _targetSpawnId = "Default";

    public void Interact()
    {
        if (string.IsNullOrEmpty(_targetScene))
        {
            Debug.LogWarning($"{name}: DoorInteractable has no _targetScene set.", this);
            return;
        }

        SceneTransition.Go(_targetScene, _targetSpawnId);
    }
}
