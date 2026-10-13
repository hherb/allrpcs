# Controls remapping: finite implementation proposal

Prepared 2026-10-13 from `f6e5babfa21debe997fd9a6d35679b4ff71baf27`. The owner requested continuation after publication.
The ledger selects **read-only planning**. This proposal completes that planning slice;
controls runtime, UI and persistence authority is still missing. Standing commit/push authority applies.
The companion [JSON contract](remapping-owner-scope.json) binds source bytes, exact defaults,
proposed files, limits and evidence. The remaining Phase 11 controls checkbox stays unchecked.

## Current controls and missing deliverable

All 21 authored InputMap actions have one physical keyboard event, device `-1`, deadzone `0.5`,
no modifiers, and no mouse or controller event. The committed and locally rewritten project files
have identical input sections; retain the unrelated editor file byte-for-byte.

| Action | Keyboard | Current owner |
| --- | --- | --- |
| `move_left` | A | `Seeker.cs` |
| `move_right` | D | `Seeker.cs` |
| `move_up` | W | `Seeker.cs` |
| `move_down` | S | `Seeker.cs` |
| `open_seeker_customization` | U | `SeekerCustomizationMenuController.cs` |
| `journal_toggle` | J | `JournalOpenCloseInputController.cs` |
| `map_toggle` | M | `SeekerMapOpenCloseInputController.cs` |
| `optional_overlays_toggle` | Tab | `OptionalOverlayVisibilityController.cs` |
| `save_current` | Q | `SaveLoadInputController.cs` |
| `load_current` | L | `SaveLoadInputController.cs` |
| `healer_toggle_follow_wait` | X | `Main.cs` |
| `healer_move` | H | `Main.cs` |
| `focused_search_start` | F | `Main.cs` |
| `focused_search_cancel` | C | `Main.cs` |
| `focused_search_complete` | E | `Main.cs` |
| `focused_search_resolve` | R | `Main.cs` |
| `layered_inscription_discover` | I | `Main.cs` |
| `remedy_prepare` | P | `OpeningRegionRemedyPreparationInputCoordinator.cs` |
| `remedy_administer` | T | `OpeningRegionFirstPatientAdministrationInputCoordinator.cs` |
| `sage_consult` | K | `OpeningRegionSageConsultationInputCoordinator.cs` |
| `character_turn_cancel` | V | `Main.cs` |

`Main._UnhandledInput` owns the search/shrine/Healer/turn commands; their existing qualification
and transitions remain authoritative. Preparation, treatment and Sage owners still perform their
existing range/state/identity checks. Save retains its existing map/journal initialization prerequisites; load retains the existing
storage boundary. Remapping changes the event admitted to these owners, never their transition rules.

Eight engine UI actions are also in scope: `ui_accept`, `ui_cancel`, `ui_left`, `ui_right`, `ui_up`,
`ui_down`, `ui_focus_next`, `ui_focus_prev`. A private temporary project containing the exact committed
input section enumerated their defaults on Godot 4.6.3 (`7d41c59c4`); the complete event codes,
modifiers, device values and deadzones are in JSON. Enter, keypad Enter and Space accept; Escape
cancels; arrows, D-pad and left stick navigate; Tab/Shift+Tab traverse focus. **This observed engine
map has no controller accept/cancel bindings.** The fixture only read InputMap and quit, with no
product scene or hardware event. Its sandbox log-directory crash was resolved by the identical
host command; the successful observation is inspection, not product/device verification.

Three contextual panels (`StreamWaterContactPanel`, `CaskCoverHandlingPanel`, `HandToolTapPanel`)
reject non-key input before handling `ui_focus_next` and `ui_accept`. Native pointer Button presses
also reach their unchanged coordinators. They need device-neutral action matching and held/release
handling, with one accepted action per press. No new water/cask/tool interaction is proposed.
Audio settings and its child views already consume `ui_cancel` and optional-overlay toggle; their
input matching must honor the new binding definitions without changing audio settings or content.
Map, journal and one-shot customization retain and restore Seeker physics processing; Seeker otherwise
polls `Input.GetVector` continuously. There is no shared remapping/capture context today.

There is no controls preference document or Controls editor. TEMP/save buttons/recovery and preparation
prompts embed literal keys. `README.md` also incorrectly calls the Healer marker command M; source
and scene bind it to **H**. Replace guidance from the binding catalog while retaining default wording
where already correct. Preserve existing default-text constants used by predecessor fixtures;
format live presentation separately so remapped controls never display stale instructions.

The missing deliverable is one bounded **29-action remapping system**: typed preferences, strict
storage, Godot adapter, capture/edit/reset UI, context gates and binding-aware guidance. It does not
claim full controller-only gameplay or all-dialogue accessibility certification.

## Exact proposed runtime grant

Approve `phase11-control-remapping-runtime-integration` for these existing 21 project actions and
eight UI actions, one Controls panel, isolated preferences, input-only consumer/context changes,
live prompts, and the specified tests/evidence. No additional gameplay action is created. Proposed
new and modified files are enumerated in JSON; no project.godot edit is needed because the adapter
installs definitions in memory after loading the complete validated preference snapshot.

Application/Input owns an engine-free catalog, immutable binding/document values, conflict edits,
capture state and store interface/results. Infrastructure/Input owns a strict JSON codec and atomic
file store, following the existing isolated audio-preferences pattern without modifying that store.
Godot composes storage, InputMap adapter, matcher, context and panel. Existing owner methods continue
to receive the same named actions. New files are limited to the enumerated models/adapters/UI/tests,
one focused validator and exact generated script UID sidecars. Existing source edits are limited
to input matching/context checks and live guidance, plus Main scene composition, README, quality
registration and continuation documents.

### Devices, defaults and edits

Each row displays an action label and current keyboard, mouse and controller bindings. Allow up to
eight bindings per action, at most four per device class; Add, Replace and Remove are explicit.
Keep every row reachable after edits; require at least one binding per action. Bindings are finite
primitive values, never serialized Godot objects, resources, paths or executable type names.

- Keyboard capture stores one non-modifier physical key plus exact Shift/Ctrl/Alt/Meta flags. Existing
  logical engine UI defaults retain their code mode. Reject modifier-only/zero/unknown codes. Display
  physical location labels truthfully; text entry retains native characters/IME and engine editing shortcuts.
- Mouse capture supports ordinary button indices Left, Right, Middle, X1 and X2. Wheel/pointer motion,
  double-click sequences and gestures are outside this grant. Clicking the capture button cannot bind
  itself: arm only after its initiating event is released. Native pointer hit testing and Button presses
  stay available; a handled GUI click must not also fire a gameplay command.
- Controller capture supports standardized buttons 0–20 and axes 0–5 with sign. Axis directions are
  allowed only for movement/UI direction rows; other rows accept buttons. Capture waits for neutral
  (absolute value at most 0.2), then accepts a new crossing of 0.75. Ignore drift, duplicate held events
  and unrelated devices. Normalize installed events to device `-1`; do not persist connection IDs,
  GUIDs or platform mappings. Defaults and display names identify Godot indices, not vendor glyphs.
- Keep all keyboard defaults. Add left-stick X/Y and D-pad to the four movement actions and Start (6)
  to optional overlays. Add controller A (0)/B (1) for UI accept/cancel and right/left shoulders (10/9)
  for focus next/previous. The other **16 project commands start unassigned on controller**; keyboard
  bindings remain and those commands can be assigned. Mouse gameplay defaults remain unassigned.
- Default deadzone stays 0.5. Expose finite 0.1–0.9 deadzones for movement/UI direction actions only;
  capture thresholds are independent of the installed deadzone. Preserve normalized diagonal speed
  and existing analog strength. Unsupported events explain rejection without changing the draft.

Conflicts are rejected before application, naming the conflicting action. Gameplay rows share one
conflict context; UI rows share another. Within a context reject duplicate event identities, equivalent
physical/logical key locations, opposite representations of the same key/modifiers, and modifier
supersets that can leak into continuously polled movement. UI Tab/Shift+Tab remain a valid exact
modifier pair. Across gameplay/UI contexts reuse is allowed only because the explicit gates below
prevent world input while navigating. Opposite signed axes are distinct; the same axis/sign conflicts.
No silent swaps or removal of somebody else's binding. A user removes/replaces the conflicting
binding explicitly. Do not add controller chords, macros, a command wheel or new interactions.

### Capture, focus and escape

Add a **Controls settings** entry alongside Audio in the existing settings overlay. It opens the
single scrollable Controls panel, hides the optional settings/dialogue overlays for that view and
restores the originating entry/focus when closed. The panel has explicit focus neighbors, readable
labels/current assignments/status, an ordered row list, Add/Replace/Remove, Apply, Save, Restore defaults
and Close. The initially focused control and selected row remain visible while scrolling. Use the
existing UI theme; verify readable, unclipped layout at 1280×720 and 1920×1080.

Edits are a draft. Apply validates the entire document before replacing the active InputMap;
Save validates/applies and atomically persists it. Closing unapplied edits offers Discard/Keep editing;
closing applied unsaved settings leaves them active and clearly reports they will reset on restart.
Restore defaults replaces the complete draft, including deadzones; Apply/Save remains explicit.
Per-row default restore is also available. There is no partial mutation on invalid edits/save loads.

A capture is one cancellable state machine with a ten-second timeout. It consumes its input,
ignores key echo and the initiating held press, and ends on candidate, Cancel, timeout, focus loss,
device disconnection, view close or scene exit. Require release/neutral after completion before
restoring world input; captured presses must never dispatch save/load, preparation or another action.
Physical Escape always cancels capture/closes the Controls panel and is reserved from capture there;
the visible Close and Restore defaults controls remain usable. This emergency fallback is local to
the new panel, so changing a UI cancel binding cannot strand the player. Keep settings entry reachable
by pointer and through the retained overlay-toggle binding; prohibit clearing the last binding.

The shared input context suppresses continuous Seeker movement and world commands while the Controls
panel/capture is active, while a GUI control has focus, or while map/journal/customization/audio reading
is modal. Add checks before Seeker polling and before Main/preparation/treatment/Sage/save-load
keyboard or controller dispatch; preserve direct explicit UI buttons and all transition qualifications.
Map/journal/optional toggles can close their own views when Controls is absent. The Controls panel
owns navigation while visible: Tab/Shift+Tab do not fall through to optional-overlay close. Restore
only the processing/focus state actually retained at entry; never globally pause world time or alter
AI/task progress. Focus loss/disconnection and closing a modal must release held movement/action
state so no drift or queued press reaches the world.

Use exact pressed-event matching for discrete action/modifier bindings; release matching must still
clear held state even if a modifier was released first. Contextual panels must handle actual key,
mouse and joypad press/release events and synthetic named-action fixtures, retaining one action per
press and their existing cooldown/eligibility guards. Preserve native Button activation: no duplicate
manual/controller and native GUI activation. The adapter replaces only the 29 catalog action event
sets/deadzones, retains a complete previous map for rollback, and never calls a whole-map reset that
could delete unrelated/text-edit actions. Build every event before mutation, release old held states,
apply as one synchronous operation before input resumes, rollback on failure.

### Persistence and error handling

Store version 1 at `user://controls-settings-v1.json`, separate from world/save, audio, credentials
and provider/model documents. The complete stable-order document contains schemaVersion, the exact
29 action IDs, bounded typed events and allowed deadzones. Strict UTF-8; maximum 64 KiB. Reject unknown
versions/fields/actions/event kinds, duplicates, missing actions, invalid codes/modifiers/numbers,
nonfinite deadzones, excessive lists, empty actions and conflicts. Validate everything before use.
Load missing documents as defaults; malformed documents as complete defaults with readable status;
leave malformed bytes untouched until explicit Save. No partial salvage or automatic overwrite.

Atomic Save uses a unique same-directory temporary file, flush and replacement. On failure retain
active settings and the previous disk document, clean only the owned temporary file, report failure
without paths. Tests use a disposable store. No test reads or overwrites real user settings.

Example document shape: `{"schemaVersion":1,"actions":[{"id":"move_left",
"deadzone":0.5,"bindings":[{"kind":"key","codeMode":"physical","code":65,
"shift":false,"ctrl":false,"alt":false,"meta":false}]}]}`. A real document requires all 29 rows.
Mouse bindings use `kind/button`; joypad buttons use `kind/button`; joypad axes use
`kind/axis/sign`, with distinct `kind` values `mouseButton`, `joyButton`, `joyAxis`. Only key bindings
carry modifier fields; key code modes are exactly `physical` or `logical`. Action IDs are ordinal
and case-sensitive; schema numbers/codes are integers, booleans are booleans. Bindings are unique
and emitted in catalog/device/code/modifier order. A row with multiple device bindings displays
all assignments rather than guessing the last-used device. Capture chooses a device class explicitly;
for controller axes the first selected connected device is retained until completion/cancellation.

## Finite completion and evidence

The runtime slice completes when all listed behavior is implemented and verified, authority and
source hashes are retained, documentation/ledger closed, and reviewed paths committed/pushed.
The controls roadmap checkbox remains open until ordinary named physical-device evidence qualifies
the actual support claim. Synthetic events and a remapping panel do not prove controller-only opening
scenario completion, hardware compatibility, screen-reader usability or executable installation.

1. Application tests: catalog completeness/stable ordering, all event kinds/code bounds, defaults,
   alias/modifier/conflict cases, eight/four limits, last-binding protection, immutable edit/reset,
   capture initiator/echo/axis drift/neutral/timeout/cancel/disconnect/release behavior.
2. Infrastructure tests: stable round trip, complete strict rejection cases, 64 KiB boundary, missing/
   malformed fallback with untouched disk bytes, restart persistence, atomic failure/cleanup, isolated
   absolute composition path and no world/provider/audio file access.
3. Godot focused fixture: install/reload/reset all 29 actions, exact modifiers and analog directions,
   rollback/no unrelated action deletion, keyboard/mouse/controller events through ordinary routing,
   old binding stops/new binding starts, no duplicate held activation, no world movement/commands
   during capture/GUI text/modal focus, release/focus-loss/disconnect safety and restored processing.
   Exercise each of the 21 existing owners through its unchanged eligibility conditions; use existing
   admission fixtures for qualification, not a new closed-owner/source/audio audit.
4. Native visible panel at both sizes: entry/return focus, row scrolling, all device event captures,
   timeout/cancel, errors/conflicts, row/global defaults, Apply/Save/restart, unsaved close and emergency
   recovery; contextual water/cask/tool controls activate once through focus with keyboard/controller
   and native pointer presses. Verify binding-aware prompts including H vs map M. Retain screenshots,
   logs and event provenance; clearly label synthetic controller events.
5. Mounted resource exports for all three existing presets include new scripts/panel; all frozen
   art/audio resource bytes remain exact, with only the proposed source/scene changes permitted. Register focused stages in quality.py without changing predecessor pins,
   frozen validators, hashes or evidence. When input-consumer changes affect the frozen current-tree
   herb hash checks, execute those unchanged checks in a private cold tree at their exact accepted
   commit `db298c8fa5dfe220467c4cc113d0998e1c1aec01`; do not rewrite their accepted inputs. Run focused validator and its self-test, continuation checker
   and self-tests, runner self-tests if changed, then root `./quality.sh`; require persisted final
   completion and every expected stage, not just a launch or `steps: []` job.
6. Physical-device evidence, if available: record platform, engine, device/model and actual events;
   observe key/layout behavior, mouse buttons, named standardized controller press/release/axes and
   unplug/reconnect. If hardware is unavailable retain that limitation and leave the roadmap item open.
   Do not manufacture a new controller-support claim from InputEvent injection.

## Planning verification and boundaries

This planning slice modifies only this Markdown/JSON and CONTINUATION/HANDOVER/ROADMAP. Input owners,
InputMap, runtime preferences, UI and product/CI files remain unchanged. Source hashes are bound in
JSON; the recorded project editor SHA remains
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`.
The intervening published commit adds only the separate offline speech-audition package. Reuse the
retained [177-stage quality completion](../audio-direction/herb-collection-runtime-review/quality/completion.json)
and 5,490 unit tests because product/CI inputs are unchanged; these verify the predecessor product,
not the unimplemented remapping proposal. Run the continuation checker and its eleven self-tests
for this prose/ledger change. Every prior persistent closure remains byte-identical.

Godot 4.6 supports per-action event/deadzone replacement and exact matching; UI actions govern focus.
The adapter/gates above account for those engine boundaries. Sources consulted:
[InputMap](https://docs.godotengine.org/en/4.6/classes/class_inputmap.html),
[InputEventKey](https://docs.godotengine.org/en/4.6/classes/class_inputeventkey.html),
[UI focus](https://docs.godotengine.org/en/4.6/tutorials/ui/gui_navigation.html),
[Input routing](https://docs.godotengine.org/en/4.6/tutorials/inputs/inputevent.html).
These explain APIs; limits/default additions/persistence/closure rules above are this proposed design.
All closed art, audio and gameplay owners stay closed. New camera, touch, macros/chords, virtual
keyboard, rumble/calibration, provider/save schemas, engine text-edit mappings, broad accessibility
certification and executable release remain outside the proposed grant.
