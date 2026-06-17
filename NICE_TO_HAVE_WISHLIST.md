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

## Shop open / close hours driven by GameTime

**What.**
A shop interactable that is only usable during a configured window (e.g. 08:00
to 20:00). Outside hours the door shows a "Closed" prompt instead of opening
the chest/menu.

**Why post-alpha.**
The clock exists and fires `OnHourChanged` — the rule is trivial to add when
the first shop NPC arrives. Doing it earlier means inventing a shop with no
shop content.

**Where.**
New `ShopInteractable` (implements `IInteractable`). Reads `GameTime.Hour` on
`Interact()` and either opens the menu or shows a dialogue line.

**Technical sketch.**
- `[SerializeField] int _opensAt = 8, _closesAt = 20;`
- `Interact()` checks `GameTime.Hour` against the window; calls
  `Dialogue.Show("The shop is closed.")` outside it.
- Optionally subscribes to `GameTime.OnHourChanged` to swap a sprite (open
  sign vs closed sign) live.

**Estimated effort.** ~45 minutes once the first shop NPC and inventory list
exist.

---

## NPC daily schedule (phase-driven movement)

**What.**
An NPC walks between locations as the day progresses (home in the morning,
shop at midday, pub in the evening, home at night). Schedule data lives on the
NPC, not in a central timetable.

**Why post-alpha.**
Needs NPCs and pathfinding first. Once those exist this is a thin layer on
top of `GameTime.OnPhaseChanged`.

**Where.**
New `NpcSchedule` component on each NPC. Reads `GameTime.OnPhaseChanged` and
sets a target waypoint Transform.

**Technical sketch.**
- `[Serializable] struct Stop { DayPhase phase; Transform location; }`
- `[SerializeField] Stop[] _stops;`
- On `OnPhaseChanged(p)` → look up stop → tell the NPC's mover to walk there.
- Standalone from animation/movement (depends on whatever NPC mover ships).

**Estimated effort.** ~2 hours once NPC movement exists.

---

## Daily crop / world tick on day rollover

**What.**
Once per in-game day, planted crops advance one growth stage; spawners respawn
chopped trees; weather rerolls.

**Why post-alpha.**
Needs crops (planting/growing) and respawner systems first. The trigger itself
is one event subscription.

**Where.**
New `WorldDailyTick` MonoBehaviour, lives on a "World" GameObject in the main
scene.

**Technical sketch.**
- `OnEnable`: `GameTime.OnDayChanged += HandleNewDay;`
- `HandleNewDay(int day)`: iterate registered tickables — crops grow, choppable
  tree clusters consider respawning, etc.
- Tickables register themselves the same way interactables do (single locator
  list, no scene-wide singleton lookups).

**Estimated effort.** ~1 hour for the trigger + dispatcher; the per-system
logic ships with each system it serves.

---

## Quests system

**What.**
Structured objectives the player can pick up, track on a small HUD, and
complete to receive a reward — pesos, items, narrative beats, or a new
expert unlocked in the INAH office. Quests can come from any NPC and chain
into longer arcs (the arqueóloga asks for ten units of *X*, then for a
specific item from the cenote area, etc.). A quest log UI lists active and
completed quests with descriptions and progress.

**Why post-MVP.**
The jam vertical slice runs on "open-ended exploration + economic loop"
without explicit objectives — that's coherent with the GDD's Pillar 3
(*"curiosity as narrative motor"*). Quests are the next step in giving the
player concrete, satisfying goals, but they require: (a) a quest data
model, (b) a tracker / log UI, (c) hooks into inventory / dialogue / vendor
systems to detect completion, (d) per-quest content (givers, copy,
rewards). The infrastructure is two or three days of work and the content
production is open-ended. Better to ship the slice without it and add it
once the narrative voice is consolidated.

**Where.**
- `Assets/Scripts/Quests/` new folder.
- A small `IQuestLog` + `QuestLog` static locator, mirroring the
  established pattern (Wallet, Hotbar, Vendors, HireOffices).
- `Quest` ScriptableObject as authored data per quest.
- `QuestGiverInteractable` (or extend `NpcInteractable` to optionally
  offer a quest after the conversation).
- `QuestLogUI` HUD widget — collapsed by default, expand on a hotkey.

**Technical sketch.**
- `Quest` (ScriptableObject) fields:
  - `string Id`, `string Title`, `string Description`
  - `QuestObjective[] Objectives` — each is a polymorphic SO subclass
    (`DeliverItem`, `TalkTo`, `HireExpert`, `ContributeAmount`, etc.) with
    its own `IsComplete(...)` logic and a `Describe()` method for the log.
  - `int RewardPesos`, `Item[] RewardItems`, `Conversation OnComplete`.
- `QuestLog` static:
  - `Accept(Quest q)`, `Complete(Quest q)`, `Abandon(Quest q)`.
  - `IsAccepted(string id)`, `IsCompleted(string id)`.
  - `OnAccepted`, `OnCompleted`, `OnObjectiveProgress` events.
- Polling vs reactive: each `QuestObjective` decides. `DeliverItem` polls
  inventory once per second; `HireExpert` subscribes to `HiredExperts.OnHired`;
  `ContributeAmount` reads `Wallet.OnChanged`.
- The log UI subscribes to `QuestLog.OnObjectiveProgress` and repaints.

**Estimated effort.** ~12–16 hours for the infrastructure (data model,
locator, log UI, the first 3 objective subclasses). Per-quest content adds
on top.

**Acceptance criteria.**
- An NPC can offer a quest; the player accepts via a dialogue choice.
- The quest log shows the active quest with its objectives and live
  progress.
- Completing the objectives unlocks the "Turn in" dialogue option with the
  quest giver.
- Reward is delivered: pesos credited, items added to inventory, optional
  completion dialogue plays, expert is unlocked, etc.
- Completing a quest changes how the giver greets the player on subsequent
  conversations.

**Stretch.**
- Branching quests (multiple completion paths with different rewards).
- Failure conditions (time-limited quests; expire at end of day).
- Hidden quests (only appear after certain world conditions, like
  contributing N pesos to the INAH office).
- Companion: a tiny on-screen marker (a `!` over the giver, a `?` over a
  quest target) that subscribes to the log to reduce navigation friction.

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
