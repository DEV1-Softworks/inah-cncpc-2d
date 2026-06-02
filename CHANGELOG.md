# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.3] - Unreleased

### Added
- Branching dialogue: `DialogueChoice` struct + new `choices` field on `DialogueLine`. Lines with one or more choices pause the conversation until the player picks; each choice jumps to a `targetLine` index (or ends the conversation when the target is out of range).
- `IDialogueService.MoveSelection(int delta)` and `IsAwaitingChoice`, plumbed through the static `Dialogue` locator. `Advance()` now confirms the highlighted choice when one is awaited, or proceeds linearly otherwise.
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

[0.0.3]: https://semver.org/
[0.0.2]: https://semver.org/
[0.0.1]: https://semver.org/
