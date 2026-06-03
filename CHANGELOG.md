# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[0.0.4]: https://semver.org/
[0.0.3]: https://semver.org/
[0.0.2]: https://semver.org/
[0.0.1]: https://semver.org/
