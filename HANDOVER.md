# Allrpcs Handover

## Current state

`main` contains merged PR #230 at `4faecd4`. The current branch is
`codex/phase-7-sabotage-candidate-flow` and advances four bounded Phase 7 slices:

- Phase 7dl retains the exact authored water-cask candidate in one stable read-only bounded
  collection and routes explicit temporary apply through the existing first-eligible scene seam;
- Phase 7dm defines `CharacterObjectSabotageConcealmentConsiderationRequest`, which defensively
  retains exact authority snapshots and at most 64 ordered candidates with value-unique evidence
  identifiers;
- Phase 7dn defines `CharacterObjectSabotageConcealmentDecision`, an explicit provider-neutral
  selection by exact evidence identifier or null-selection abstention;
- Phase 7do accepts only the exact considering character and an exact currently eligible candidate,
  while allowing that exact character to abstain without applying an effect.

The temporary flow still runs only on an explicit request. No slice chooses timing, decodes or
generates a decision, constructs a model, invokes a provider, applies a decision result, introduces
another sabotage kind, or adds UI/input/events/time/transcripts/persistence or movement behavior.

Draft PR #231 is open from implementation commit `9e5b6c4`. Hosted run `34786810160` completed with
all five jobs at `steps: []`; it is Actions/account infrastructure state, not product-test or
unsigned-build evidence, and should not be retried unchanged.

## Validation and review

- Every slice began with the expected compile failure for its absent contract or seam.
- Focused host-local tests pass for the complete object-sabotage contract set, including exact
  immutable request retention, defensive bounded collection materialization, explicit selection and
  abstention, exact-identifier enforcement, current eligibility, and existing concealment behavior.
- Focused host-local source, flow, presentation-flow, and main-scene Godot smokes pass exact bounded
  collection routing, inert initialization, first eligible application, repeat rejection, and
  unchanged evidence-gated recovery. The restricted run reproduced the known `user://logs` crash
  before project code; the identical host-local checks pass.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,298 Domain tests, 478 Application tests, 532 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines and use no lines longer than 120 characters.
  No live-conformance switch was enabled and no provider was invoked.
- Hosted run `34786810160` executed no job steps; host-local quality is the current product evidence.

## Exact next bounded slice

Take **Phase 7dp: bind one accepted concealment decision to its exact consideration request**.

Acceptance boundary:

1. Add one immutable `CharacterObjectSabotageConcealmentConsiderationResult` retaining the exact
   request and exact accepted decision.
2. Retain the exact selected candidate from the request when an evidence identifier is selected, or
   `null` for explicit abstention; do not reconstruct an identifier or candidate.
3. Reject null inputs, a distinct/value-equal character or evidence identifier, an unknown candidate,
   and any candidate that is no longer eligible under the request's exact snapshots.
4. Prove exact selection, abstention, rejection, unchanged inputs, and the closed immutable surface
   in focused Domain tests.
5. Do not apply concealment, append evidence, choose timing, create a scheduler or scene controller,
   decode or serialize model data, invoke a provider, add another sabotage kind, or add UI/events/
   transcripts/persistence/movement behavior.
