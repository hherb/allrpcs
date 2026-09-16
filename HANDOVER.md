# Allrpcs Handover

## Current state

PR #280 is merged into `main` at `0dd600d`. Branch
`codex/phase-7-misquotation-live-candidate-flow` advances Phase 7 through three bounded slices:

- Phase 7ju owns `CharacterDialogueMisquotationEvidenceCandidateController` immediately after the
  occurrence coordinator in `Main.tscn`. Only the dedicated occurrence smoke requests a candidate,
  after its switch-gated exact occurrence.
- Phase 7jv adds `CharacterDialogueMisquotationEvidenceCandidateRequestController`, which retains
  one immutable consideration request only for the candidate controller's exact current candidate
  while the existing observability policy still accepts the supplied actor. The request retains that
  exact actor and candidate.
- Phase 7jw owns the request controller immediately after its candidate dependency in `Main.tscn`.
  The dedicated smoke explicitly creates the request only after the live occurrence and candidate.

The preauthored `TemporaryCharacterDialogueMisquotationEvidenceSource` and its existing
consideration flow remain unchanged. No live candidate request starts a task or consumes a model.
No misquotation is automatically proposed, scheduled, delivered, appended to a transcript,
evaluated, exposed, reviewed, selected, presented, acknowledged, corrected, consumed, or persisted.
No provider, retry, timing, navigation, movement, world, perception, character, or general
dialogue-controller behavior changed. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

## Validation and review

- Phase 7ju began with the expected missing-main-node smoke failure. Its focused main-scene smoke
  proves ordinary startup plus unavailable, stale, unheard, and repeated paths remain inert; one
  exact switch-gated occurrence can create one exact witness candidate.
- Phase 7jv began with the expected missing-controller build failure. Its focused standalone smoke
  proves unavailable, stale, null, and repeated request creation remains inert while one accepted
  request retains the exact supplied actor and current candidate without starting work.
- Phase 7jw began with the expected missing-main-node smoke failure. The extended main-scene smoke
  proves the exact request is created only after the live occurrence/candidate chain while the
  transcript, task, exposure, view, presentation, world, perception, and preauthored source remain
  reference-identical.
- Session review inspected every changed path and found no unresolved issue. `git diff --check`, the
  repository formatter, and a 120-column check pass.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch. The sandbox-only VSTest `SocketException (13)` was retried unchanged on the host.

## Exact next bounded slice

Take **Phase 7jx: start one explicit decision from the exact current live-candidate request without
main-scene composition**.

Acceptance boundary:

1. Add a focused scene flow that accepts an expected request only when it is the request controller's
   exact current `Request` and the existing misquotation-evidence task is idle. Validate currentness
   before taking one model from `TemporaryCharacterDialogueMisquotationEvidenceModelSource`, then
   delegate that exact request to `CharacterDialogueMisquotationEvidenceTaskController`.
2. Prove unavailable, stale, null, overlapping, cancelled, and repeated starts fail closed without
   consuming a queued one-shot model or replacing the exact request; one explicit accepted start may
   complete with a result retaining the exact request and selected live candidate.
3. Keep the new flow out of `Main.tscn`. Do not replace or remove the preauthored source/flow;
   automatically propose, schedule, deliver, append, start, expose, review, select, present,
   acknowledge, correct, consume, or persist evidence; or add provider, retry, timing, navigation,
   movement, world, perception, or character mutation.
