# Allrpcs Handover

## Current state

`main` includes merged PR #368 at `2801e10`. Branch
`codex/phase-7-acceptance-turn-lifecycle` continues the switch-gated Phase 7 acceptance path without
extending the closed repeated Move-cycle chain. Draft PR #369 carries this branch.

The ordinary-turn Delay path now resolves its exact sabotage-owning character turn after terminal
route reset, completion exposure, and admission. Domain state `Resolved` is a distinct response-free
terminal state: it retains the exact request, fabricates no dialogue response, is not player
cancellation, and permits the existing turn-start policy to accept a later turn. One scene coordinator
revalidates the original ordinary-turn authoring provenance through the exact acknowledged selected
Delay author, exact admitted completion, and current recovered event-log/world-time authority before
installing the successor. The existing ordinary-turn boundary rejects the active `AwaitingResponse`
activity and binds only that exact installed `Resolved` successor.

Ordinary startup remains inert. The path still runs only under its smoke environment variable: no
ordinary scheduler produces a real sabotage-turn opportunity, and Delay/Influence still have no
player-visible investigation surface. Broad Phase 7 outcomes and final acceptance remain unchecked.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route,
turn completion, scheduling authorization, cycle boundary, or admission.

## Validation and review

- The switch-gated proof was extended first; the focused game build failed with `CS0246` for the
  intentionally absent lifecycle coordinator, then passed zero-warning after implementation.
- The first sandboxed focused Domain run hit the known VSTest loopback `SocketException (13)`; the
  identical host-local run passed, and the final review-driven focused run passed all 2,748 Domain
  tests.
- The host-local switch-gated ordinary-turn Godot proof passed with the generic, typed Delay, and
  recovery model-call counts remaining one each.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,745 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility launch. The three review-added state-surface
  cases then passed in the final 2,748-test focused Domain run, zero-warning game build, and switch-gated
  Godot proof.
- Complete-diff review fixed one misleading next-slice cadence statement and added missing `Resolved`
  rejection/eligibility coverage across the remaining turn consumers. It found no fabricated response,
  cancellation reuse, request replacement, stale-authority acceptance, extra model use, automatic
  scheduling, Move-chain extension, line-length issue, or whitespace error.
- Draft PR #369 targets `main`. Hosted run `36108207789` failed all five jobs with `steps: []`; no
  workflow step executed, so this is Actions/account infrastructure rather than product-test evidence.

## Exact next bounded slice

Define only the provider-neutral ordinary-play scheduling contract that produces one sabotage-turn
opportunity from an authoritative meaningful-world-event cadence signal.

Acceptance boundary:

1. Start test-first in the pure Domain/Application layer; do not extend the current smoke chain first.
2. Retain the exact scene-owned enemy actor and exact authoritative event/cadence provenance in one
   immutable opportunity token.
3. Require an explicit eligible meaningful-world-event signal; do not use wall-clock time or reuse the
   unrelated independent-task travel cadence.
4. Reject null, rebuilt actor, foreign or non-authoritative event, stale cadence, duplicate, and repeated
   inputs without partial output or mutation.
5. Stop before creating or reserving a `CharacterTurnActivity`, invoking any sabotage author, consuming
   a model, applying an action, or chaining another opportunity.
6. Add no sixth or later Move-cycle seam and no provider, persistence, UI, presentation, or investigation
   work.
7. Keep ordinary startup inert without an explicit eligible signal; update `README.md`, `ROADMAP.md`, and
   `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added.
8. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next acceptance slice.
