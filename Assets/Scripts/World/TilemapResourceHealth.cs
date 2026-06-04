using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Per-resource HP tracking for a Tilemap. A "resource" is a connected cluster
// of non-empty cells (4-neighbor flood-fill from the chopped cell). HP is
// kept against the cluster's CANONICAL ANCHOR (lex-min cell), so chopping
// any cell of a multi-tile tree damages the same pool, and the whole group
// is cleared together when depleted.
//
// Tool behaviors call Damage(cell, amount); the component returns the list
// of cells that just depleted (empty list when there's still HP left;
// 1-cell list when a single-tile resource finally drops; many-cell list
// when a multi-cell tree falls). The caller clears each returned cell from
// the tilemap and spawns drops as it sees fit.
//
// Why flood-fill on every chop? Cheap — trees are 2–6 cells, the queue
// returns in microseconds. The alternative (caching group membership) is
// more code and only matters if you have hundreds of damaged groups at
// once, which gameplay doesn't produce.
[RequireComponent(typeof(Tilemap))]
public class TilemapResourceHealth : MonoBehaviour
{
    [SerializeField] private int  _defaultMaxHp  = 3;

    // Optional tool gate. When set, AcceptsTool returns true only for this
    // exact Item — used by the chop behavior to reject "wrong tool" hits
    // (Berry can't chop trees, even if someone wires it to a chop behavior).
    // Leave null for "anything goes" (useful for foraging tiles that any
    // tool can clear).
    [SerializeField] private Item _requiredTool;

    // HP keyed by the group's canonical anchor (lex-min cell of the cluster).
    // Stable across chops because flood-fill from anywhere in the same
    // cluster always finds the same anchor.
    private readonly Dictionary<Vector3Int, int> _hp = new();

    // Cached so we don't GetComponent every call.
    private Tilemap _map;

    private void Awake() => _map = GetComponent<Tilemap>();

    public int  DefaultMaxHp => _defaultMaxHp;
    public Item RequiredTool => _requiredTool;

    // True if the given tool is allowed to damage this tilemap. When no
    // _requiredTool is configured, every tool is allowed.
    public bool AcceptsTool(Item tool) => _requiredTool == null || _requiredTool == tool;

    // Apply damage at `cell`. Returns the cells that depleted as a result:
    //   empty list -> still has HP; tile(s) stay visible
    //   non-empty  -> caller should clear each cell from the tilemap and
    //                 spawn drops at (e.g.) the first cell of the list
    public IReadOnlyList<Vector3Int> Damage(Vector3Int cell, int amount)
    {
        if (amount <= 0) return System.Array.Empty<Vector3Int>();
        if (_map == null) _map = GetComponent<Tilemap>();
        if (_map.GetTile(cell) == null) return System.Array.Empty<Vector3Int>();

        var group = FloodFillFrom(cell);
        if (group.Count == 0) return System.Array.Empty<Vector3Int>();

        Vector3Int anchor = LexMin(group);

        int hp = _hp.TryGetValue(anchor, out int current) ? current : _defaultMaxHp;
        hp -= amount;

        if (hp > 0)
        {
            _hp[anchor] = hp;
            return System.Array.Empty<Vector3Int>();
        }

        _hp.Remove(anchor);
        return group;
    }

    public int GetHp(Vector3Int cell)
    {
        if (_map == null) _map = GetComponent<Tilemap>();
        if (_map.GetTile(cell) == null) return 0;

        var group = FloodFillFrom(cell);
        if (group.Count == 0) return 0;
        var anchor = LexMin(group);
        return _hp.TryGetValue(anchor, out int current) ? current : _defaultMaxHp;
    }

    public void Reset(Vector3Int cell)
    {
        if (_map == null) _map = GetComponent<Tilemap>();
        if (_map.GetTile(cell) == null) return;
        var group = FloodFillFrom(cell);
        if (group.Count == 0) return;
        _hp.Remove(LexMin(group));
    }

    // 4-neighbor flood-fill from `start`, including any cell with a non-null
    // tile. Stops at empty cells, so individual resources separated by at
    // least one blank cell stay separate groups.
    private List<Vector3Int> FloodFillFrom(Vector3Int start)
    {
        var result  = new List<Vector3Int>();
        var visited = new HashSet<Vector3Int> { start };
        var queue   = new Queue<Vector3Int>();
        queue.Enqueue(start);

        Vector3Int[] dirs = {
            new Vector3Int( 1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 1, 0), new Vector3Int( 0,-1, 0),
        };

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (_map.GetTile(c) == null) continue;
            result.Add(c);
            foreach (var d in dirs)
            {
                var next = c + d;
                if (visited.Add(next)) queue.Enqueue(next);
            }
        }
        return result;
    }

    // Pick a deterministic anchor for the cluster: smallest y, then smallest
    // x. Any chop on the same cluster finds the same anchor → HP persists
    // across hits no matter where the player swings.
    private static Vector3Int LexMin(List<Vector3Int> cells)
    {
        var min = cells[0];
        for (int i = 1; i < cells.Count; i++)
        {
            var c = cells[i];
            if (c.y < min.y || (c.y == min.y && c.x < min.x)) min = c;
        }
        return min;
    }
}
