# Allrpcs Handover

## Current state

`main` includes merged PR #321 at `3e6e61f`. Branch
`codex/phase-7-sabotage-action-acknowledgement` advances Phase 7 through two bounded slices:

- Phase 7pb acknowledges only one still-current exact generic sabotage-action result after its
  selection or abstention was bound, delegates reset through the existing selection coordinator,
  retains exact internal acknowledgement provenance, and preserves the selected exact author
  request as a later dispatch token.
- Phase 7pc gives every action-specific author an immutable strongly typed binding between its
  exact singleton generic request and exact candidate. The binder retains only the selected exact
  candidate binding, verifies it remains current through acknowledgement, and leaves abstention
  without an output.

Selected request and candidate provenance is now exact across conceal, move, substitute,
contaminate, misquote, delay, and influence. Unavailable, unbound, running, cancelled, failed,
reset, stale, rebuilt, replaced, foreign, wrong-kind, duplicate, null, externally acknowledged,
and repeated states remain inert. The switch-gated main-scene smoke observes, binds, and
acknowledges one live conceal selection while ordinary startup remains inert.

No slice starts action-specific consideration, dispatches or applies an action, authors evidence,
persists state, or adds input/UI, timing, navigation, movement, perception, dialogue delivery, or
other world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

Implementation commit `23a7cc7` is published in draft PR #322 targeting `main`.

## Validation and review

- Both slices were test-first: the game build first failed on the absent acknowledgement operation,
  and the focused Domain test first failed on the absent typed candidate-binding contract.
- The focused candidate-binding Domain tests pass host-local after the sandbox's known VSTest
  loopback denial. All seven action-author smokes, the expanded author-binding smoke, and the
  switch-gated main-scene smoke pass host-local after the sandbox's known Godot user-log denial.
- `scripts/smoke-godot.sh` now gives the expanded asynchronous author-binding graph enough frames
  to reach and assert its terminal state rather than exiting before completion.
- The final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build,
  2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covered immutable typed candidate provenance, atomic author publication,
  selected and abstaining acknowledgement, replacement and external-reset failure, ordinary-startup
  inertness, unchanged authoritative state, closed public surfaces, documentation, file/line limits,
  and whitespace. Review found and fixed one lifecycle gap: the binder now retains the exact result
  acknowledged through its own guarded operation, so an external reset cannot masquerade as a
  binder-authorized dispatch token.
- Draft PR #322's published heads report four failed jobs and one queued job, all with `steps: []`;
  no checkout, build, test, smoke, or export step ran, so this is Actions/account infrastructure
  state rather than hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7pd: compose one selected candidate binding into its action-specific consideration
request without starting or applying it**.

Acceptance boundary:

1. Test-first add one explicit coordinator that accepts only the binding coordinator's retained
   exact selected author request and exact strongly typed candidate binding after successful
   acknowledgement. Abstention remains inert.
2. Revalidate the corresponding action-specific scene source and authoritative snapshots by exact
   reference and the existing action policy, then create only that action's existing provider-neutral
   consideration request containing the exact selected candidate once.
3. Preserve all seven typed branches without reflection, untyped candidate casts, rebuilt values,
   or kind inference. Stale, replaced, foreign, wrong-kind, null, repeated, and ineligible inputs
   must fail closed without consuming any queued action-specific model.
4. Dependency-order the coordinator in `Main.tscn`; extend the switch-gated smoke through exact
   concealment request composition only. Ordinary startup stays inert.
5. Do not start an action-specific task, dispatch or apply an action, acknowledge an action-specific
   result, author evidence, persist state, or add input/UI, timing, navigation, movement,
   perception, dialogue delivery, or other world behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to start only the selected exact action-specific consideration request.
