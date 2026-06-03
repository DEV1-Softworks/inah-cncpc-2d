# INAHWithNoName (working title)

Unity 2D top-down **Stardew-Valley-like** (farming/life-sim). "INAH" alludes to
Mexico's Instituto Nacional de Antropología e Historia — likely a
museum/archaeology theme. **Stealth/disguise/crouch was deliberately cut** as
out-of-genre (see `[0.0.1] Removed` in [CHANGELOG.md](CHANGELOG.md)). The
`IMovementGate` plus a new `PlayerState` mode make it cheap to add back if the
theme ever wants it.

## Stack

- **Unity 6.3** (`6000.3.5f2`), URP 2D renderer.
- **Input System** (new package) — generated `InputSystem_Actions.cs`.
- **TextMeshPro** for UI text.
- **2D Tilemap + 2D Tilemap Extras** for the world (includes `AnimatedTile`).

## Architecture — three patterns to internalize

Once these click, every file in the project explains itself.

### 1. Interface-driven decoupling

Every Unity subsystem is hidden behind a *game-language* interface. One
concrete owns the Unity-side; consumers depend only on the interface and
are wired via a serialized `MonoBehaviour _xxxSource` slot cast in `Awake`.

| Concern | Interface | Single owner (concrete) | Consumers |
|---|---|---|---|
| Player input | `IMovementInput` | `PlayerInputReader` (wraps `InputSystem_Actions`) | `PlayerMotor` |
| Movement gating | `IMovementGate` (`CanMove`) | `PlayerController` (the brain) | `PlayerMotor` |
| Character animation | `ICharacterAnimator` | `AnimatorView` (owns Unity `Animator`) | `PlayerAnimationDriver` |
| Interactable world objects | `IInteractable` (`Interact()`) | `TextInteractable` (and future kinds) | `PlayerInteractor` |
| Dialogue presentation | `IDialogueService` | `CanvasDialogueService` (on `DialogueUI.prefab`) | Anyone, via static `Dialogue` locator |

### 2. Single owner of state

Anything that owns state exposes a `get; private set;` property and a
`SetState(next)` method. The compiler enforces the rule; callers can't sidestep
the door. See `PlayerController.SetState` for the canonical shape — that's
where future transition rules / events / sounds will live.

### 3. Static locator for app-wide singletons

For "there's exactly one of these" systems (dialogue), we use a small static
class that holds the active interface implementer plus events. The concrete
registers itself in `OnEnable` and unregisters in `OnDisable`. See
`Dialogue` in [Assets/Scripts/UI/IDialogueService.cs](Assets/Scripts/UI/IDialogueService.cs):
`Dialogue.Show(text)`, `Dialogue.Hide()`, plus `OnShown` / `OnHidden` events.

The locator is **only** used for genuinely singleton-ish systems — not as a
substitute for the DI seams everywhere else.

## Player

The Player is a **prefab** at [Assets/Prefabs/Player.prefab](Assets/Prefabs/Player.prefab).
Structure:

```
Player                  (Dynamic Rigidbody2D, gravity 0, freeze rot Z, solid BoxCollider2D)
  ├── PlayerInputReader, PlayerMotor, PlayerController, AnimatorView,
  │   PlayerAnimationDriver  (components on the root)
  └── InteractorZone   (child; trigger BoxCollider2D + PlayerInteractor)
```

- [PlayerMotor.cs](Assets/Scripts/Player/PlayerMotor.cs) — moves via
  `Rigidbody2D.MovePosition` in `FixedUpdate`. Exposes `Velocity` (units/sec,
  zeroed when gated).
- [PlayerController.cs](Assets/Scripts/Player/PlayerController.cs) — the
  brain. Owns `PlayerState` (`Free` / `UsingTool` / `InEvent`).
  `CanMove => State == Free`. Subscribes to `Dialogue.OnShown/Hidden` to lock
  player movement during dialogue automatically.
- [PlayerAnimationDriver.cs](Assets/Scripts/Player/PlayerAnimationDriver.cs)
  — reads `Velocity`, snaps to nearest cardinal direction (ties break to
  **horizontal** — by design), keeps the last-non-zero facing so idle pose is
  correct, and writes `MoveX` / `MoveY` / `Moving` to the `AnimatorView`.
- [Animator Controller](Assets/Animations/Controllers/Player.controller) —
  Idle + Walk 2D blend trees keyed on `MoveX/MoveY`. Transitions toggle on the
  `Moving` bool with **Has Exit Time off** (responsive start/stop).
- [PlayerInteractor.cs](Assets/Scripts/Player/PlayerInteractor.cs) — lives on
  `InteractorZone`. Tracks overlapping `IInteractable`s, fires the closest on
  `InteractPressed`. **Pressing Interact while dialogue is open closes it** —
  that toggle logic lives here, not in any interactable.

## Dialogue UI

[Assets/Prefabs/DialogueUI.prefab](Assets/Prefabs/DialogueUI.prefab) — drop
**one instance** into any scene that needs dialogue. Without an instance,
`Dialogue.Show(...)` silently no-ops.

- **Canvas**: Screen-Space-Overlay, Pixel Perfect on.
- **Canvas Scaler**: Scale With Screen Size, Reference Resolution **320 × 180**,
  Reference Pixels Per Unit **16**, Match **0.5**.
- **Root stays active** so the service can register; **Panel child is the
  toggled visual** (the service's `_visualRoot`).
- **Panel Image**: 9-sliced sprite from `Modern_UI_Style_1`. Tint color must
  have **alpha = 1** (a common mistake is leaving it lower → semi-transparent box).
- **Text (TMP)**: uses **`m5x7`** pixel font, generated in **SDFAA mode** at
  Sampling Point Size 16. Display sizes must be **integer multiples of 16**
  (16, 32, 48) or the text gets blurry.

## World

- **Grid** layers: `Ground`, `Walls`, `Water`, `Trees`, `Interactable`,
  `EnvironmentBlock`.
- **Collision** lives on **Walls** (TilemapCollider2D + Static Rigidbody2D +
  CompositeCollider2D, "Used By Composite"). Don't put a TilemapCollider2D on
  `Ground` — every tile in the Serene Village set has `m_ColliderType: Sprite`,
  so a collider on Ground would make the whole floor solid.
- **Water** is animated through an `AnimatedTile` (`Water_Anim`). For all
  cells to animate in lockstep, set `MinSpeed == MaxSpeed` (TMP's internal
  randomization is per-cell otherwise — looked-like-static cells at low rolls).
- **Interactable** layer hosts world-side interactables (GameObjects with a
  `Collider2D` + an `IInteractable` component, *not* painted tiles).
- **Tileset**: Serene Village 16×16. UI uses `Modern_UI_Style_1`. Character
  sprites in `Adam_*` / `Bob_*` / `Alex_*` / `Amelia_*` sheets.

## Conventions

- **PPU = 16** everywhere: world sprites, tilemaps, and UI Canvas Scaler
  `Reference Pixels Per Unit`. This is what keeps world and UI on one pixel grid.
- **Filter Mode = Point** on every sprite.
- **Compression = None** on textures (no DXT/ETC artifacts on pixel art).
- **Sprite Extrude = 1** (prevents tile-seam bleeding).
- **No namespaces** — the existing scripts don't use them; stay consistent.
- **Comments explain *why***, not what. Well-named identifiers cover *what*.
  The `// Mirror of IMovementInput: ...` comment style is intentional —
  cross-link patterns so future readers see the symmetry.
- **No "Debug.Log" left in shipping code paths.** Awake-time validation is
  fine; per-frame logs are not.

## Folder layout

```
Assets/
  Animations/Controllers/   Animator Controllers (Player.controller)
  Animations/Animations/    .anim clips
  Fonts/                    Pixel fonts (m5x7.ttf, m5x7.asset)
  Prefabs/                  Player.prefab, DialogueUI.prefab
  Scenes/                   SampleScene.unity
  Scripts/
    Input/                  IMovementInput, PlayerInputReader, InputSystem_Actions
    Player/                 PlayerMotor, PlayerController, PlayerState,
                            IMovementGate, AnimatorView, ICharacterAnimator,
                            PlayerAnimationDriver, PlayerInteractor
    Interaction/            IInteractable, TextInteractable
    UI/                     IDialogueService (+ Dialogue locator),
                            CanvasDialogueService
  Sprites/Tilemaps/         Serene_Village, Modern_UI_Style_1, character sheets
  Sprites/Animated stuff/   water_waves, campfire, door
```

## History and likely next directions

[CHANGELOG.md](CHANGELOG.md) tracks what's been built and what changed. Likely
next features, in roughly increasing complexity:

- **NPC interactable** (`NpcInteractable` implementing `IInteractable`) with a
  named character.
- **Branching dialogue / typewriter effect** — extend `CanvasDialogueService`
  or swap in a richer impl; `TextInteractable` doesn't change because it
  depends only on `IDialogueService`.
- **Inventory + chest interactable** (`ChestInteractable`, `PickupInteractable`).
- **Day/night cycle and time system.**
- **Tool use** — `PlayerState.UsingTool` is already wired through `CanMove`;
  just needs an action layer that drives the transition.
- **Cutscene director** — would compose `Dialogue.Show` + `PlayerState.InEvent`
  the same way the current dialogue flow does.

## Quick pitfall checklist (lessons from past fixes)

- **Player won't collide with walls** → player Rigidbody2D must be **Dynamic**
  (not Kinematic), and its body collider must not be `Is Trigger`.
- **A trigger script doing nothing** → script attached to the wrong GameObject
  (commonly the Player root instead of `InteractorZone`).
- **Service self-disabling on `OnEnable`** → the script that toggles a
  GameObject must never live on the GameObject it toggles. (`CanvasDialogueService`
  is on the root; it toggles the **Panel child**.)
- **Pixel-art UI blurs** → check PPU 16 on the Canvas Scaler, Point filter,
  and font sizes that are *integer multiples* of the sampling size.
- **Walking through walls after fixing the Rigidbody** → look for `m_IsTrigger`
  on the body collider; it's a classic confusion with the InteractorZone.
- **Tilemap edits don't appear in scene file** → the scene wasn't saved.
  Always ⌘S before inspecting YAML.
- **Every slot in an array shows the same thing at runtime** → the prefab
  asset was dragged from the *Project window* into a `MonoBehaviour[]` field
  instead of the scene instances being dragged from the *Hierarchy*. Unity
  accepts this silently and produces N identical entries that all point at
  the prefab asset's component. Fix: clear the array, drag the actual scene
  instances from the Hierarchy. YAML tell: every entry should have a unique
  `fileID` and **no** `guid: ... type: 3` suffix — that suffix means
  "Project window drag."
- **Two `IInteractable` components on one GameObject** → `GetComponent<IInteractable>()`
  returns only the first, silently shadowing the other. One interactable per
  GameObject; compose behavior inside a single component if you need both.
