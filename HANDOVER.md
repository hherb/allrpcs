# Allrpcs Handover

## Current state

PR #276 is merged into `main` at `4dd0727`. Branch
`codex/phase-7-misquotation-evidence-presentation-acknowledgement` advances Phase 7 through
explicit exact witness-presentation acknowledgement:

- `CharacterDialogueMisquotationEvidencePresentationController.TryAcknowledge` accepts only its
  exact current witness view, selection, and non-null presentation snapshot, then hides only that
  presentation. It retains the exact view, selection, exposure, evidence, and dialogue events and
  does not permit the acknowledged selection to be projected again;
- `TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController` delegates that exact
  acknowledgement. Its isolated smoke rejects stale or distinct equivalent views, selections, and
  presentations, abstention, repetition, and nulls while preserving source state; and
- the existing switch-gated main-scene smoke now explicitly selects, presents, and acknowledges
  the exact exposed candidate. Ordinary startup remains inert, and acknowledgement leaves the
  exact main-scene view, selection, exposure, candidate, and source/delivered events unchanged.

No UI or text formatting was added. No evidence is automatically chosen, presented, acknowledged,
discovered, corrected, consumed, or persisted. No dialogue, truth, transcript, character, event,
audience, world object, perception, or other authoritative state is mutated. No credential or
provider I/O, input, retry, timing, scheduling, navigation, or movement behavior was added. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jj began with the expected missing-`TryAcknowledge` game build failure. The focused game
  build then succeeded with zero warnings, and the isolated owner and flow smokes passed on the
  host after the sandbox-only Godot `user://logs` crash.
- Phase 7jk began with the expected closed-surface main-scene smoke failure. The rebuilt
  switch-gated smoke then passed with exact one-shot acknowledgement and unchanged retained state.
- Session-wide review found that the initial smoke edit had displaced the prior stale-hiding proof.
  The test was fixed to acknowledge one presentation, accept a later fresh exact-view presentation,
  and hide that later presentation when its view becomes stale. No unresolved issue remains.
- The host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. The sandbox-only VSTest `SocketException (13)` was retried
  unchanged on the host. `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity
  minimal` and `git diff --check` also pass.

## Exact next bounded slice

Take **Phase 7jl: project one exact witness presentation into isolated truth-neutral temporary
UI**.

Acceptance boundary:

1. Add one isolated temporary presentation seam that accepts only the presentation owner's exact
   current non-null snapshot and renders its source and delivered utterance texts separately under
   neutral labels without calling either statement true, false, corrected, or authoritative.
2. Prove hidden inert startup, exact snapshot/text retention, stale and distinct equivalent
   rejection, hiding after exact acknowledgement, unchanged view/selection/exposure/evidence/events,
   null rejection, and a closed surface in one credential-free isolated smoke.
3. Do not compose the UI into `Main.tscn`; add input or automatic selection/acknowledgement;
   discover, correct, consume, or persist evidence; deliver dialogue; change transcripts,
   characters, events, audiences, world state, or perception; or add provider, retry, timing,
   scheduling, navigation, or movement behavior.
