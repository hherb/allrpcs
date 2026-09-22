# Allrpcs Handover

## Current state

`main` includes merged PR #319 at `9e2f010`. Branch
`codex/phase-7-sabotage-action-request-composition` advances Phase 7 through three bounded slices:

- Phase 7ow composes the seven action-specific authors' exact current outputs into at most one
  fresh same-actor request in canonical roadmap order.
- Phase 7ox starts the existing explicit consideration flow only for that composer-owned request,
  preserving queued one-shot models across stale and overlapping starts.
- Phase 7oy dependency-orders all seven authors, the composer, and exact-start coordinator in
  `Main.tscn`; ordinary startup stays inert, while a switch-gated smoke completes one live
  conceal-only selection without observation, acknowledgement, dispatch, or application.

The composer accepts only an exact seven-output snapshot, validates each available author output
against its canonical singleton and the same exact actor, rejects empty, stale, foreign,
value-equal replacement, wrong-kind, duplicate-kind, null, and repeated composition, and never
reruns an action-specific eligibility gate. No slice selects an action-specific candidate, binds a
generic selection back to its author, dispatches or applies an action, acknowledges a result,
authors evidence, persists state, or adds input/UI, timing, navigation, perception, or dialogue
delivery.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Every slice was test-first: the focused game build failed for the missing composer or flow type,
  and the switch-gated main-scene smoke failed on the absent nodes before their implementation.
- Focused builds pass with zero warnings. Both isolated smokes and the switch-gated main-scene
  smoke pass host-local; the main smoke proves the live concealment/evidence snapshots remain the
  exact unchanged instances and the generic result remains unobserved.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` and
  `git diff --check` pass.
- The sandboxed full quality run reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`. The identical host-local `./scripts/quality.sh` passes
  restore, formatting, a zero-warning build, 2,728 Domain tests, 586 Application tests, 1,220
  Infrastructure tests, all 8 architecture tests, scenario validation, every credential-free
  Godot smoke, and the Compatibility renderer launch.
- Session-wide review covered exact author-output provenance, same-actor and canonical-order
  validation, one-shot and queued-model ownership, stale/replacement/overlap failure, inert main
  startup, unchanged authoritative state, closed public surfaces, documentation, file/line limits,
  and whitespace. Review found and fixed two smoke paths that dereferenced the composed request
  before checking composition success; the affected build and smokes pass after the fix. No
  unresolved finding remains.

## Exact next bounded slice

Take **Phase 7oz: bind one exact completed sabotage-action selection back to its author output**.

Acceptance boundary:

1. Test-first add one isolated scene coordinator that explicitly observes the existing selection
   coordinator and accepts only a result whose request is the composer's exact current request.
2. For a selected kind, expose only the exact current request output of the one corresponding
   action-specific author. Require the same exact actor, the canonical singleton, and exact
   membership in the composed request. Preserve explicit abstention as no selected author output.
3. Keep unavailable, running, cancelled, failed, stale, rebuilt value-equal, foreign-actor,
   wrong-kind, duplicate, null, and repeated binding inert. Prove selection and abstention plus
   unchanged composer, author outputs, task result, selection observation, and authoritative state.
4. Do not yet acknowledge the generic result, wire this binding coordinator into `Main.tscn`,
   select an action-specific candidate, dispatch or apply any action, author evidence, persist
   state, or add input/UI, timing, navigation, movement, perception, dialogue delivery, or other
   world behavior.
5. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to dependency-order the exact author binding in `Main.tscn` without dispatching or applying
   the selected action.
