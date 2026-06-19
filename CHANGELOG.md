# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-06-18 — Alpha Release

This is the first formal **alpha release**, consolidating all prior iterative work (0.0.1 → 0.0.5) plus the systems built since the tool/world-resource pass. The project also now has a name — **ESTELA** — that frames it as a franchise umbrella with one subtitle per archaeological zone (the first being *Hoyo Negro*). The repo folder (`INAHWithNoName`) stays unchanged to avoid local-path churn; everything user-facing uses ESTELA.

### Title, identity and narrative
- **Project named *ESTELA*.** Triple-meaning: the protagonist (a young woman called Estela), *stele* (the carved monuments pre-Columbian cultures left behind), and *trail / wake* (what something leaves in passing). The three readings stack across the arc — Estela follows the trail of paleoamerican populations and, by supporting the research, leaves her own.
- **Protagonist's gender resolved** as a young woman called Estela. This closes the open trait left in §5.2 of the GDD.
- **GDD (Game Design Document)** authored in two versions: the full edition (~40 pages) and a synthesized 8–10 page edition for institutional delivery. Includes the §2.4 commitments around the cultural treatment of Sac Actún and Hoyo Negro, the Pillar-2 economy framing, and the §9 risk register / cronograma.
- **Jam deadline tightened from 2026-08-08 to 2026-07-11.** Risk R2 (calendar pressure) escalated; cronograma compressed accordingly.

### Added — Time and day/night
- **`GameTime` static locator + `ITimeService`** — a single contract anyone in the game uses to read the current day, hour, minute, phase, and `DayProgress01`. Mirrors the Dialogue / Inventories pattern.
- **`TimeService` MonoBehaviour** — the single concrete clock. Configurable speed (default 1 real second = 1 in-game minute), configurable phase boundaries (Morning / Afternoon / Evening / Night), auto-pause during blocking overlays (so time can't sneak past while a menu is open). Tracks `_totalSeconds` as `double` to avoid float drift in long sessions.
- **`DayNightLightDriver`** — drives a URP 2D global `Light2D` color + intensity by sampling a Gradient + AnimationCurve at `GameTime.DayProgress01`. Visual day/night without writing per-time-of-day logic.
- Event surface (`OnDayChanged`, `OnHourChanged`, `OnPhaseChanged`) fires only on real transitions — subscribers (HUD, NPC schedules, shop hours) don't poll.

### Added — Scene transitions and persistence
- **`SceneTransition` orchestrator** — static `Go(scene, spawnId)` entry point. Lazily self-creates a DDOL GameObject with a fade canvas built programmatically (no scene authoring required). Coroutine: lock player → fade to black → `LoadSceneAsync(Single)` → find `SpawnPoint` → teleport player → fade from black → unlock.
- **`SpawnPoint` marker component** with a string `_id`. Each inbound door references the spawn id in the destination scene.
- **`DoorInteractable`** — `IInteractable` that calls `SceneTransition.Go(_targetScene, _targetSpawnId)`. Same component for outbound and return doors.
- **`PersistentRoot`** + the convention of grouping the Player, Main Camera, Cinemachine VCam, UIs, and `TimeService` under a single `Persistent` GameObject that marks itself `DontDestroyOnLoad`. Includes a duplicate-guard for second loads of the world scene and a `ResetInstance()` static for the title screen flow.
- **`CameraFollow`** — simple 2D follow camera as a fallback / convenience component (the project ended up on Cinemachine; this stays available for scenes that don't have a VCam).
- **Editor wizard `Tools → INAH → Create Interior Scene...`** — scaffolds a new interior scene (Grid + Tilemap + arrival SpawnPoint + return DoorInteractable) and adds it to Build Settings in one click.

### Added — Restricted-access interactable
- **`RestrictedAccessInteractable`** — gates entry behind an authorization flag. Default state plays a "denied" Conversation; a `GrantAccess()` seam allows future story logic to flip on a "granted" Conversation. First use case: the cenote entrance refuses entry to anyone but INAH personnel — turning the GDD's Pillar 2 ("Sac Actún is untouchable") into a moment-to-moment player experience.

### Added — Mobile (Android / iOS / web touch) compatibility
- **`MobileInput` static one-frame-edge bus** for on-screen taps. `PressInteract` / `PressUse` / `PressDrop` stamp the current `Time.frameCount`; matching `*PressedThisFrame` properties mirror `WasPressedThisFrame` semantics.
- **`MobileButton` component** — `IPointerDownHandler` on a UI Image, routes the press to the matching `MobileInput` flag (or, for `Pause`, directly to `PauseMenu.Open()`).
- **`MobileGamepadVisibility`** — drives the on-screen gamepad's `CanvasGroup` (alpha + interactable + blocksRaycasts) based on platform AND pause state. Subscribes to `PauseMenu.OnOpened` / `OnClosed` so the controls hide while the pause UI is up. `_showInEditor` toggle for layout iteration.
- **`HotbarUI` now listens to slot clicks** via `HotbarSlotView.OnClicked` — tap-to-select for mobile, also works as left-click for desktop.
- **`PlayerInputReader` OR-s** the action-map intents with `MobileInput` flags, so existing `IMovementInput` consumers (`PlayerInteractor`, `PlayerUseHandler`, `PlayerDropHandler`) work unchanged.
- Tech-debt signposted: virtual gamepad bypasses the Input Actions asset for `Drop`, `Pause`, and hotbar slot selection. Post-jam rebinding pass will move these into proper actions and delete the `MobileInput` bus.

### Added — Economy: wallet + vendors
- **`Wallet` static locator + `IWallet`** — pesos: `Add(amount)` always succeeds, `TrySpend(amount)` returns false if insufficient, `OnChanged` fires the new balance.
- **`PlayerWallet`** concrete component. Lives under Persistent. Broadcasts the current balance in `OnEnable` after `Register`, so HUD listeners that woke up before the wallet initialise correctly (race-resilient).
- **`Item.SellPrice` and `Item.BuyPrice`** — both default to 0 (a zero in either direction means "not transactable that way"; the vendor UI auto-disables the slot).
- **`VendorInteractable`** — `IInteractable` with a `VendorMode` (SellOnly / BuyOnly / Both) and a `_stock: List<Item>` for the buy panel. Pattern-locked: the same component runs the *comerciante del pueblo* (Both) and the *Oficina del INAH* desk (BuyOnly).
- **`IVendorUI` + `Vendors` locator + `VendorSession`** — mirror of the Chests / Dialogue locator shape with `OnOpened` / `OnClosed`.
- **`VendorUI`** modal — sell panel (player inventory rendered with sell prices) + buy panel (vendor stock with buy prices and affordability tints), live wallet display. Click to sell or buy one unit at a time.
- **`VendorSlotView`** — reusable slot for both panels. Icon + count + price + disabled tint when not transactable.
- **`WalletHudView`** — persistent HUD readout. Listens to `Wallet.OnChanged`. Format string configurable.

### Added — INAH office: specialist hiring system
- The Pillar-2 second basket from the GDD ("supporting the research") rendered as a recruit UI rather than as a separate counter — the player spends pesos at the **Oficina del INAH** like at any vendor, but the items they fund (supply kits, ropes, mapping gear, specialist weeks) are *contributions*, not personal goods.
- **`ExpertSpecialist` ScriptableObject** — authored data for each hireable specialist: id, display name, specialty, portrait, description, hire cost.
- **`HiredExperts` static set** — `Hire(expertId)`, `IsHired(expertId)`, `OnHired` event.
- **`ExpertNpc`** tag component — pinned on the world NPC that should appear once that expert is hired (a stable id matched against `HiredExperts`).
- **`SceneExpertActivator`** — placed on an always-active container; at scene start activates any child whose expert is already hired, and listens to `OnHired` so a hire during the current session materialises the NPC immediately. The container-as-listener pattern avoids the "script deactivates itself" deadlock.
- **`HireOfficeInteractable`** — opens the hire modal with a configured roster of `ExpertSpecialist`.
- **`IHireOfficeUI` + `HireOffices` locator + `HireOfficeSession`** — same shape as `Vendors`.
- **`HireOfficeUI`** — two-column modal. Left: scrollable list of specialists (each row shows portrait, name, specialty, status badge, cost). Right: detail panel with the selected specialist's portrait, description, and a *Contratar* CTA that grays out when unaffordable or already hired.
- **`ExpertListItemView`** — selectable row in the left column with `IPointerClickHandler`. Status badge live-updates on `HiredExperts.OnHired`.
- First expert authored: **Romina Vázquez**, espeleobuza (cost 2000 pesos). Activation flips on a (currently inactive) NPC GameObject in the INAH scene.

### Added — Title screen and pause flow
- **`TitleScreen.unity`** scene — first scene in Build Settings. Minimal: Camera + Canvas + EventSystem + buttons. Calls into `SceneFlow`.
- **`TitleScreenUI`** controller — `OnStartClick()` → `SceneFlow.StartNewGame()`, `OnQuitClick()` → `SceneFlow.Quit()`.
- **`SceneFlow`** static helper — centralises `StartNewGame()`, `ReturnToTitle()`, and `Quit()`. Both navigations destroy the existing `PersistentRoot` and call `PersistentRoot.ResetInstance()` before loading the target, so "Iniciar" always boots a fresh world (no save system yet).
- **`PauseMenu` static locator + `IPauseMenu`** — same overlay pattern as Vendors / Chests / HireOffices.
- **`PauseMenuUI`** — Canvas under Persistent. Toggles open/closed on Escape (direct `Keyboard.current` read, signposted for the post-jam rebinding pass). On open: `Time.timeScale = 0` (which auto-pauses `TimeService` because it reads `Time.deltaTime`). Resume / Volver al menú / Salir buttons.
- **`HideOnWebGL`** component — disables itself when `Application.platform == WebGLPlayer`. Wired on Quit buttons so the WebGL build doesn't show a button that can't function.

### Added — Dialogue / NPC reactivity
- **`NpcInteractable` extended with first-encounter support** — optional `_firstEncounter` Conversation slot plus a `_conversation` default. First interaction plays first-encounter (if set); subsequent interactions play default. `_hasSpokenBefore` flag flips at the moment of interaction (matches Stardew semantics; resets per session for now — to be serialised when save/load ships).
- **`NpcInteractable` documentation note**: the in-memory `_hasSpokenBefore` is the first thing to serialise once save/load lands.

### Changed
- **`PlayerController.OnEnable` / `OnDisable`** now subscribe to all four overlay locators (`Dialogue`, `Chests`, `Vendors`, `HireOffices`, `PauseMenu`) — movement locks for free across every modal.
- **`PlayerInteractor`** — early-outs on `PauseMenu.IsOpen`, and routes Interact to close the active overlay (Chest / Vendor / HireOffice) before falling through to dialogue / world interaction.
- **`PlayerUseHandler` and `PlayerDropHandler`** — early-out when *any* of the five blocking overlays is open. (Signpost from 0.0.4 still pending: consolidate into a shared `Overlays.AnyOpen` aggregator.)
- **`SceneTransition.TeleportPlayer`** — after moving the player, calls `OnTargetObjectWarped(target, delta)` on every `CinemachineVirtualCamera` in the scene. Eliminates the brief Z-axis tilt / skid that damping produced during fade-in of a new scene.
- **`PersistentRoot.Awake`** — adds explicit error / warning logs when the GameObject isn't a scene root or has no children. Surfaces the most common DDOL configuration mistakes immediately.
- **`MobileGamepadVisibility`** rewritten to drive a `CanvasGroup` instead of `SetActive`. The previous `SetActive(false)` approach disabled the script's own GameObject, leaving no way to re-show controls after pause closed.

### Documentation
- **`CLAUDE.md`** kept current across the alpha cycle. Added new sections for the Economy, the title/pause/scene flow, and a substantially expanded pitfall checklist:
  - `DoorInteractable` needs `Is Trigger`.
  - Persistent GameObject + Cinemachine VCam parenting under Persistent (otherwise a black-screen-after-transition mystery).
  - `SpawnPoint` global vs. local position.
  - Manually-created Canvas missing `GraphicRaycaster` → silent click loss.
  - `OnScreenStick` with RectTransform Scale (0,0,0) / wrong Pivot / wrong AnchoredPosition.
  - Cinemachine warp-on-teleport requires `OnTargetObjectWarped`.
  - Vertical scroll list growing bottom-to-top (Content Pivot Y must be 1).
  - Inner UI slot prefab carrying its own Canvas without GraphicRaycaster → swallowed clicks.
  - Empty Button `onClick` list (no warning, silent failure).
  - Slot in vendor / hire UI showing placeholder data when the script field isn't wired.
- **`NICE_TO_HAVE_WISHLIST.md`** extended with: shop open / close hours via GameTime, NPC daily schedule (phase-driven movement), daily world tick on day rollover, and the **Quests system** (full proposal with data model, locator, and acceptance criteria).

### Signposts captured in code
- The five overlay locators (`Dialogue`, `Chests`, `Vendors`, `HireOffices`, `PauseMenu`) all share the same shape. Either keep adding new ones the same way, or — once a sixth ships — extract a shared `Overlays.AnyOpen` / event aggregator and have `PlayerController` and the handlers subscribe to *that* instead of OR-ing growing lists.
- `MobileInput`, the `Drop` keyboard read, and the hotbar number-key read all bypass the Input Actions asset. The post-jam rebinding pass collapses all three into proper actions.
- `_hasSpokenBefore` on `NpcInteractable` lives in memory only. First field on the save/load list.
- `HiredExperts` and `Wallet` likewise — their state is session-only until save/load ships.
- The cenote's `RestrictedAccessInteractable._granted` slot is the seam for the day the arc unlocks player access. The narrative event that flips it will also be the first use of a future `StoryFlags` / cutscene director.

## [0.0.5] - Unreleased

### Added
- **Tool / world-resource system** — the first real "do something to the world" loop. Composes existing inventory + tilemap + facing primitives; one shared chop behavior runs every tool that damages tiles in front of the player.
- **`WorldTilemaps` static locator** + **`WorldTilemapRegistrar` component** — each named tilemap (Trees, Walls, Ground, Water, custom layers) registers itself in `OnEnable` so tool behaviors held as ScriptableObjects can reach it without serialized scene refs (which SOs cannot hold).
- **`PlayerAnimationDriver.Facing` is now public** — same cardinal direction the animator reads; tools query it to know which cell is "in front of the player" even while the player is standing still.
- **`ChopTreeUseBehavior`** (`ItemUseBehavior` subclass) — on Use, computes the cell one step in `Facing` from the player on the chosen tilemap (`_targetKey`), applies `_damagePerHit`, and on depletion clears the cells + spawns a configurable drop (`_dropItem` × `_dropCount` from `_dropPrefab`). Item is not consumed.
- **`TilemapResourceHealth` component** (on the resource tilemap) — per-cluster HP tracking. On `Damage(cell, amount)` it flood-fills 4-neighbors from the hit cell to discover the connected resource, keys HP against the cluster's canonical lex-min anchor (so any chop on the same tree damages the same HP), and on depletion returns the whole cluster's cells for the caller to clear. Memory stays small because only damaged clusters are stored.
- **Required-tool gate on `TilemapResourceHealth`** — `_requiredTool` (Item ref) + `AcceptsTool(item)`. Stardew-style: rocks need a pickaxe, trees need an axe; a wrong tool no-ops cleanly even if it would otherwise hit the right code path.
- **`ChoppableTrees` tilemap layer** in the scene (decorative `Trees` layer stays untouched). `WorldTilemapRegistrar` publishes it under key `"ChoppableTrees"`; the `AxeChopTree` behavior asset targets that key.
- **`Axe` Item** + **`Wood` Item** + **`AxeChopTree` `ChopTreeUseBehavior` asset**, wired through `Axe._useBehavior`. Drop is `Wood × 1` using the same `DroppedItem.prefab` `PlayerDropHandler` already uses.

### Signposts captured in code
- `ChopTreeUseBehavior` is named for trees but is actually a generic "damage the tile in front of me." When the second use case (mining, scything) lands, either reuse this behavior with a different `_targetKey` + tool, or rename to `DamageTileUseBehavior` if the swing styles diverge enough (sounds, particles).
- Visual mid-chop feedback (damaged-tile sprite swap on each non-fatal hit) is deliberately deferred — the slot to add it is `Damage()` returning `false`: also call `map.SetTile(cell, damagedVariant)`. Skipped until art exists.
- Tool tiers (better axe → more `_damagePerHit`) are expressible as just different behavior assets today. A more principled "tier" system would attach a level to the *item* and have the behavior read it; not built yet, not needed.

## [0.0.4] - Unreleased

### Added
- **Inventory data model** — `Item` ScriptableObject (display name, description, icon, max stack, optional `ItemUseBehavior`), `ItemStack` value struct with canonical `Empty`.
- **`IInventory` contract** + `InventoryBase` abstract MonoBehaviour with the shared slot-array implementation; `PlayerInventory` and `ChestInventory` inherit. `IInventory.TryRemoveFromSlot(slotIndex, count)` for slot-targeted removal (used by chest click-transfer and drop).
- **`Inventories` static locator** — `Inventories.Player` registers automatically; pickups, hotbar, chest UI, and future shops all reach the player's inventory through it.
- **`PickupInteractable`** — drop on a world object with a `Collider2D` to make it harvestable; calls `Inventories.Player.TryAdd` on Interact. `Configure(item, count)` runtime setter so a single prefab can be instantiated and parameterized.
- **`HotbarUI` + `HotbarSlotView`** — row of pre-created slot views, live-updated via `OnChanged`, with `> ` highlight on the selected slot. Number keys 1–9 / 0 / `-` / `=` select slots 0–11.
- **`IHotbarUI` + `Hotbar` static locator** — `Hotbar.SelectedSlot` exposes the current selection to the player-use code without serialized wiring.
- **`HotbarSlotView.IPointerClickHandler`** + `OnClicked` event — the hotbar ignores it; the chest UI consumes it for click-to-transfer.
- **`ChestInteractable`** + `ChestInventory` — each chest GameObject is its own independent storage (`RequireComponent` enforces both on the same GameObject).
- **`IChestUI` contract + `Chests` static locator** with `OnOpened` / `OnClosed` events fired only on real transitions.
- **`ChestUI`** — drives a panel with two stacked grids (chest above, player below). Click a slot to transfer its stack to the other side; transfers respect `TryAdd` capacity and remove via `TryRemoveFromSlot`.
- **Player lock during chest** — `PlayerController` subscribes to `Chests.OnOpened` / `OnClosed` and toggles `PlayerState.InEvent` / `Free` (same pattern as dialogue, so the existing motor gate handles the lock for free).
- **Interact button closes the chest** — `PlayerInteractor` checks `Chests.IsOpen` before dialogue/interactable routing.
- **`ItemUseBehavior` (abstract ScriptableObject)** + two concrete starters: `ConsumableUseBehavior` (logs + consumes 1) and `DebugLogUseBehavior` (logs + leaves stack alone). `Item._useBehavior` slot is optional — items without one are silent on Use.
- **`PlayerUseHandler`** — reads `UsePressed` from the input layer, runs the selected hotbar slot's `ItemUseBehavior`, deducts 1 from the stack if the behavior returns true.
- **`PlayerDropHandler`** — reads `DropPressed`, instantiates a `DroppedItem` prefab at a configurable offset, calls `PickupInteractable.Configure` with the selected stack's item + count, and empties the slot.
- **`IMovementInput.UsePressed` and `DropPressed`** — `PlayerInputReader` backs `UsePressed` with the Attack action (default left mouse / gamepad RT) and `DropPressed` with a direct keyboard read of `Q` (same pragmatic shortcut as the hotbar number keys).
- **`InventoryDebugLogger`** (temp dev tool) — logs the player's inventory state to the Console on every `OnChanged`. Useful for verifying pickups / drops / transfers without UI noise.

### Changed
- **Inventory slot prefab consolidated** into a single `Assets/Prefabs/InventorySlot.prefab` with the *correct* internal structure: root has a real background Image (the slot frame, always visible), `Icon` child is sprite-less by default (gets set at runtime by `SetStack`), `Count` child is a `TMP_Text` (auto-hidden when empty or count = 1), and `SelectionHighlight` is a separate overlay GameObject **inactive by default** that toggles on only for the selected slot. All 36 slot instances across `HotbarUI.prefab` (12) and `ChestUI.prefab` (12 chest + 12 player) are now instances of this one prefab — edit the frame once and every slot updates. Replaces the previous "SelectionHighlight doubling as the slot frame" hack and removes the whole class of bugs where touching one slot left the other 35 broken.

### Fixed
- `ChestUI.RefreshGrid` no longer force-deactivates each slot's `SelectionHighlight`. The previous "defensive" call was hiding the *entire slot* whenever the slot prefab used the highlight as its de facto background frame (no root Image present). Selection state in the chest UI is now left alone — slots stay visible regardless of how the slot prefab is composed.
- Clicking a slot in the chest UI no longer also fires `PlayerUseHandler` and consumes one of the clicked item. `PlayerUseHandler` and `PlayerDropHandler` now early-out on `Chests.IsOpen || Dialogue.IsShowing`, mirroring the guard `PlayerInteractor` already had. (Signpost: when a third blocking overlay is introduced, consolidate into a shared `Overlays.AnyOpen` aggregator instead of OR-ing growing lists in each handler.)
- Slot array fields (`HotbarUI._slots`, `ChestUI._chestSlots`, `ChestUI._playerSlots`) were briefly wired by dragging `InventorySlot.prefab` from the *Project window* — Unity accepts a prefab-asset reference into a `MonoBehaviour[]` field with no warning, producing 12 identical entries that all point at the same prefab-asset component. Re-wired by dragging from the *Hierarchy* (the actual scene instances under each grid). YAML check for future paranoia: each entry should have a unique `fileID` and **no** `guid: ... type: 3` suffix — that suffix is the tell that you dragged from Project instead of Hierarchy.

### Signposts captured in code
- `IMovementInput` is no longer strictly "movement" — it now carries `InteractPressed` / `UsePressed` / `DropPressed`. Rename to `IPlayerInput` when the next input pass happens.
- Hotbar number-key reading and Drop key reading both poll `Keyboard.current` directly. When binding becomes a real concern (gamepad / rebinding), move both behind dedicated actions in the Input Actions asset.

## [0.0.3] - Unreleased

### Added
- Branching dialogue: `DialogueChoice` struct + new `choices` field on `DialogueLine`. Lines with one or more choices pause the conversation until the player picks; each choice jumps to a `targetLine` index (or ends the conversation when the target is out of range).
- `IDialogueService.MoveSelection(int delta)` and `IsAwaitingChoice`, plumbed through the static `Dialogue` locator. `Advance()` now confirms the highlighted choice when one is awaited, or proceeds linearly otherwise.
- `IDialogueService.PlayConversation(lines)` (and the matching `Dialogue.PlayConversation` proxy) — explicit graph-traversal entry point alongside the linear `ShowSequence`. Same data shape, different semantics: in conversation mode a no-choice line ends the branch instead of falling through to the next array index.
- Choice navigation in `PlayerInteractor`: edge-detected `Move.y` while a choice is awaited (configurable `_choiceNavThreshold`) so holding the stick selects one option per push instead of scrolling at frame rate.
- `CanvasDialogueService` choices UI: `_choicesPanel` toggleable container + `_choiceTexts` row array, auto-populated with the current line's choices, highlight via configurable `_selectedChoicePrefix` / `_unselectedChoicePrefix`.
- Speaker name plate on `DialogueUI.prefab`: TMP_Text + sliced background that auto-hides on lines with no speaker, restructured to sit *behind* the main Panel as a Stardew-style tab.
- ChoicesPanel with 9-sliced background and per-row TMP slots in the dialogue prefab.
- PixelifySans variable-font TMP font asset, generated with full Latin-1 Supplement (Spanish/French/German accents: `¡¿áéíóúñÁÉÍÓÚÑ` and more).
- `CLAUDE.md` at the project root — architecture, conventions, and pitfall checklist so any future session bootstraps with the full context.
- `NICE_TO_HAVE_WISHLIST.md` for post-alpha polish items (first entry: input-aware advance glyph that shows the current device's button: `E` / `A` / `X`).

### Fixed
- `CanvasDialogueService.Hide()` now explicitly deactivates the speaker plate, advance indicator, and choices panel — needed because they may be siblings of the main visual root (e.g. the speaker tab), so deactivating the Panel alone wouldn't hide them.
- `m5x7` font asset regenerated with Latin-1 Supplement (previously ASCII-only, causing Spanish characters like `¡`, `¿`, accented vowels and `ñ` to render as missing glyphs).
- "Two `IInteractable`s on one GameObject" foot-gun documented in `CLAUDE.md` pitfall list (NpcInteractable can be silently shadowed by a leftover TextInteractable; rule: one `IInteractable` per interactable GameObject).
- Conversation leaf nodes no longer fall through to the next array index. Previously a no-choice "No-path" terminal line could bleed into a "Yes-path" line that happened to sit next in the data; now `NpcInteractable` uses the new `Dialogue.PlayConversation` entry point so the service treats no-choice lines as terminal leaves and ends the branch on Advance.

## [0.0.2] - Unreleased

### Added
- Interaction system: `IInteractable` interface and `PlayerInteractor` (trigger-zone child of the Player; tracks overlapping interactables and fires the closest on `InteractPressed`).
- `TextInteractable` — attach to any world object with a `Collider2D` to make it readable; per-instance `_message` field renders through the dialogue UI.
- Dialogue UI system: `IDialogueService` contract + `Dialogue` static service-locator (with `OnShown`/`OnHidden` events) + `CanvasDialogueService` MonoBehaviour implementation.
- `DialogueUI.prefab` — Screen-Space-Overlay Canvas, Pixel Perfect on, Canvas Scaler at 320×180 reference (PPU 16, Match Width-or-Height 0.5), 9-sliced Panel from the Modern UI sheet, child `TMP_Text` rendered with the m5x7 pixel font (SDFAA at sampling size 16).
- Modern UI Style 1 sprite sheet for UI; 9-slice border set on the dialogue Panel sprite.
- `m5x7` pixel-art TMP font asset.
- Interact-to-close: pressing Interact while dialogue is open hides it (handled in `PlayerInteractor`, not the interactables, so every `IInteractable` stays ignorant of dialogue state).
- Movement lock during dialogue: `PlayerController` subscribes to `Dialogue.OnShown` / `OnHidden` and toggles between `PlayerState.InEvent` and `Free` — any future system that opens dialogue (cutscene, NPC, chest) locks the player automatically.

## [0.0.1] - Unreleased

### Added
- Player movement via `PlayerMotor` (physics-based, `Rigidbody2D.MovePosition` in `FixedUpdate`) with walk speed, sprint multiplier, and diagonal normalization.
- Input layer: `IMovementInput` interface implemented by `PlayerInputReader` (single owner of the new Input System actions).
- Movement gating: `IMovementGate` / `CanMove`, owned by `PlayerController` (the player "brain") with a single `SetState` mutation point.
- `PlayerState` commanded modes: `Free`, `UsingTool`, `InEvent`.
- `PlayerMotor.Velocity` exposed (units/second) for systems that need to read motion.
- Four-directional sprite animation: `ICharacterAnimator` interface, `AnimatorView` (owns Unity `Animator`), and `PlayerAnimationDriver` (reads `Velocity`, snaps to cardinal facing, drives the controller).
- `Player.controller` animator with Idle/Walk 2D blend trees keyed on `MoveX`/`MoveY` and a `Moving` bool.
- Tilemap world (Grid layers: Ground, Walls, Water, Trees, and others) using the Serene Village 16×16 tileset.
- Animated water layer via an `AnimatedTile` (`Water_Anim`).
- `Player` prefab.
- `.gitignore` for Unity projects.

### Changed
- `PlayerState` reduced to commanded modes only; locomotion (idle/walk) is derived from velocity by the animation layer rather than stored as state.
- `PlayerController` no longer resolves an input source — `PlayerMotor` is the sole input consumer.

### Removed
- Stealth/disguise/crouch scope (`Hide`/`Disguised` states, `CrouchPressed` input intent) as out-of-genre for a Stardew-like.

### Fixed
- Player passing through walls (Rigidbody2D set to Dynamic; collider was a trigger).
- Animator not transitioning (parameter name and Move blend-tree Y-axis mismatch; transitions' Has Exit Time).
- Water tiles animating out of sync (`AnimatedTile` min/max speed set equal).

[0.1.0]: https://semver.org/
[0.0.5]: https://semver.org/
[0.0.4]: https://semver.org/
[0.0.3]: https://semver.org/
[0.0.2]: https://semver.org/
[0.0.1]: https://semver.org/
