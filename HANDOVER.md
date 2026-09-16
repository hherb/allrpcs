# Allrpcs Handover

## Current state

PR #278 is merged into `main` at `df4d508`. Branch
`codex/phase-7-misquotation-evidence-dismissal` advances Phase 7 through explicit temporary
witness-evidence review, selection, presentation, and dismissal:

- the presentation panel's neutral `Dismiss` button delegates its exact current witness view,
  selection, and non-null presentation through the existing acknowledgement flow, then clears only
  its temporary text and visibility;
- `TemporaryCharacterDialogueMisquotationEvidenceReviewPanel` lists the exact current witness
  view in stable exposure order only after `Review evidence`, using separate neutral source and
  delivered statement text. Empty and stale views remain hidden, and `Close review` clears the list
  without selecting anything; and
- selecting one reviewed list item resolves only that exact retained entry through the existing
  one-shot selection and presentation owners. Missing, stale, repeated, and out-of-range
  state stays inert.

Implementation commit `2f27104` is published in draft PR #279 targeting `main`.

Evidence creation and exposure remain switch-gated. No misquotation is automatically proposed,
delivered, exposed, reviewed, selected, presented, acknowledged, dismissed, discovered, corrected,
consumed, or persisted. Neither statement is called true, false, corrected, or authoritative. No
dialogue, transcript, character, event, audience, world, perception, navigation, movement, provider,
retry, timing, or scheduling behavior changed. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jn began with the expected focused behavioral failure: the new `Dismiss` button had no
  handler. The completed isolated and main-scene smokes prove exact one-shot acknowledgement,
  synchronous hiding and text clearing, absent/stale/repeated inertness, and unchanged retained
  source state.
- Phase 7jo began with the expected missing-type build failure. Its focused smoke proves explicit
  empty-view inertness, stable exact source/delivered rendering, stale clearing, and a closed public
  surface.
- Phase 7jp began with the expected focused behavioral failure. Its focused and main-scene smokes
  prove exact reviewed-entry selection and presentation plus absent, stale, repeated, and
  out-of-range inertness without source mutation.
- Phase 7jq began with the expected focused behavioral failure. Review can now close without a
  selection and can be reopened against the same exact current view.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch. The sandbox-only VSTest `SocketException (13)` was retried unchanged on the host.
- Session-wide review covered every changed implementation, scene, smoke, documentation, and
  validation-script path. Exact references remain the only selection authority, stale UI clears
  without mutation, and hidden repeated signals stay inert. No unresolved review issue remains.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` and
  `git diff --check` pass.
- Draft PR #279 is open and mergeable. Initial hosted run `35081208698` failed every job with
  `steps: []`, including Pure C# quality and Headless Godot smoke; this is Actions/account
  infrastructure rather than hosted product-test evidence and was not retried unchanged.

## Exact next bounded slice

Take **Phase 7jr: derive one explicit misquotation occurrence from the ordinary dialogue-event
occurrence seam**.

Acceptance boundary:

1. Add one bounded scene coordinator that accepts the existing exact proposal plus an explicit
   delivered `DialogueDelivery` and caller-supplied hearing query, delegates occurrence to the
   existing `DialogueEventController.TryOccur`, and constructs a misquotation occurrence only when
   the controller's resulting exact event satisfies the existing actor, text, mode, and audience
   contract.
2. Prove one valid delivery is both `DialogueEventController.LastOccurredEvent` and the exact
   occurrence's delivered event. Invalid hearing queries, a retained stale prior event, wrong actor,
   wrong text, wrong delivery mode, absent witness, null input, and repeated use must fail closed
   without replacing the last accepted occurrence.
3. Do not automatically schedule or deliver the misquotation; expose, evaluate, select, present,
   acknowledge, consume, correct, or persist evidence; mutate transcripts, characters, world state,
   or perception; change the general dialogue controller; or add provider, retry, timing,
   navigation, or movement behavior.
