# Allrpcs Handover

## Current state

PR #270 is merged into `main` at `79cae26`. Branch
`codex/phase-7-misquotation-evidence-exposure` advances Phase 7 through the exact exposure-state
and scene-ownership boundary:

- `CharacterDialogueMisquotationEvidenceExposureSnapshot` copies at most 64 exact evidence entries
  in stable order, rejecting nulls and duplicate exact delivered events while allowing distinct
  value-equal events;
- `CharacterDialogueMisquotationEvidenceExposureTransition` returns the exact current snapshot for
  abstention or appends one accepted exact selection into a fresh snapshot, rejecting exact repeat
  delivery and capacity exhaustion without partial state; and
- `CharacterDialogueMisquotationEvidenceExposureController` initializes one exact snapshot once
  and resolves an exact request at most once only from expected current state. Selection installs
  the fresh snapshot; abstention retains the exact current instance.

Implementation commit `2bb5e65` is published in draft PR #271 targeting `main`.

No task result is observed, presented, acknowledged, discovered, corrected, or persisted. The new
controller is not in `Main.tscn`; no dialogue, truth, transcript, character, event, audience, world
object, perception, or other authoritative state is mutated. No automatic task, credential or
provider I/O, input, retry, timing, scheduling, navigation, or movement behavior was added. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7iv began with the expected missing snapshot and transition failures; 10 focused Domain
  tests now prove copying, bounds, exact delivered-event uniqueness, distinct value-equal allowance,
  selection append, abstention identity, failure atomicity, null rejection, and closed surfaces.
- The exposure-controller slice has a credential-free isolated Godot smoke proving one-time
  initialization, exact compare-and-swap selection, one-shot abstention, stale, duplicate, repeated,
  uninitialized and null rejection, nested reference retention, closed surface, and atomic failure.
- Sandboxed VSTest and Godot were blocked only by the known loopback and `user://logs` restrictions.
  The identical host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,546 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review checked exact nested event retention, distinct value-equal acceptance,
  duplicate and capacity rejection, selection and abstention identity, one-time request resolution,
  stale-safe atomic failure, closed surfaces, file sizes, C# line lengths, formatting, and
  whitespace. No unresolved issue remains.
- Hosted run `35030156922` completed with every job reporting `steps: []`; this is Actions/account
  infrastructure state rather than product-test evidence. Do not retry it unchanged.

## Exact next bounded slice

Take **Phase 7ix: observe and explicitly acknowledge one exposed misquotation-evidence result**.

Acceptance boundary:

1. Add one isolated scene coordinator that observes each fresh exact completed
   `CharacterDialogueMisquotationEvidenceTaskController` result at most once and delegates it to
   `CharacterDialogueMisquotationEvidenceExposureController` only when the result request can
   resolve from the controller's exact current snapshot.
2. Selected evidence must install the transition's fresh snapshot; abstention must retain the exact
   current snapshot. Stale, duplicate, full, idle, running, cancelled, failed, null, and repeatedly
   observed results must be inert.
3. Add explicit acknowledgement that resets only the exact completed result already observed while
   the exposure controller still retains the expected post-resolution snapshot; repeated or stale
   acknowledgement must fail closed.
4. Prove the flow in an isolated credential-free smoke. Do not add it to `Main.tscn`; present,
   discover, correct, or persist evidence; change the existing source/model/task/start flow; deliver
   dialogue; mutate truth, transcripts, characters, events, audiences, world state, or perception;
   or add provider, input, retry, timing, scheduling, navigation, or movement behavior.
