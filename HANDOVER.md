# Allrpcs Handover

## Current state

PR #275 is merged into `main` at `164083d`. Branch
`codex/phase-7-misquotation-evidence-presentation-flow` advances Phase 7 through explicit bounded
witness-presentation orchestration and inert main-scene ownership:

- `TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController` accepts an explicit
  exact current-view member or `null` abstention, delegates the existing selection owner, then
  delegates that exact resulting selection to the existing presentation owner;
- its isolated credential-free smoke proves inert startup, exact selection and abstention, exact
  nested witness/evidence/event identity, and stale, distinct equivalent, non-member, repeated,
  and null-view rejection with a closed public surface; and
- `Main.tscn` owns the flow after its view, selection, and presentation dependencies. The existing
  switch-gated main-scene smoke explicitly abstains on the empty initial view and later selects the
  exact exposed candidate; ordinary startup never invokes the flow.

No UI or text formatting was added. No evidence is automatically chosen, presented, discovered,
corrected, consumed, or persisted. No dialogue, truth, transcript, character, event, audience,
world object, perception, or other authoritative state is mutated. No credential or provider I/O,
input, retry, timing, scheduling, navigation, or movement behavior was added. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jh began with the expected missing-flow-controller game build failure. The focused game
  build then succeeded with zero warnings, and the isolated smoke passed on the host after the
  sandbox-only Godot `user://logs` crash.
- Phase 7ji began with the expected missing-main-node Godot failure. The rebuilt switch-gated
  main-scene smoke then passed with the node in dependency order, proving inert startup, explicit
  abstention and exact later selection, one-shot behavior, exact nested identity, unchanged source
  dialogue evidence, and closed flow surfaces.
- Session-wide review verified that the flow cannot accept a choice unless the exact supplied view
  is still scene-current, that a newly accepted selection is synchronously projectable without a
  partial-state path, and that stale selections disappear before a later exact-view choice. No
  unresolved issue remains.
- The host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. `dotnet format Allrpcs.sln --no-restore --verify-no-changes
  --verbosity minimal` and `git diff --check` also pass.
- Hosted checks must be treated separately from this complete local evidence and re-queried at the
  final published head.

## Exact next bounded slice

Take **Phase 7jj: explicitly acknowledge one exact witness presentation without consuming its
evidence**.

Acceptance boundary:

1. Add one exact-reference acknowledgement operation to the presentation owner that accepts only
   its exact current witness view, selection, and non-null presentation snapshot, then hides that
   presentation without changing the view, selection, exposure, evidence, or dialogue events.
2. Delegate that operation through the isolated temporary presentation flow and prove exact
   acknowledgement, stale or distinct equivalent view/selection/presentation rejection, abstention
   rejection, repeated and null rejection, unchanged source state, and closed surfaces in its
   credential-free smoke.
3. Do not compose acknowledgement into `Main.tscn`; automatically choose, present, acknowledge,
   discover, correct, consume, or persist evidence; render or format UI; label either statement
   true or false; deliver dialogue; change transcripts, characters, events, audiences, world state,
   or perception; or add provider, input, retry, timing, scheduling, navigation, or movement
   behavior.
