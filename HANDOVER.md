# Allrpcs Handover

## Current state

`main` includes merged PR #345 at `c3f5c4e`. Branch
`codex/phase-7-third-move-terminal-route` completes **Phase 7rn** through **Phase 7rq**. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact recovered third-cycle Move now advances through four additional bounded seams:

- A terminal-route coordinator retains only the exact guarded author request, canonical `Move`
  kind, and still-current recovered placement/evidence authority.
- Its explicit reset clears only those route references once and records exact author/kind
  provenance while preserving recovery authority.
- A completion coordinator exposes one fresh `CharacterSabotageTurnCompletion` only from that exact
  reset route.
- A scheduling-admission coordinator retains only that exact completion after the existing pure
  scheduling gate accepts it.

Null, rebuilt-author/request/placement/evidence/completion, foreign-actor, wrong-kind,
replaced-authority, stale, and repeated paths remain inert. All four bounded model calls still occur
once, the ordinary turn stays empty, and ordinary startup remains inert. No ordinary-turn binding or
reservation, scheduling authorization, another cycle, cadence, author reset, provider, credential,
persistence, UI/input, navigation, perception, dialogue, timer, or new Domain behavior was added.

## Validation and review

- Each slice was test-first: the focused game build failed only for the deliberately absent
  terminal-route, reset, completion, or admission surface before its implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact third-cycle recovery, route retention/reset, turn completion, and scheduling admission.
- Sandboxed Godot could not open `user://logs`; the identical focused command passed host-local.
- The complete session diff was reviewed for exact-reference provenance, stale-state rejection,
  one-shot behavior, scene dependency order, model-call preservation, and scope exclusions.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the Compatibility launch. Formatting and whitespace also pass.

## Exact next bounded slice

Take **Phase 7rr: bind the exact admitted third-cycle completion to the empty ordinary-turn
boundary**.

Acceptance boundary:

1. Test-first add one boundary coordinator over the exact current Phase 7rq admission and the
   scene's exact still-empty ordinary-turn reference.
2. Retain only that exact admission/empty-turn pair once; do not reserve or replace the slot.
3. Reject null, rebuilt-completion/request, foreign actor, wrong kind, active or replaced turn,
   stale recovery/route, and repeated inputs without changing prior owners, authoritative state, or
   any model call count.
4. Extend only the focused switch-gated Move proof. Do not create scheduling authorization, enter
   another cycle, invoke an author, start a request, or add cadence.
5. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to retain one fresh scheduling authorization without reserving the ordinary turn.
