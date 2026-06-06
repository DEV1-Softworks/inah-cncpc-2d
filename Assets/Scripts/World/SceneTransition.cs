using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Singleton orchestrator of scene transitions. Owns the fade overlay and the
// transition coroutine. Static entry point: SceneTransition.Go(scene, spawnId).
//
// Lifecycle:
//   1. Caller (DoorInteractable) invokes Go(scene, spawnId).
//   2. First call lazy-creates the instance and its fade canvas, both DDOL.
//   3. Coroutine: lock player -> fade to black -> LoadSceneAsync (Single) ->
//      find matching SpawnPoint -> teleport player -> fade from black ->
//      unlock player.
//
// Why a lazy singleton: callers never need a serialized reference, and the
// fade canvas is built programmatically so the world scene authors zero UI
// for transitions. The pattern mirrors Dialogue / Chests / Hotbar / GameTime
// — one active, accessed by name.
public class SceneTransition : MonoBehaviour
{
    [Header("Fade")]
    [Tooltip("Seconds for the fade-to-black AND the fade-from-black halves of the transition.")]
    [SerializeField] private float _fadeDuration = 0.35f;

    private CanvasGroup _fadeGroup;
    private static SceneTransition _instance;
    private static bool _isTransitioning;

    // -------- Public entry --------

    public static void Go(string sceneName, string spawnId)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (_isTransitioning) return; // ignore re-entrancy mid-transition

        EnsureInstance();
        _instance.StartCoroutine(_instance.Run(sceneName, spawnId));
    }

    // -------- Singleton bootstrap --------

    private static void EnsureInstance()
    {
        if (_instance != null) return;

        var existing = FindAnyObjectByType<SceneTransition>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject("[SceneTransition]");
        _instance = go.AddComponent<SceneTransition>();
        // Awake of the new component will build the canvas and DDOL itself.
    }

    private void Awake()
    {
        // Guard against duplicates if the user pre-placed one AND a Go() call
        // also fired the lazy path: keep the first, drop the rest.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);
        BuildFadeCanvas();
    }

    // -------- Coroutine --------

    private IEnumerator Run(string sceneName, string spawnId)
    {
        _isTransitioning = true;

        // Lock the player so movement input is ignored during the transition.
        // PlayerController.SetState(InEvent) sets CanMove=false; PlayerMotor
        // zeroes velocity. Restored after the second fade.
        var controller = FindAnyObjectByType<PlayerController>();
        if (controller != null) controller.SetState(PlayerState.InEvent);

        yield return Fade(0f, 1f);

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (op != null && !op.isDone) yield return null;

        // The destination scene's objects are now live. Find the right spawn.
        var spawn = FindSpawn(spawnId);
        if (spawn != null)
        {
            // Re-fetch the controller: if the previous scene's PlayerController
            // was destroyed (i.e. it wasn't DDOL), we must locate the new one.
            // If the same DDOL player carried over, this returns the same ref.
            controller = FindAnyObjectByType<PlayerController>();
            if (controller != null) TeleportPlayer(controller, spawn.transform.position);
        }
        else
        {
            Debug.LogWarning($"SceneTransition: no SpawnPoint with id '{spawnId}' in scene '{sceneName}'. Player position unchanged.");
        }

        yield return Fade(1f, 0f);

        if (controller != null) controller.SetState(PlayerState.Free);
        _isTransitioning = false;
    }

    // -------- Helpers --------

    private IEnumerator Fade(float from, float to)
    {
        if (_fadeGroup == null) yield break;

        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled so a paused world doesn't freeze the fade
            _fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / _fadeDuration));
            yield return null;
        }
        _fadeGroup.alpha = to;
    }

    private static SpawnPoint FindSpawn(string spawnId)
    {
        // FindObjectsByType is the modern, sorted alternative to the
        // deprecated FindObjectsOfType. SortMode.None is fine — we just need
        // the first match by id.
        var spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < spawns.Length; i++)
        {
            if (spawns[i].Id == spawnId) return spawns[i];
        }
        return null;
    }

    private static void TeleportPlayer(PlayerController controller, Vector3 worldPos)
    {
        // Dynamic Rigidbody2D: set rb.position so the physics step doesn't
        // interpolate between the old and new positions (which would draw a
        // brief streak). Setting transform.position too covers the case where
        // the rigidbody is missing or sleeping.
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = worldPos;
        controller.transform.position = worldPos;
    }

    private void BuildFadeCanvas()
    {
        // Canvas at very high sort order so the fade sits on top of every
        // existing UI (HUD, dialogue, hotbar, etc.).
        var canvasGo = new GameObject("FadeCanvas");
        canvasGo.transform.SetParent(transform, worldPositionStays: false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        // Solid full-screen black image stretched to all corners.
        var imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, worldPositionStays: false);

        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false; // never block input

        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // CanvasGroup lives on the canvas root so a single alpha drives the
        // whole overlay (and we can extend later with extra children).
        _fadeGroup = canvasGo.AddComponent<CanvasGroup>();
        _fadeGroup.alpha = 0f;
        _fadeGroup.interactable = false;
        _fadeGroup.blocksRaycasts = false;
    }
}
