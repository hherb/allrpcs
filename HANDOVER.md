# Allrpcs Handover

## Current state

`main` includes merged PR #331 at `a2b1826`. Branch
`codex/phase-7-sabotage-turn-completion` completes **Phase 7pv** and **Phase 7pw**.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

`CharacterSabotageTurnCompletionCoordinator` accepts only the terminal coordinator's exact reset
author request and canonical kind after its retained output is clear and exactly one matching route
owner still retains its current authoritative result. Its one-shot token retains the request's exact
actor, the exact request, and the exact kind without copying route output or changing prior owners.

`CharacterSabotageTurnSchedulingEligibilityPolicy` is pure. It admits only the coordinator's exact
current completed-turn token when that token's actor, request, and one canonical kind remain an
exact-reference graph. It does not schedule, author, start, consume, reset, or mutate anything.
Ordinary startup remains inert.

## Validation and review

- Test-first game builds failed only for the deliberately missing Phase 7pv coordinator and Phase
  7pw policy before each implementation was added.
- The focused switch-gated Move proof passes host-local through terminal reset, exact one-shot turn
  completion, rejected null/rebuilt/foreign/wrong/repeated inputs, and pure exact-token eligibility.
- Sandboxed Godot still hits the known `user://logs` permission crash; the identical focused command
  passes host-local with Godot 4.6.3 Mono.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covers exact reset/actor/request/kind provenance, canonical-kind identity, exactly
  one current branch owner, stale and multiple-owner rejection, one-shot completion, pure exact-token
  eligibility, preserved prior owners and models, ordinary-startup inertness, docstrings, practical
  file-size and line-length limits, formatting, and whitespace. Review added the missing internal
  token-constructor contract docstring; no behavioral defect remained.

## Exact next bounded slice

Take **Phase 7px: retain one exact ordinary-scheduling admission for the eligible completed-turn
token without starting another sabotage action**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot admission coordinator that accepts only the exact
   `CharacterSabotageTurnCompletionCoordinator.Completion` after the pure scheduling eligibility
   gate accepts that same instance.
2. Retain only that exact completed-turn token as admission provenance. Do not copy its actor,
   request, or kind into a replacement token and do not retain any route output.
3. Reject null, rebuilt, replaced, malformed-provenance, and repeated tokens without changing the
   completion coordinator, terminal route, any prior route owner, authoritative state,
   presentation, task, result, model source, or admission output.
4. Extend only the focused switch-gated Move proof. Ordinary startup must remain inert; do not
   author, start, observe, acknowledge, dispatch, apply, reset, or schedule an action.
5. Do not add cadence, timers, provider transport, credentials, persistence, UI/input, navigation,
   movement, perception, dialogue delivery, new Domain behavior, or a claim that ordinary
   enemy-agent sabotage is enabled.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to connect this exact admission to an existing ordinary turn-scheduling boundary without
   starting the sabotage flow itself.
