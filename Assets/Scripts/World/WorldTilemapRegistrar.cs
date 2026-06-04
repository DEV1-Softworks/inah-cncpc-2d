using UnityEngine;
using UnityEngine.Tilemaps;

// Drops onto any Tilemap GameObject to expose it through the WorldTilemaps
// locator under the chosen key.
//
// Set _key to "Trees" on the Trees tilemap, "Walls" on the Walls tilemap,
// etc. — convenient keys are already exposed as named accessors on
// WorldTilemaps (Trees / Walls / Ground / Water), but the dictionary
// accepts anything so you can add new layer names without code changes.
[RequireComponent(typeof(Tilemap))]
public class WorldTilemapRegistrar : MonoBehaviour
{
    [SerializeField] private string _key = "Trees";

    private Tilemap _tilemap;

    private void Awake() => _tilemap = GetComponent<Tilemap>();

    private void OnEnable()  => WorldTilemaps.Register(_key, _tilemap);
    private void OnDisable() => WorldTilemaps.Unregister(_key, _tilemap);
}
