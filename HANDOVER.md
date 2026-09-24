# Allrpcs Handover

## Current state

`main` includes merged PR #342 at `d9f9327`. Branch
`codex/phase-7-follow-up-move-cycle-result` completes **Phase 7rd** through **Phase 7rf**. The
implementation is committed at `b81c5b1` and published in draft PR #343 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The third admitted Move cycle now advances through three more exact seams:

- `CharacterSabotageActionMoveCycleResultObservationCoordinator` retains only the exact completed
  generic Move result while the complete admission, recovery-authority, authoring, composition, and
  start graph remains current.
- `CharacterSabotageActionMoveCycleResultBindingCoordinator` pairs that exact observation only with
  the third author's exact `CycleCandidate` binding, leaving the completed task and observation
  intact.
- Explicit acknowledgement accepts only that same bound graph, resets only the third-cycle generic
  task, clears its observation, and retains the exact result and candidate binding as downstream
  tokens.

Null, idle/running, rebuilt-result/request/binding, foreign-actor, wrong-kind, replaced-authority,
stale-graph, and repeated paths remain inert. All three model call counts remain one; authoritative
placement and evidence snapshots remain the exact recovered instances; the ordinary turn stays
empty. No typed request, dispatch/application, another recovery or terminal route, author reset,
turn reservation, cadence, provider, credential, persistence, UI/input, navigation, perception,
dialogue, timer, or new Domain behavior was added. Ordinary startup remains inert.

## Validation and review

- Each slice was test-first: the focused game build failed only for the deliberately absent
  observation coordinator, result-binding coordinator, or acknowledgement members before their
  respective implementations.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  third-cycle completion, exact observation, candidate binding, acknowledgement, and task reset.
- Sandboxed Godot could not open `user://logs`; the identical focused command passed host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, current recovered placement/evidence authority,
  task lifecycle, one-shot behavior, model call counts, inert startup, closed surfaces, docstrings,
  file/line limits, scene dependency order, documentation, formatting, and whitespace. It reduced
  the main smoke below the practical 500-line limit and narrowed the bound-result surface to
  assembly-internal visibility; no unresolved findings remain.

## Exact next bounded slice

Take **Phase 7rg: compose one fresh typed Move request from the exact acknowledged third-cycle
result and candidate binding without starting it**.

Acceptance boundary:

1. Test-first add one composition coordinator over the third-cycle result-binding owner and the
   current `CharacterObjectSabotageMoveController` state.
2. Accept only the exact acknowledged result and exact retained `CycleCandidate` binding while the
   complete third-cycle graph and exact current recovered placement/evidence snapshots remain
   authoritative.
3. Produce one fresh `CharacterObjectSabotageMoveConsiderationRequest` retaining the exact character,
   physical access, world objects, current placements, current evidence, and sole cycle candidate.
4. Reject null, rebuilt-result/binding, foreign candidate, ineligible candidate, replaced authority,
   stale graph, and repeated composition without changing any prior owner, task, authoritative state,
   ordinary turn, or model call count.
5. Extend only the focused switch-gated Move proof. Do not start the typed request, observe/apply a
   typed result, recover or route it, reset either author, reserve the ordinary turn, or add cadence.
6. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to explicitly start only that exact typed request on a separate fresh task/model source.
