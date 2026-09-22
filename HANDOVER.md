# Allrpcs Handover

## Current state

`main` includes merged PR #320 at `a2c7bb3`. Branch
`codex/phase-7-sabotage-action-binding` advances Phase 7 through two bounded slices:

- Phase 7oz binds one exact already-observed generic sabotage-action selection or abstention to
  the still-current exact output of its corresponding action-specific request author.
- Phase 7pa dependency-orders that binder in `Main.tscn`; ordinary startup stays inert, while the
  existing switch-gated request smoke explicitly observes and binds one live conceal selection.

The composer now internally retains the exact seven-output snapshot used by successful composition.
The binder accepts only the composer's exact request, the selection coordinator's exact current
observation, the task's same exact completed result, and an unchanged canonical author-output
snapshot. Selection exposes only the corresponding exact singleton author request; abstention binds
once without exposing an output. Unavailable, idle, running, reset, cancelled, failed, stale,
rebuilt value-equal, foreign-actor, wrong-kind, duplicate, null, and repeated states remain inert.

No slice acknowledges the generic result, selects an action-specific candidate, dispatches or
applies an action, authors evidence, persists state, or adds input/UI, timing, navigation,
perception, dialogue delivery, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both slices were test-first: the isolated game build first failed because the binding coordinator
  did not exist, and the switch-gated main-scene smoke then failed on its absent scene node.
- The isolated binding smoke and switch-gated main-scene smoke pass host-local after implementation.
- The sandboxed `./scripts/quality.sh` reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`. The identical host-local command passes restore,
  formatting, a zero-warning build, 2,728 Domain tests, 586 Application tests, 1,220 Infrastructure
  tests, all 8 architecture tests, scenario validation, every credential-free Godot smoke, and the
  Compatibility renderer launch.
- Session-wide review covered exact composer provenance, task/result/observation identity, canonical
  author mapping, selection and abstention, replacement and stale failure, ordinary-startup
  inertness, unchanged authoritative state, closed surfaces, documentation, file/line limits, and
  whitespace. Review found and fixed a lifecycle gap: the binder now also requires the task to
  retain the same exact completed result, so a reset or later terminal task cannot authorize a
  stale observation. The affected build and both smokes pass after the fix.

## Exact next bounded slice

Take **Phase 7pb: explicitly acknowledge one exact bound sabotage-action result**.

Acceptance boundary:

1. Test-first extend the binding coordinator with one explicit acknowledgement operation that is
   available only after its exact selection or abstention has bound successfully.
2. Require the task to remain completed with the same exact bound result, the selection coordinator
   to retain that same observation, the composer to retain the same request, and every author output
   to remain the exact snapshot used by composition. Delegate reset only through the existing
   selection coordinator's exact acknowledgement operation.
3. Preserve the selected exact author request after acknowledgement as the later dispatch token;
   preserve abstention as no selected output. Keep unavailable, unbound, running, cancelled, failed,
   reset, stale, rebuilt, replaced, null, and repeated acknowledgement inert, with composer, authors,
   authoritative state, and any queued model unchanged.
4. Update the isolated and switch-gated main-scene smokes to acknowledge only after exact binding.
   Ordinary startup must remain inert.
5. Do not yet select an action-specific candidate, dispatch or apply an action, author evidence,
   persist state, or add input/UI, timing, navigation, movement, perception, dialogue delivery, or
   other world behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to bind the selected author request to its exact action-specific candidate without applying
   it.
