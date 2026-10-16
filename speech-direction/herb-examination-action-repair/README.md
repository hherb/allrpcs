# Herb-patch examination action repair

The owner approved the [bounded proposal](../player-microphone-qualification/herb-examination-action-proposal.md)
on 2026-10-16. [Authority](authority.json) binds that exact proposal. Ordinary questions were working,
but requests to examine the herb patch produced no visible execution or acknowledgement. The
implementation addresses the remaining Phase 10 typed/spoken intention-flow acceptance for this
one existing action; it does not complete the whole acceptance item.

## Resulting behavior

Main hands a newly installed, accepted Healer examination response to an independent action owner.
Admission retains the exact actor, response and intention. A silent response can start an examination;
a spoken response first needs its exact recorded statement. The dialogue turn can complete while
the Healer travels, so another question can receive an answer without losing the examination.

The Healer uses the existing navigation and collision movement. The approach passes through the
authored woodland-clearing opening with body clearance: the direct navigation corridor can otherwise
clip the `ClearingBottomLeft` tree boundary. No navigation surface, collider or artwork is changed.
Only arrival at the target permits the examination occurrence. The existing engine result source
provides collection safety. A separate readable action line shows preparation, travel, result or
failure; it does not invent character speech or feed speech synthesis.

Examination leaves the herbs in place. Main disables the old automatic examination/collection
scaffold for ordinary generated responses. Its original controller bytes and frozen presentation
receipts remain unchanged. The two legacy examination/conditional-collection fixtures explicitly
select that scaffold, preserving their original ordered-event contracts.

Only one examination can run. A newer accepted examination replaces it; ordinary wait/refusal
dialogue does not replace it. A manual movement command supersedes its command revision, and
cancellation stops only movement still owned by that action. Invalid/unreachable targets, a
three-second stationary stall or the 45-second travel bound terminate with feedback. An away Healer
reports unavailability. Scene exit releases the owner. Successful load cancels travel, clears its
result token and fences a reply generated for the request that was pending before load.

Repeated examinations use distinct advancing events, `herb-patch-examination-1`, `-2`, and so on.
The current log chooses the next identifier. A pure catalog expansion reconstructs only contiguous
examination identifiers for save preflight; rejected loads leave definitions and live state unchanged.
Existing world-event and transcript transitions still install each occurrence atomically. No save
schema change or general action planner is introduced.

## Verification

- Eight Application tests cover silent/exact spoken admission, reconstructed/unrecorded speech,
  foreign actor/target, wait/refusal/terminal responses, unique repeated events and strict catalog
  restoration. The game builds without warnings.
- [Native Main check](native-main.json): 127 assertions passed using fictional HTTP and synthetic
  ASR. Spoken and silent examination both reach the actual patch before committing. It covers
  ordinary replies during travel, repeated and duplicate offers, replacement, cancellation,
  manual command ownership, invalid/unreachable targets, stationary obstruction, cold event-catalog
  restore, complete JSON load during travel, and a delayed generation arriving after load.
- The same fictional request enters through typed input and the synthetic recognition draft seam.
  Recognition alone executes nothing; explicit Submit remains the dialogue owner. No physical
  microphone is opened. Every examination-only request leaves collection absent.
- [Selected local-model check](live-main.json): actual `gemma4:e4b` chose an accepted examination,
  reached the patch and committed its result through ordinary Main. One fictional request is an
  integration observation, not model reliability or physical speech qualification.
- [Native screenshot](native-main.png) was inspected: the result line is readable, inside the
  1280×720 viewport and separate from the reply/generation status. This is visual evidence, not
  screen-reader or audibility evidence. Both new fixture exits were free of ObjectDB leak warnings.
- The new controlled Main check is required by `quality.py`. The recovered full root gate passed 216 stages and 5,644 unit tests.
  [Final receipt](quality/final/summary.json), [verification](verification.json) and
  [source inputs](source-inputs.json) bind the actual implementation and results.
  The first run passed 21 historical/art boundaries, then stopped at C# whitespace formatting.
  `dotnet format whitespace` corrected three files without changing their tokens, string literals
  or comments. The [recovery runner](finish-quality.py) checks all 18,409 recorded inputs, exact
  before/after bytes, failure/log hashes and command signatures before reusing those 21 boundaries.
  Its drift-rejection self-test passes. Continuation and every current product/runtime stage rerun
  through the normal full pipeline; frozen validators and acceptance receipts are unchanged.
  The next gameplay run caught the new controller being appended after the channel artwork's
  required final root owner. Main now inserts the controller before that owner. Review also corrected
  the inactive legacy live fixture so it cannot reset another fixture's scaffold opt-in. The unchanged
  channel validator and generated examination/collection check both pass. A second
  [guarded recovery](finish-gameplay-quality.py) binds precisely those two changes and reruns all
  current product/runtime stages; its drift-rejection self-test passes as well.

The human microphone trial remains three passes, five partial cases and seven unperformed cases.
The prior two-turn failure and subsequent action-specific clarification remain recorded. No new
human, echo, device, playback or broad platform pass follows from these synthetic/local-model checks.
The model still owns participation and action choice; the repair does not force obedience or speech.

## Reproduction

Build with `dotnet build game/Allrpcs.Game.csproj`. Run the standalone scene with isolated settings
and missing speech configurations so it cannot open a physical microphone:

```sh
ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 \
ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 \
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
ALLRPCS_SPEECH_CONFIG=/private/tmp/allrpcs-speech-missing.json \
ALLRPCS_PLAYER_SPEECH_CONFIG=/private/tmp/allrpcs-asr-missing.json \
ALLRPCS_EXAMINATION_MODE=controlled \
ALLRPCS_EXAMINATION_REPORT=/private/tmp/examination-report.json \
godot --headless --path game --quit-after 10800 res://HerbExaminationActionMainCheck.tscn
```

Use `ALLRPCS_EXAMINATION_MODE=live` for the one fictional selected-local-model request. For native
visual inspection, omit `--headless`, add `--always-on-top` to keep the macOS render surface visible,
and optionally set `ALLRPCS_EXAMINATION_SCREENSHOT` to an
absolute PNG path. Main's authored startup independent task must return the Healer before the
fixture begins. Full product verification uses `./quality.sh` with its pinned runtime.

The final gate retains nonfatal ObjectDB cleanup warnings from legacy fixtures. The new examination
fixture has a separate clean-exit requirement. Recovery runners are checkpoint-specific and reject
later source/ledger changes; reproduce current behavior with the standalone Main command above.

The final native image was recaptured with `--always-on-top` and inspected after an occluded-window
capture retained an initial frame. The corrected image shows the completed action line. Product
source and the passed full quality gate are unchanged by this capture correction.
