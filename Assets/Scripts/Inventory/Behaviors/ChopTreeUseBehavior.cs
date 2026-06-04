using UnityEngine;
using UnityEngine.Tilemaps;

// Tool behavior: pressing Use applies damage to the tile one cell in front
// of the player on the chosen tilemap. If a `TilemapResourceHealth` lives
// on the tilemap GameObject, damage is tracked per-cell and the tile stays
// visible until depleted; if not, a single hit clears the tile (legacy
// instant-chop behavior, preserved for "scrub a weed" cases that don't need
// HP).
//
// When the cell finally depletes, the tile is cleared from the tilemap AND
// (optionally) a pickup of the drop item is spawned at the cell's center.
//
// Wires (per Item that uses this behavior — i.e. each axe/saw asset):
//   _targetKey      -> name of the tilemap to chop in (matches a
//                      WorldTilemapRegistrar in the scene)
//   _damagePerHit   -> how much HP to chip per swing (1 by default; a
//                      stronger axe could be 2; ignored when the tilemap
//                      has no TilemapResourceHealth)
//   _dropItem       -> Item to spawn as a pickup when something depletes
//                      (leave null for "chop, no drop")
//   _dropCount      -> how many of _dropItem to spawn on depletion
//   _dropPrefab     -> the generic DroppedItem prefab (sprite + trigger
//                      collider + PickupInteractable) — same prefab
//                      PlayerDropHandler uses
//
// Returns false so the axe is NOT consumed on use.
//
// Future polish (not built here): damaged-tile sprite swap mid-chop, tool
// tier vs tile hardness, axe animation trigger, particle/SFX per swing.
[CreateAssetMenu(menuName = "Inventory/Use Behaviors/Chop Tree",
                 fileName = "ChopTreeUseBehavior")]
public class ChopTreeUseBehavior : ItemUseBehavior
{
    [SerializeField] private string             _targetKey    = "Trees";
    [SerializeField] private int                _damagePerHit = 1;
    [SerializeField] private Item               _dropItem;
    [SerializeField] private int                _dropCount    = 1;
    [SerializeField] private PickupInteractable _dropPrefab;

    public override bool Use(GameObject user, ItemStack stack)
    {
        var map = WorldTilemaps.Get(_targetKey);
        if (map == null) return false;

        // Where is the player looking? PlayerAnimationDriver caches facing
        // from motor velocity; we read it instead of guessing from input
        // (which is zero when the player is standing still).
        Vector2 facing = ReadFacing(user);
        if (facing == Vector2.zero) facing = Vector2.down; // safety fallback

        // Target the cell one step in the facing direction.
        Vector3Int userCell   = map.WorldToCell(user.transform.position);
        Vector3Int targetCell = userCell + new Vector3Int(
            Mathf.RoundToInt(facing.x),
            Mathf.RoundToInt(facing.y),
            0);

        var tile = map.GetTile(targetCell);
        if (tile == null) return false; // nothing to chop

        // If the tilemap has health tracking, ask it to absorb the hit and
        // tell us which cells (if any) just depleted. The health component
        // flood-fills the connected cluster from the hit cell, so chopping
        // ANY cell of a multi-tile tree damages the whole tree's HP and
        // returns ALL its cells when it finally falls.
        //
        // If no health component is on the tilemap, behave like the original
        // one-shot chop (single cell instantly removed).
        var health = map.GetComponent<TilemapResourceHealth>();
        System.Collections.Generic.IReadOnlyList<Vector3Int> depleted;
        if (health != null)
        {
            // Tool gate: if the tilemap names a required tool (e.g. Axe),
            // any other item bouncing off this behavior is rejected. Same
            // as Stardew's "wrong tool, no effect."
            if (!health.AcceptsTool(stack.item)) return false;

            depleted = health.Damage(targetCell, Mathf.Max(1, _damagePerHit));
            if (depleted.Count == 0) return false; // HP remains; tiles stay visible
        }
        else
        {
            depleted = new[] { targetCell };
        }

        // Clear every cell of the depleted group.
        for (int i = 0; i < depleted.Count; i++)
            map.SetTile(depleted[i], null);

        // Spawn the drop once at the anchor cell (first in the depleted list).
        if (_dropItem != null && _dropPrefab != null && _dropCount > 0)
        {
            Vector3 spawnPos = map.GetCellCenterWorld(depleted[0]);
            var pickup = Object.Instantiate(_dropPrefab, spawnPos, Quaternion.identity);
            pickup.Configure(_dropItem, _dropCount);
        }

        return false;
    }

    private static Vector2 ReadFacing(GameObject user)
    {
        var driver = user.GetComponent<PlayerAnimationDriver>();
        return driver != null ? driver.Facing : Vector2.zero;
    }
}
