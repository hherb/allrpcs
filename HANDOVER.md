# Allrpcs Handover

## Current state

`main` contains merged PR #236 at `3d38437`. The current branch is
`codex/phase-7-concealment-result-acknowledgement` and advances one bounded Phase 7 slice:

- Phase 7eg adds one explicit acknowledgement operation to the completed concealment-result
  presentation coordinator. It resets only the exact result already observed when a selection's
  current concealment is the exact last-presented snapshot, or an abstention's exact request
  concealment and evidence remain current.
- Exact acknowledgement returns the task to idle with no result. The real main scene proves that a
  later explicitly requested abstention can complete and be acknowledged after one selected result,
  without changing the authored source, authoritative world object, presentation, or body position.

Unobserved, stale, idle, running, cancelled, failed, null, and repeated acknowledgements preserve
task, concealment, evidence, and presentation state. `_Process` observes/presents but never
acknowledges. Nothing chooses timing, starts a model or sabotage automatically, persists sabotage,
adds input/UI/events/time/transcripts/movement, invokes a provider during standard validation, or
adds another provider or sabotage effect.

## Validation and review

- The acknowledgement-first smoke began with the expected `CS1061` compile failure for the absent
  operation. The isolated smoke disables coordinator processing after tree entry so unobserved and
  stale rejection are deterministic rather than dependent on frame order.
- The isolated smoke passes exact selected and abstaining acknowledgement, a later explicit start,
  and preservation for idle, running, cancelled, failed, unobserved, stale, null, and repeated
  acknowledgement. The real-main-scene smoke passes selected acknowledgement followed by a later
  acknowledged abstention with exact surrounding state.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 487 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- The initial sandbox quality run reached the known VSTest `SocketException (13)` restriction; the
  identical host-local run passed. All changed authored C# files are below 500 lines with no lines
  longer than 120 characters. No live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7eh: explicitly author one incompatible-belief and incompatible-goal character
fixture**.

Acceptance boundary:

1. Define one immutable Domain declaration that binds two distinct exact characters to one exact
   belief and one exact goal each, all retained by their owning character, plus one bounded explicit
   incompatibility rationale. Do not infer incompatibility from prose, keys, or model output.
2. Compose one fresh representative fixture from the existing sage and enemy-agent roles. Extend
   only their authored belief/goal state if necessary to make both conflicts explicit; preserve each
   role's existing knowledge, capabilities, limitations, and private/public boundary.
3. Prove exact-instance retention, value-distinct character identity, bounded inputs, rejected
   mismatches/nulls/same-character declarations, fresh independent composition, and no mutation.
4. Do not schedule dialogue or sabotage, call a provider, resolve either conflict, change allegiance,
   add scene/persistence/input/UI/events/time/transcripts/movement, or implement another sabotage
   effect.
