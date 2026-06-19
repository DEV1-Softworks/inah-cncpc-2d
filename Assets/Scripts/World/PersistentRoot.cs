using UnityEngine;

// Parent GameObject in the world scene that owns everything which must
// survive scene transitions: the Player, the main Camera, the DialogueUI,
// the Hotbar UI, the TimeService, and any future persistent system.
//
// Put this component on an empty "Persistent" GameObject in the WORLD scene
// only. On first load it marks itself DDOL (DontDestroyOnLoad), carrying all
// its children along. On every subsequent reload of the world scene the
// duplicate guard destroys the freshly-spawned copy, keeping the original.
//
// Interior scenes must NOT include their own Persistent. They contain only
// tilemaps, interior NPCs, SpawnPoints and a return DoorInteractable.
public class PersistentRoot : MonoBehaviour
{
    private static PersistentRoot _instance;

    // Called by SceneFlow when going back to the title screen (or starting a
    // new game). The Persistent GameObject itself is destroyed separately;
    // this clears the static reference so the next load's duplicate-guard
    // sees "no instance yet" and accepts the new Persistent.
    public static void ResetInstance() => _instance = null;

    private void Awake()
    {
        // Second-load duplicate guard: the world scene file still defines
        // this GameObject, so loading it again instantiates a fresh copy.
        // Keep the first; drop the rest.
        if (_instance != null && _instance != this)
        {
            Debug.Log($"[PersistentRoot] Duplicate destroyed on scene reload — keeping the original DDOL instance.", _instance);
            Destroy(gameObject);
            return;
        }

        // Unity ignores DontDestroyOnLoad on non-root GameObjects. Detect this
        // configuration error early and tell the developer exactly why.
        if (transform.parent != null)
        {
            Debug.LogError(
                $"[PersistentRoot] '{name}' is NOT a root GameObject (parent is '{transform.parent.name}'). " +
                $"DontDestroyOnLoad is silently ignored on children. Move '{name}' to the scene root.",
                this);
            return;
        }

        if (transform.childCount == 0)
        {
            Debug.LogWarning(
                $"[PersistentRoot] '{name}' has no children. The Player, Camera, UIs and TimeService " +
                $"must be PARENTED under this GameObject for them to survive scene transitions.",
                this);
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[PersistentRoot] '{name}' marked DontDestroyOnLoad with {transform.childCount} child(ren). It will survive scene transitions.", this);
    }
}
