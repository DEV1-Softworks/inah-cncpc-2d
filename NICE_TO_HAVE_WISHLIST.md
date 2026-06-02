# Nice to have wishlist

Post-alpha polish and "would be lovely" features intentionally deferred from MVP
delivery. Each entry is scoped enough that, when its time comes, a single
focused work session can ship it.

Format: **What** · **Why post-alpha** · **Where** · **Technical sketch** ·
**Estimated effort** · **Acceptance criteria** · **Stretch**.

---

## Input-aware advance glyph (E / A / X / etc.)

**What.**
The dialogue box's "press to continue" indicator shows the *actual button* the
current input device uses for the Interact action.

| Device | Glyph shown |
|---|---|
| Keyboard / mouse | `E` (or whatever Interact is rebound to) |
| Xbox controller | `A` |
| PlayStation controller | `Cross` (or the ✕ glyph) |
| Switch Pro / Joy-Con | `B` |

Updates live when the player hot-plugs a different controller mid-game.

**Why post-alpha.**
The existing static chevron already communicates "press to continue" perfectly
well. Input-aware glyphs are pure polish — they make screenshots and onboarding
sparkle but are not blocking gameplay. Worth doing once the alpha is stable and
input bindings are settled (so we're not re-doing this every time we change
which key Interact is mapped to).

**Where it lives.**
Inside the existing `_advanceIndicator` GameObject in `DialogueUI.prefab`. A
TMP_Text child renders the glyph; the chevron Image (if kept) stays alongside
as the "blinks-while-you-wait" affordance. `CanvasDialogueService` does not
change — it already only toggles the indicator GameObject as a whole.

**Technical sketch.**

1. **New component** `Assets/Scripts/UI/InteractPromptDisplay.cs`:
   - Holds a serialized `TMP_Text` reference.
   - Reads the current binding display string for the Interact action.
   - Subscribes to `UnityEngine.InputSystem.InputSystem.onDeviceChange` and
     refreshes the text on device add / remove.

2. **Expose the binding through the existing input layer** (don't read the
   Input System directly from UI code — that breaks our decoupling rule):
   - Add `string InteractBindingDisplay { get; }` to `IMovementInput`.
   - Implementation in `PlayerInputReader`:
     `_actions.Player.Interact.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)`.

3. The display component depends only on `IMovementInput`, wired via the same
   serialized-slot pattern used everywhere else in the project. Zero coupling
   to `CanvasDialogueService` or `Dialogue`.

**Estimated effort.** ~30 minutes. One new script, one interface addition, one
implementation line, one Inspector wire-up.

**Acceptance criteria.**
- Default keyboard → glyph reads `E`.
- Plug in a gamepad mid-play → glyph updates to the gamepad button (no
  recompile, no restart).
- Unplug gamepad → reverts to keyboard glyph.
- Glyph hides when dialogue closes (same lifecycle as the chevron).
- Rebinding Interact in the input asset → glyph reflects the new binding next
  time it appears.

**Stretch.**
Use a TMP sprite asset to render *actual icons* (a gamepad-A pictogram, the
PlayStation ✕ glyph) instead of letter strings. Requires sourcing or drawing a
small input-glyph atlas and registering it as a TMP Sprite Asset; the display
component would set `_text.text = $"<sprite name=\"{glyphName}\">"`.

---

<!--
  Future entries go below, same template:
  ## Title
  **What.** ...
  **Why post-alpha.** ...
  **Where.** ...
  **Technical sketch.** ...
  **Estimated effort.** ...
  **Acceptance criteria.** ...
  **Stretch.** ...
-->
