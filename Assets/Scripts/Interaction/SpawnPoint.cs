using UnityEngine;

// World-side marker naming the position where the player lands after a scene
// transition. Doors that initiate a transition reference a target spawn id;
// the destination scene must contain a SpawnPoint with the matching id.
//
// Conventions:
//   - One SpawnPoint per inbound door (e.g. "from_world", "from_basement").
//   - Id "Default" is the fallback for doors that don't specify one.
//   - Multiple spawns per scene are fine — keep ids unique within the scene.
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Stable id referenced by the inbound DoorInteractable. Keep unique within a scene.")]
    [SerializeField] private string _id = "Default";

    public string Id => _id;
}
