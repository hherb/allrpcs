# Allrpcs Handover

## Current state

`main` includes merged PR #359 at `29f0862`. Branch
`codex/phase-7-fifth-move-turn-boundary` advances the exact fifth Move cycle through ordinary-turn
boundary binding and scheduling authorization after its completion admission.

The boundary coordinator accepts only the exact current fifth-cycle admission and the exact empty
`CharacterTurnActivityController` reference. The authorization coordinator revalidates that whole
graph and retains one fresh `CharacterSabotageOrdinaryTurnSchedulingAuthorization`. Null, rebuilt,
substituted non-empty activity, stale-graph, and repeated inputs remain inert. No turn is started,
reserved, replaced, or cancelled; no sixth sabotage cycle is entered; all seven model-call counts
remain one; ordinary startup remains inert.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both slices were introduced test-first. Each focused game build failed only with `CS0246` for its
  absent coordinator, then passed with zero warnings after the minimum implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8, including
  rejected substitute references, exact one-shot binding and authorization, preserved recovered
  authority, an unchanged empty ordinary-turn slot, and all seven model-call counts remaining one.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch. The sandbox-only run reached the build and then
  hit the known VSTest loopback `SocketException (13)`; the identical host-local command passed.
- Complete-diff review found no remaining exact-provenance, stale-authority, one-shot,
  scene-ordering, public-surface, formatting, line-length, documentation, or whitespace issue.
- Hosted checks with `steps: []` are Actions/account infrastructure, not product-test evidence.

## Exact next bounded slice

Consume only the exact fifth-cycle scheduling authorization once at an explicit successor boundary,
stopping before cycle-start admission, authoring, consideration, or a sixth sabotage action.

Acceptance boundary:

1. Test-first add one successor-boundary coordinator over only the exact current fifth-cycle
   authorization.
2. Revalidate the complete completion, admission, ordinary-turn boundary, and authorization graph.
3. Reject null, rebuilt authorization, rebuilt admission, substituted activity, stale graph, and
   repeated entry.
4. Retain only the exact authorization; do not copy completion provenance or mutate any owner.
5. Do not admit cycle start, invoke an author/model, compose or start consideration, reserve a turn,
   or begin a sixth sabotage action.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to admit only that exact current successor boundary through the pure cycle-start gate.
