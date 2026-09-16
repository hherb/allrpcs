# Allrpcs Handover

## Current state

PR #279 is merged into `main` at `07dc36b`. Branch
`codex/phase-7-misquotation-occurrence-flow` advances Phase 7 through three bounded slices:

- Phase 7jr adds `CharacterDialogueMisquotationOccurrenceCoordinator`, which snapshots one
  caller-supplied hearing query, preflights the exact proposal actor, text, retained source delivery
  mode, and non-empty audience, then delegates once to `DialogueEventController.TryOccur`. The
  resulting exact dialogue event is the occurrence's exact delivered event.
- Phase 7js owns that coordinator inertly in `Main.tscn`. Only a dedicated smoke switch supplies an
  explicit delivery; ordinary startup does not occur a misquotation.
- Phase 7jt adds `CharacterDialogueMisquotationEvidenceCandidateController`, which retains one
  potential witness-evidence candidate only for the occurrence coordinator's exact current
  occurrence and only when the existing observability policy accepts the explicit actor and
  witness.

Implementation commit `485dfda` is published in draft PR #280 targeting `main`.

No misquotation is automatically proposed, scheduled, delivered, appended to a transcript,
evaluated, exposed, reviewed, selected, presented, acknowledged, corrected, consumed, or persisted.
The existing temporary preauthored evidence source is unchanged and is not yet connected to the new
occurrence or candidate owners. No provider, retry, timing, navigation, movement, world, perception,
character, or general dialogue-controller behavior changed. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jr began with the expected missing-coordinator build failure. Its focused smoke proves
  invalid hearing queries, distinct value-equal actors, wrong text, wrong delivery mode, absent
  listeners, nulls, and repetition fail closed while one valid delivery becomes both the ordinary
  controller's exact event and the retained occurrence.
- Phase 7js began with the expected missing-main-node smoke failure. Its switch-gated main-scene
  smoke proves inert startup and one exact occurrence without transcript, task, exposure, or
  existing source changes.
- Phase 7jt began with the expected missing-controller build failure. Its focused smoke proves
  unavailable, stale, unheard, null, and repeated candidate requests remain inert while one
  accepted witness retains the exact current occurrence.
- Session review found that the occurrence coordinator initially enumerated the caller's hearing
  candidates separately for preflight and delegation. It now snapshots them once; a changing-list
  regression proves both selections use the same retained query. No unresolved review issue remains.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch. The sandbox-only VSTest `SocketException (13)` was retried unchanged on the host.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` and
  `git diff --check` pass.
- Draft PR #280 is open and mergeable. Hosted run `35083046594` failed every job with `steps: []`,
  including Pure C# quality and Headless Godot smoke; this is Actions/account infrastructure rather
  than hosted product-test evidence and was not retried unchanged.

## Exact next bounded slice

Take **Phase 7ju: compose exact-current misquotation evidence-candidate ownership inertly into the
main scene**.

Acceptance boundary:

1. Add `CharacterDialogueMisquotationEvidenceCandidateController` to `Main.tscn` after the
   occurrence coordinator and extend the dedicated occurrence smoke to request one candidate only
   after its switch-gated exact occurrence.
2. Prove the candidate retains that exact current occurrence and the explicit existing witness,
   while unavailable, stale, unheard, repeated, and ordinary-startup paths remain inert. Preserve
   the existing preauthored source, transcript, task, exposure, view, presentation, world, and
   perception state by exact reference.
3. Do not replace or remove `TemporaryCharacterDialogueMisquotationEvidenceSource`; automatically
   propose, schedule, deliver, append, expose, evaluate, select, present, acknowledge, correct,
   consume, or persist evidence; or add provider, retry, timing, navigation, or movement behavior.
