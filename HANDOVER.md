# Allrpcs Handover

## Current state

`main` includes merged PR #300 at `410350c`. Branch `codex/phase-7-delay-decisions` advances the
character-targeted delay chain through three bounded slices:

- Phase 7mf adds `CharacterTargetSabotageDelayDecision`, its exact-current acceptance policy, and
  `CharacterTargetSabotageDelayConsiderationResult`. Only the exact request actor may abstain or
  select an exact currently eligible request-member candidate against unchanged access/world state.
- Phase 7mg adds `CharacterTargetSabotageDelayConsiderationTransition`. Accepted abstention is inert;
  an accepted selection delegates through the existing immutable delay transition and produces fresh
  event-log and qualitative-time snapshots.
- Phase 7mh adds provider-neutral `ICharacterTargetSabotageDelayDecisionModel` and
  `CharacterTargetSabotageDelayConsiderationController`. The controller forwards the exact request
  and caller cancellation once, then validates the returned decision through the Domain result.

The branch changes only bounded Domain/Application contracts and tests plus README, ROADMAP, and this
handover. It does not bind a provider or settings, own background work, automatically apply delay,
recover time, author/authenticate evidence, add scene ownership, persist state, or add UI/input or
other autonomous behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched.

## Validation and review

- Test-first validation produced the expected missing-type compiler failures for each decision,
  result, transition, model, and controller before the smallest implementations were added.
- Focused host-local validation passes all 7 decision/result tests, 3 consideration-transition
  tests, and 6 Application controller/model tests. Sandboxed .NET runs were blocked by local IPC or
  VSTest loopback permissions; identical host-local commands supply the test evidence.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,687 Domain tests, 566 Application tests, 1,028 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact actor and candidate identity, stale/ineligible world-state
  rejection, abstention inertness, immutable event application, cancellation/failure propagation,
  closed public surfaces, documentation, file size, line length, formatting, whitespace, and scoped
  paths. The review added explicit already-recorded-event coverage; no unresolved finding remains.
- Draft PR #301's initial hosted run `35228368467` completed with all five jobs reporting
  `steps: []`. That is Actions/account infrastructure state rather than hosted product-test evidence;
  the unchanged jobs were not retried.

Implementation commit `deff62a` is published in draft PR #301 targeting `main`.

## Exact next bounded slice

Take **Phase 7mi: retain and supply one provider-neutral target-delay decision model**.

Acceptance boundary:

1. Test-first add one immutable binding retaining the exact selected OpenAI settings and exact
   `ICharacterTargetSabotageDelayDecisionModel`, without loading credentials or invoking the model.
2. Add one temporary provider-neutral source with exact-instance configured binding initialization,
   replacement, and clearing; one queued one-shot model must take precedence, and the same stable
   fail-closed offline model must be returned when neither source exists.
3. Require exact-reference compare-and-swap for configured replacement/clear, preserve any queued
   one-shot across those operations, and expose only closed read-only/provider-neutral surfaces.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet own a
   task, start consideration, decode/serialize provider traffic, load credentials, send HTTP, wire
   `Main.tscn`, apply/recover a delay autonomously, infer evidence, persist state, or add UI/input.
