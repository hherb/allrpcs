# Allrpcs Handover

## Current state

PR #281 is merged into `main` at `7cbc2ea`. Branch
`codex/phase-7-misquotation-live-decision-flow` advances Phase 7 through two bounded slices:

- Phase 7jx adds
  `CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController`. It starts one
  decision only for the candidate-request controller's exact current request and only while the
  shared task is idle. Request currentness and task availability are checked before model take.
- Phase 7jy owns that flow immediately after its live request dependency in `Main.tscn`. Only the
  dedicated occurrence smoke invokes it, after explicitly creating the exact occurrence, candidate,
  and request; one supplied model then completes for that exact request and candidate.

Implementation commit `9d56eb7` is published in draft PR #282 targeting `main`.

The preauthored `TemporaryCharacterDialogueMisquotationEvidenceSource` and its existing
consideration flow remain unchanged. Ordinary startup does not start a task or consume a model. No
live result is exposed, reviewed, presented, acknowledged, corrected, consumed, or persisted by the
new flow. No provider, retry, timing, navigation, movement, world, perception, character, transcript,
or general dialogue-controller behavior changed. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jx began with the expected missing-controller smoke failure. Its standalone smoke proves
  unavailable, null, distinct stale, overlapping, cancelled, and completed/repeated starts preserve
  a queued one-shot model and the exact request; after explicit task reset, one accepted start
  completes with the exact live request and candidate.
- Phase 7jy began with the expected missing-main-node smoke failure. The extended main-scene smoke
  proves ordinary startup stays inert and only the switch-gated occurrence/candidate/request chain
  starts the exact decision, while transcript, exposure, view, presentation, world, perception, and
  preauthored source references remain unchanged.
- Session review inspected every changed and new path and found no unresolved issue. The repository
  formatter, `git diff --check`, and a 120-column check pass. The production controller is 40
  lines and the focused smoke is 280 lines.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch. The sandbox-only VSTest `SocketException (13)` was retried unchanged on the host.

## Exact next bounded slice

Take **Phase 7jz: expose and acknowledge one exact completed live-candidate decision through the
existing main-scene evidence owners**.

Acceptance boundary:

1. Extend only the dedicated switch-gated live-occurrence smoke so its exact selected result is
   observed once by `CharacterDialogueMisquotationEvidenceExposureCoordinator`, appends the exact
   live candidate to the current exposure, and refreshes the exact witness view.
2. Prove repeated observation is inert, then acknowledge only that observed exact result through the
   existing coordinator. The task returns to idle while the exact exposure and witness view remain;
   stale or mismatched state must fail closed.
3. Do not add automatic occurrence, candidate, request, or decision start; replace the preauthored
   source/flow; automatically select a reviewed entry, present UI, correct dialogue, consume or
   persist evidence; or add provider, retry, timing, navigation, movement, world, perception,
   character, transcript, or general dialogue-controller mutation.
