using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Editor wizard that scaffolds a new interior scene with the minimum wiring
// needed by the SceneTransition system: a Grid+Tilemap to paint on, an
// arrival SpawnPoint, and a return DoorInteractable already pointed at the
// world scene. Saves the file and adds it to Build Settings in one go.
//
// Tools -> INAH -> Create Interior Scene...
//
// After the scaffold is created you still need to:
//   1. Paint the interior tiles on the Ground tilemap (Walls layer if needed).
//   2. Place the return door GameObject (already created) over the actual
//      door visual.
//   3. In SampleScene, add a SpawnPoint with id "from_<sceneName>" at the
//      position where the player should appear when leaving the interior.
public class InteriorSceneWizard : EditorWindow
{
    private string _sceneName        = "House_Abuela";
    private string _returnSceneName  = "SampleScene";
    private string _arrivalSpawnId   = "from_world";
    private string _returnSpawnId    = "from_house_abuela";

    [MenuItem("Tools/INAH/Create Interior Scene...")]
    private static void Open()
    {
        var w = GetWindow<InteriorSceneWizard>("Interior Scene");
        w.minSize = new Vector2(380, 220);
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a new interior scene scaffold", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Builds a Grid+Tilemap, an arrival SpawnPoint, and a return DoorInteractable. " +
            "Saves to Assets/Scenes and adds the result to Build Settings.",
            MessageType.Info);

        _sceneName       = EditorGUILayout.TextField("Scene name",        _sceneName);
        _returnSceneName = EditorGUILayout.TextField("Return scene",      _returnSceneName);
        _arrivalSpawnId  = EditorGUILayout.TextField("Arrival spawn id",  _arrivalSpawnId);
        _returnSpawnId   = EditorGUILayout.TextField("Return spawn id",   _returnSpawnId);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_sceneName)))
        {
            if (GUILayout.Button("Create scene", GUILayout.Height(28)))
                Create();
        }
    }

    private void Create()
    {
        string path = $"Assets/Scenes/{_sceneName}.unity";

        if (System.IO.File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog(
                "Scene exists",
                $"A scene already exists at {path}. Overwrite?",
                "Overwrite", "Cancel"))
                return;
        }

        // Make sure the Scenes folder exists.
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        // Save the currently-edited scene if needed before opening a new one.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Grid with a single Ground tilemap. User can add Walls / Decor
        //    sub-tilemaps later in the same Grid.
        var gridGo  = new GameObject("Grid");
        var grid    = gridGo.AddComponent<Grid>();
        grid.cellSize = Vector3.one; // PPU 16 + 1 unit per tile, matches the world scene.

        var tilemapGo = new GameObject("Ground");
        tilemapGo.transform.SetParent(gridGo.transform, worldPositionStays: false);
        tilemapGo.AddComponent<Tilemap>();
        tilemapGo.AddComponent<TilemapRenderer>();

        // 2. Arrival SpawnPoint where the player lands when entering from
        //    outside. Placed at (0, 1) so it's visually above the (0,0) door
        //    position; adjust in the editor.
        var spawnGo = new GameObject($"Spawn_{_arrivalSpawnId}");
        spawnGo.transform.position = new Vector3(0, 1, 0);
        var spawn = spawnGo.AddComponent<SpawnPoint>();
        SetPrivateString(spawn, "_id", _arrivalSpawnId);

        // 3. Return door — trigger collider so the player walks onto it
        //    rather than into it. Already pre-wired to take the player back
        //    to the world scene at the matching spawn id.
        var doorGo = new GameObject("ReturnDoor");
        doorGo.transform.position = new Vector3(0, 0, 0);
        var col = doorGo.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;

        var door = doorGo.AddComponent<DoorInteractable>();
        SetPrivateString(door, "_targetScene",   _returnSceneName);
        SetPrivateString(door, "_targetSpawnId", _returnSpawnId);

        // 4. Save the scene file.
        EditorSceneManager.SaveScene(scene, path);

        // 5. Add to Build Settings if not already there.
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, enabled: true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Debug.Log($"Interior scene scaffolded at {path} and added to Build Settings. " +
                  $"Don't forget to add a SpawnPoint with id '{_returnSpawnId}' in {_returnSceneName}.");

        Close();
    }

    // SpawnPoint and DoorInteractable expose their data via [SerializeField]
    // private backing fields. SerializedObject is the safe, official way to
    // poke them from editor scripts.
    private static void SetPrivateString(Object target, string fieldName, string value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"Wizard: field '{fieldName}' not found on {target.GetType().Name}.");
            return;
        }
        prop.stringValue = value;
        so.ApplyModifiedProperties();
    }
}
