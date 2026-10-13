# Physical controls: finite evaluation proposal

Prepared 2026-10-13 against `e7b17575479be3f9061979c3269168f956782566`. The owner approved the ledger's
**planning** slice with “Read HANDOVER.md and follow the instructions - scope is approved”.
This completes that proposal. Physical input/session execution is a distinct grant.
The [JSON contract](physical-controls-owner-scope.json) binds source bytes, session intake,
profiles, cases and required evidence. No physical test has been performed by this slice.

## Named session and exact provenance

Proposed session **PC-MAC-01** evaluates ordinary physical keyboard **KB01**, mouse **M01**,
and one **Microsoft Xbox Wireless Controller, model 1914, connected by USB** on the current
Apple Silicon host. The controller is a proposed candidate; its presence, detection and
compatibility have not been established. KB01/M01 are session handles, not invented model names.
No device purchase is required by this proposal. The inventory question was sent to the owner;
these proposed identities remain unconfirmed until the owner supplies the actual setup.

Read-only host observation returned macOS **27.0.1**, build **26A434**, architecture **arm64**.
This identifies a candidate host OS, not a completed hardware test. Before execution record the
computer model/chip, actual OS version/build, keyboard manufacturer/model/layout/connection,
mouse manufacturer/model/buttons/connection, controller manufacturer/model/firmware if reported,
USB cable/adapter path, and the actual .NET/Godot versions. Record “not reported” for unavailable
firmware; missing device model or OS/build blocks qualification. Record controller identity as
reported by Godot, its mapping/recognition status and connection identifier locally. Do not put
connection IDs into preferences. Exclude serial numbers, account names and machine identifiers.

PC01 uses a separate disposable, read-only Godot device-intake probe, with its source retained in
review evidence. It enumerates connected controllers, reported name/GUID and recognition status
(`Input.GetConnectedJoypads`, `GetJoyName`, `GetJoyGuid`, `IsJoyKnown` in the installed GodotSharp
API), then logs only physical joy-button/axis/connection events for the operator's labeled presses.
The probe never injects events, installs mappings, runs Main or writes product preferences/state.
Its report establishes intake only; close it before ordinary Main gameplay evidence. Proposed
`device-intake.gd`/`device-intake.json` belong to the later evaluation package, not this planning
slice or the product source. Physical keyboard/mouse operation is evidenced in the native cases;
no typed content is logged. These API names were checked in the installed GodotSharp XML reference.

Substitution of the named controller or host requires the owner to bind the changed session before
execution; update a successor authority file, leaving this closed proposal intact. KB01/M01 exact
models are mandatory intake values in that authority. No OS driver installation, firmware update,
Bluetooth pairing, accessibility setting change or device emulation is included.

## Source-grounded boundary

The accepted catalog has 21 project actions and eight UI actions. Left stick/D-pad movement,
Start (Godot button 6) for settings, A (0)/B (1) for UI accept/cancel, and RB (10)/LB (9) for
focus next/previous exist. Sixteen other world commands have no controller defaults. The editor
supports explicit per-action edits, validated Apply/Save and complete defaults; controller-only
coverage must therefore include actual controller-driven configuration, not seeded preferences.
Vendor labels below are operator instructions to verify against observed Godot indices; an
unrecognized/mismapped physical button fails intake rather than proving compatibility.

`OptionalOverlayVisibilityController` initially focuses Controls after Start. `ControlSettingsPanel`
opens on the action list; use shoulders for focus, D-pad for list/choice navigation and A for
activation. Capture must wait for the initiating release; directions require neutral <= 0.2 and
a new crossing >= 0.75, with a ten-second timeout. Direction deadzones default to 0.5 and permit
0.1–0.9. GUI/modal focus fences world commands and movement, without pausing world/AI processing.
These are source contracts to evaluate, not fresh synthetic test results.

The ordinary Main scene initializes the first-patient recipe and ingredients. Preparation needs
range <= 48 px at the retained garden station `(2300,1300)`, then distinct presses through start,
existing recipe steps, completion and batch creation. Treatment needs range <= 64 px of the actual
patient binding near the sickhouse anchor `(2640,1450)`. Consultation needs the resulting patient
testimony and range <= 64 px of Sage `(2240,950)`. Those exact owners determine eligibility; the
operator may not seed progress, teleport bodies, freeze NPCs or invoke controller methods directly.

## Execution setup and cases

A human operator supplies physical events. Use an ordinary native `Main.tscn` launch with all
synthetic smoke/check flags unset, no injected input, automation, virtual controller, remote-input
stream or fixture arrangement. Build/reuse the exact committed source; record commit, clean product
paths, executable hash, Godot version, launch command/environment and session times. Use a disposable
Godot user-data directory through an isolated operator test account/home, with the isolation method
recorded and verified before launch. Do not use the `ALLRPCS_CONTROLS_MAIN_CHECK` storage branch: it
also enables synthetic fixtures. Do not overwrite the owner's controls, saves or provider settings.
If isolation needs new OS-account/setup authority, stop and report the unmet prerequisite.

Use the existing offline model/default composition; no credential/provider changes or speech service
is needed for this route. Native 1280×720 and 1920×1080 windows are separate cases on the same OS.
Evidence records the actual viewport, display scale and focus state; unsupported sizing is a failure,
not a substituted headless result. Cap the session at **120 minutes**, controller route at **45 minutes**,
and one repeat per failed case after restoring ordinary starting conditions. Preserve first failures;
no indefinite retries. Unavailable hardware or blocked setup is **not performed**, never pass.

| Case | Physical operation | Required observable result |
| --- | --- | --- |
| PC01 | Intake, neutral controller, press each used button and move both stick axes | Exact device/OS/runtime provenance; recognized indices; no idle world motion for 10 s |
| PC02 | Keyboard WASD, diagonals, release; Tab into Controls, focus/accept/cancel | Four directions and normalized diagonals; stop on release; visible focus; no world action in UI |
| PC03 | In Controls capture a physical non-modifier key with Shift; cancel by Escape; wait 10 s in a second capture | Exact displayed binding; no initiating-key echo; Escape/timeout restore navigation and leave active settings unchanged |
| PC04 | Mouse native Add/Cancel/Close; capture Right for `healer_toggle_follow_wait`, Apply, close and press/release in world | Native hit targets remain usable; one Healer mode change per press; capture click never dispatches a world command |
| PC05 | Mouse/keyboard draft rejection, Apply, Save, restart, row/full defaults | Same-context duplicate rejected without partial changes; draft differs until Apply; saved bindings reload; defaults require explicit Apply/Save |
| PC06 | Controller Start -> Controls, traverse first/last action and footer at both window sizes | All rows/edits/footer reachable and readable; A/B and shoulders act once; no keyboard/mouse rescue |
| PC07 | Controller capture signed movement directions from neutral, vary deadzone 0.1/0.5/0.9 then restore; hold input across close | Correct signed direction/threshold display, drift rejection, release fence; no captured command or held movement leaks |
| PC08 | Disconnect USB during capture and during held movement; reconnect; separately move window focus away/back | Capture ends on disconnect/focus loss; gameplay stops; release/neutral then deliberate new input restores movement; no queued command |
| PC09 | Fresh controller-only opening route below | Continuous physical-controller route through discovery, prepared batch, treatment/testimony, Sage consultation and save/reload; no synthetic state or keyboard/mouse input |
| PC10 | Controller UI cancel changed from B to X in Controls; use new cancel, discard draft, restore defaults and Save | New binding actually cancels; visible Close/discard remain reachable through controller navigation; no stranded menu |
| PC11 | Separate keyboard/mouse check of map/journal/settings gates and held release | World input suppressed while reading/focused; after release and close it resumes without changing world/AI processing flags |
| PC12 | Finish at defaults in disposable profile; terminate; inspect evidence completeness | Result per case, timestamps/media, complete saved preferences and session manifest; owner's data/editor bytes preserved |

Focus-switch actions in PC08 may use OS keyboard/mouse and are outside PC09. The PC09 input restriction
starts at the first fresh Main frame and includes settings configuration, profile changes and restart.
The operator may click the launcher before that frame and close/relaunch the process between segments;
record those boundaries. Any keyboard/mouse use inside a gameplay/settings segment fails PC09.
No screen-reader, free-text controller entry, complete dialogue playability, other platform/device,
wireless connection, executable export/install or release claim follows from this session.

## Controller-only opening route PC09

Start from a fresh disposable profile/world at defaults. Start opens settings; Controls already has
focus. Enter **profile A** by Add on each world row, retaining keyboard defaults. Do not change UI,
movement or Start bindings. Profiles are operator-entered settings, not new runtime code.

| World action | Profile A: discovery | Profile B: recovery |
| --- | --- | --- |
| `focused_search_start` | X (2) | Remove X; keyboard retained |
| `focused_search_complete` | Y (3) | Remove Y; keyboard retained |
| `focused_search_resolve` | A (0) | Remove A; keyboard retained |
| `focused_search_cancel` | B (1) | Remove B; keyboard retained |
| `remedy_prepare` | Keyboard only | X (2) |
| `remedy_administer` | Keyboard only | Y (3) |
| `sage_consult` | Keyboard only | A (0) |
| `layered_inscription_discover` | Keyboard only | B (1) |
| `map_toggle` | View (4) | View (4) |
| `journal_toggle` | LS click (7) | LS click (7) |
| `healer_toggle_follow_wait` | RS click (8) | RS click (8) |
| `save_current` | LB (9) | LB (9) |
| `load_current` | RB (10) | RB (10) |

UI reuse of A/B/shoulders is allowed across the two contexts only through the existing gates; a
world command firing during controller UI navigation is a failure. Profile B removes all four
search buttons **before** assigning recovery buttons, so conflicts cannot be silently swapped.
Save each profile through the actual UI and retain its complete 29-action preference bytes/hash.

1. Save profile A and Close Controls/settings back to world. Open/close map with View and journal
   with LS click to satisfy the existing Save initialization requirements. Use both left stick and
   D-pad to follow retained paths; do not cut collisions or use coordinates as teleport instructions.
2. Visit the existing herb-patch target through village edge/woodland clearing, obtain its visible
   interaction focus, and press X, Y, A with release between each. Record the identified description
   and the ordinary search lifecycle. Start a second search then B-cancel; verify no completed result.
   Visit abandoned wagon via village square -> southern road `(1500,1400)` -> wagon `(1900,1640)`
   and perform X/Y/A for the tool-gated clue. Missing ordinary target/eligibility is a recorded failure.
3. Start -> Controls; remove profile A's four search button assignments, add profile B, Save and
   Close. Visit the old shrine via southern road -> old shrine `(380,300)` and B for the existing
   inscription owner. Do not synthesize route-completion tokens or claim an unexposed completion UI.
4. Return by retained connections to village square -> healer garden. Within 48 px of the actual
   station press X once per displayed preparation prompt until a prepared batch exists. Hold one
   press briefly and verify no extra steps occur; never infer success from audio alone.
5. Return through village square -> sickhouse and enter via its authored north entrance. Within
   64 px of the actual patient, Y administers once; retain the treatment/stabilization/testimony
   presentation. Repeated Y must not create another treatment. Walk to Sage `(2240,950)` and A
   within 64 px; require “Sage consulted” from the ordinary qualified owner.
6. LB saves in world with map/journal initialized. Close/relaunch the native process with the same
   disposable profile; RB loads. Record loaded state and retained recovery/Sage presentation. Show
   the persisted controller assignments through Controls and resume one deliberate movement after
   closing/releasing. Failure to restore the documented state fails the route.

PC09 passes only if every step completes within 45 minutes without keyboard/mouse assistance,
provider-dependent free text, injected state or product changes. This finite recovery/discovery
route does not establish controller-only playability of every opening interaction or later game.
Unreachable UI or missing action is a useful failure; fixes require a distinct defect scope and
must preserve the accepted owners, rather than silently amending this evaluation.

## Evidence and completion

Execution creates `input-direction/physical-controls-review/` with authority, session, results,
route timeline, complete profiles, captures, launch log, preservation record and manifest as
specified in JSON. Every case has pass/fail/not_performed plus expected/observed behavior, input
source, time, device and media references. Controller route media must show both hands/device and
screen (synchronized recordings are acceptable); a screen-only capture cannot prove physical input
or absence of assistance. Record any assistance and interruption explicitly. No typed dialogue,
credentials or unrelated desktop content is needed. Keep raw evidence bytes immutable and bind every
retained file's size/SHA-256; compressed logs additionally bind decompressed SHA-256.

A complete **evaluation** can contain failures; close its session when all cases are accounted for
and a review states the exact limits. A **qualification pass** additionally requires every case
passed, all PC09 steps passed and complete provenance/media. Never check off the broad controls
roadmap item solely for one macOS/device combination without an owner-approved support matrix.
Reuse the closed product verification for unchanged sources; product fixes or CI changes require
new applicable checks and root quality under a separate authorized slice.

The one new decision is authority for **phase11-physical-controls-evaluation-execution / PC-MAC-01**:
confirm KB01/M01 actual models, the proposed controller/connection and exact host, and authorize a
human-operated native session, disposable preference/save writes, USB disconnect/reconnect,
focus changes, captures and bounded evidence publication. This grants evaluation only. Owner or
an authorized local operator must supply actual hardware events; an agent cannot substitute
synthetic/UI automation for that operator. No execution or device setup is granted by this plan.
