# Allrpcs Handover

## Current state

PR #277 is merged into `main` at `a179b8d`. Branch
`codex/phase-7-misquotation-evidence-temporary-ui` advances Phase 7 through isolated and inert
main-scene temporary UI for one exact witness presentation:

- `TemporaryCharacterDialogueMisquotationEvidencePresentationPanel` is an initially hidden
  `CanvasLayer`. It accepts only the presentation owner's exact current non-null snapshot and
  renders the exact source and delivered utterance texts separately under the neutral labels
  `Source statement` and `Delivered statement`;
- stale, distinct equivalent, repeated, and null presentation attempts fail closed. The panel
  clears and hides when its exact snapshot becomes stale or is acknowledged, without changing its
  retained view, selection, exposure, evidence, or dialogue events; and
- `Main.tscn` owns the panel inertly. Ordinary startup, selection, abstention, failure, and
  acknowledgement do not present it automatically. Only the existing switch-gated main-scene
  smoke explicitly selects, presents, verifies, and acknowledges one exact snapshot.

Implementation commit `83e2fac` is published in draft PR #278 targeting `main`.

No evidence picker or acknowledgement input was added. No evidence is automatically selected,
presented, acknowledged, discovered, corrected, consumed, or persisted. Neither statement is
called true, false, corrected, or authoritative. No dialogue, transcript, character, event,
audience, world, perception, or other authoritative state is mutated. No credential or provider
I/O, retry, timing, scheduling, navigation, or movement behavior was added. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jl began with the expected missing-panel game build failure. The implemented isolated
  panel smoke passes hidden inert startup, exact snapshot and text retention, neutral separate
  labels, stale and distinct-equivalent rejection, acknowledgement hiding, unchanged source state,
  a null guard, and a closed public surface.
- Phase 7jm extends the environment-gated main-scene smoke. It passes inert ownership, exact
  screen-space rendering, and acknowledgement hiding while retaining the exact view, selection,
  exposure, evidence, and source/delivered events.
- Session-wide review found that the initial `VBoxContainer` was mounted directly in the world
  canvas and could move under camera transforms. The seam now owns a `CanvasLayer`; both focused
  smokes and the full suite pass after that fix. No unresolved review issue remains.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch. The sandbox-only VSTest `SocketException (13)` was retried unchanged on the host.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` and
  `git diff --check` pass.

## Exact next bounded slice

Take **Phase 7jn: dismiss one exact temporary witness presentation through explicit neutral UI
input**.

Acceptance boundary:

1. Add one neutral `Dismiss` button to the temporary panel. A press may delegate only the panel's
   exact current view, selection, and non-null presentation through the existing presentation flow
   acknowledgement; absent, stale, distinct equivalent, and already-acknowledged state stays inert.
2. Prove isolated one-shot dismissal, hiding and text clearing, unchanged
   view/selection/exposure/evidence/events, repeated and stale inertness, ordinary hidden startup,
   and a closed surface. Update the switch-gated main-scene smoke to press that button instead of
   calling acknowledgement directly.
3. Do not add an evidence list or selector; automatic selection, presentation, acknowledgement, or
   dismissal; truth or correction labels; discovery, consumption, or persistence; dialogue
   delivery; authoritative mutation; or provider, retry, timing, scheduling, navigation, or
   movement behavior.
