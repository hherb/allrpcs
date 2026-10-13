# Approved controls remapping runtime

The owner approved the exact [29-action proposal](../remapping-owner-scope.md) on
2026-10-13. [Authority](../remapping-runtime-authority.json) retains the proposal
hashes, publication grant and sole unfrozen verification assertion adjustment.
[Verification](../remapping-runtime-verification.json) binds current source inputs,
protected baseline, Main reports, native captures and mounted resources.

## Implemented boundary

The 21 existing project actions and eight UI actions have immutable validated
preferences, exact keyboard modifiers, supported mouse buttons, standardized
controller buttons and signed directional axes. Each action retains at least one
binding; complete documents reject unknown fields/actions/events, duplicate or
conflicting assignments, invalid values, oversized bytes and invalid UTF-8.

The Controls entry opens one scrollable draft editor with Add, Replace, Remove,
row defaults, full defaults, direction deadzones, Apply, Save and explicit draft
discard. Capture waits for the initiating release, consumes events, selects a
controller connection, requires axis neutral/crossing and ends on candidate,
Cancel, ten-second timeout, focus loss, disconnection or close. Escape remains a
local emergency exit. Modal/text focus suppresses world input; held input must be
released before it can resume. World time and AI continue. Existing action owners,
admission checks and audio/art resources remain authoritative and unchanged.

Apply installs all 29 action sets transactionally and retains unrelated engine
actions. Save applies explicitly then atomically replaces the isolated
`user://controls-settings-v1.json`; write failure retains active values and previous
disk bytes. Unreadable startup documents select complete defaults without rewriting
the document. Live guidance and Save/Load buttons describe current assignments.

## Evidence and limits

- 38 Application and 27 Infrastructure focused tests pass, including complete
  defaults, conflicts, capture transitions, strict schema and failed atomic writes.
- Genuine Main headless report has nine checks. Each native window report has ten.
  Adapter rollback, keyboard-layout startup fallback without disk rewriting, all 29
  defaults, movement and signed axes, actual Healer command
  routing, live guidance, row scrolling/focus, capture, input gates and disposable
  persistence pass. The three qualified contextual owners accept keyboard/controller
  once per press, suppress held repeats and retain native pointer hit testing even
  when primary mouse input is mapped to UI accept. Capture Cancel and Close are
  exercised with actual pointer events, including Close while movement is held.
  During capture, visible Close retains draft-discard confirmation and Restore
  defaults cancels capture without applying the new draft implicitly. Native
  primary-pointer controls remain usable when Left is remapped to UI cancel,
  and the actual Controls entry remains reachable with Left assigned to the
  optional-overlay toggle.
  Contextual positive cases temporarily freeze the existing NPC fixture owners to
  retain qualification, then restore their processing flags. Separate modal checks
  assert that production Main/Healer processing continues and the tree stays unpaused.
- All six actual native PNGs were visually inspected at 1280×720 and 1920×1080:
  mixed bindings, final action row and active capture. Text, status, edit controls
  and footer fit; the last row and capture cancel are reachable.
- Linux, macOS and Windows Development resource packs mount successfully. Seven
  C# scripts and three scene resources are present; Godot converts the scenes to
  binary and consolidates UID sidecars. Exact converted scene bytes agree across
  all presets. All 130 preceding audio/notice members remain byte-identical.
  The retained mounted subset contains those 140 actual packed members; the
  manifest separately binds source scenes and seven generated source UID sidecars.
- Full root quality passes 188 stages and 5,576 unit tests. Frozen
  art/audio validators and accepted evidence stay unchanged; the herb checks now
  run cold at accepted `db298c8f` with all preceding runner stages preserved.

Input events are synthetic. No physical controller, controller-only opening
scenario, broad screen-reader certification, executable installation or release is
claimed. The Phase 11 device-support checkbox remains open. Export warnings and
existing C# executable-export limits are retained honestly in the export logs.
The unrelated editor rewrite and separately owned speech work are excluded from
this slice. Concurrently published Pocket speech tests are included in the actual
full-suite total; the retained quality record names its committed product baseline.

## Reproduction

```sh
python3 scripts/check-continuation.py --self-test
python3 scripts/validate-control-remapping-runtime.py --self-test
python3 scripts/validate-control-remapping-runtime.py --reviewed
python3 quality.py --self-test
./quality.sh
python3 scripts/validate-control-remapping-runtime.py --complete
```

Native runs are serialized on the host, with `ALLRPCS_CONTROLS_MAIN_CHECK=1`,
`ALLRPCS_CONTROLS_MODE=native-1280` or `native-1920`, disposable report/screenshot
paths, `--fixed-fps 60 --path game --quit-after 1800`, and the existing model-disable
fixture flags. Their provenance remains synthetic even though the UI is rendered
by the actual native window. Packaging inspection uses the retained GDScript with
an absolute `CONTROLS_PACK` and `CONTROLS_PACK_REPORT` in a private empty project.
