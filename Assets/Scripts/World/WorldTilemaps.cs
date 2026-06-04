using System.Collections.Generic;
using UnityEngine.Tilemaps;

// Static locator for the world's named tilemaps. The Trees / Walls / Ground
// tilemaps each have a WorldTilemapRegistrar component that registers itself
// here in OnEnable. Tools (ChopTreeUseBehavior, WaterUseBehavior, etc.) read
// `WorldTilemaps.Trees` directly — no serialized scene references needed,
// which is what lets us hold the behavior in a ScriptableObject asset.
//
// Why a string-keyed dictionary instead of hard-coded properties? So we can
// add new layers (Crops, Lights, Bridges) without growing this file's API —
// just register with a new key and reference it from whatever consumer needs
// it.
public static class WorldTilemaps
{
    private static readonly Dictionary<string, Tilemap> _maps = new();

    public static Tilemap Get(string key)
        => _maps.TryGetValue(key, out var m) ? m : null;

    public static void Register(string key, Tilemap tilemap)
        => _maps[key] = tilemap;

    public static void Unregister(string key, Tilemap tilemap)
    {
        if (_maps.TryGetValue(key, out var existing) && existing == tilemap)
            _maps.Remove(key);
    }

    // Convenience accessors for the most common layers. Keeping these
    // discoverable for ToolBehaviors that just want "the trees" without
    // remembering the key string.
    public static Tilemap Trees  => Get("Trees");
    public static Tilemap Walls  => Get("Walls");
    public static Tilemap Ground => Get("Ground");
    public static Tilemap Water  => Get("Water");
}
