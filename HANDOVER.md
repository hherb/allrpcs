# Allrpcs Handover

## Current state

`main` contains merged PR #191 at `05b46f2`. The current branch is
`codex/phase-6-ingredients-preparation`.

Phase 6 now includes hidden disease truth and meaningful-event progression, qualitative condition
projection, dormant-controller selection, exact-event exposure/protection/care/cure, bounded patient
assessment, and the ingredient and initial preparation foundations.

Canonical ingredient identities are distinct from world-object identities. Immutable ingredient
state retains an exact authored definition, a positive integer quantity in the closed count,
milligram, or millilitre vocabulary, and exact closed freshness, pre-preparation physical-condition,
and contamination values. Units do not convert implicitly.

Preparation plans copy one to 16 exact grinding, heating, steeping, cooling, or fermentation steps
in authored order. Repeated process kinds are legal. Immutable progress exposes only the exact next
step, and its pure transition advances once only for that exact step; a distinct value-equal,
out-of-order, or post-completion step fails closed.

There are no ingredient decay or state transitions, recipe requirements/results, preparation tools,
timing or temperatures, dosage, administration, inventory/scene/persistence/provider wiring,
experiment journal, conclusions, or ingredient-driven treatment effects. The broader preparation
roadmap item remains unchecked.

## Validation

- complete host-local `./scripts/quality.sh` passed: workflow validation, restore, formatting, and
  zero-warning solution build
- 1,699 Domain, 393 Application, 423 Infrastructure, and 8 architecture tests passed
- representative discovery scenario validated 5 evidence definitions
- all 77 credential-free Godot smokes passed
- bounded 60-frame Compatibility-renderer launch passed on Godot 4.6.3 .NET

The sandboxed focused test commands reproduced the known VSTest
`System.Net.Sockets.SocketException (13): Permission denied`; identical host-local commands passed.

PR #191's final hosted run `34573972410` reported all five jobs as failures before merge. Its jobs
completed within seconds, consistent with the already observed unavailable account/infrastructure
condition. Do not treat it as product-test evidence or retry/dispatch it without a changed external
condition.

## Exact next bounded slice

Take **Phase 6g3: authored preparation recipe foundation**.

Acceptance boundary:

1. Add a canonical preparation-recipe identity and the smallest immutable authored recipe that
   retains that exact identity and one exact existing preparation plan.
2. Add immutable positive input requirements retaining exact ingredient definitions and quantities;
   copy them in stable supplied order, bound their count, and reject nulls and duplicate ingredient
   identities without converting units or consuming ingredient state.
3. Retain one exact authored result ingredient definition and quantity, without executing the plan or
   claiming that inputs can produce it.
4. Add focused reflection-backed tests for exact-instance retention, copied collections, count and
   duplicate boundaries, canonical identifier behavior, nulls, and a minimal public API.
5. Do not yet add eligibility/matching, consumption, result creation, tools, timing/temperature,
   events, dosage, administration, inventory/scene/persistence/provider wiring, experiments,
   journals, discovery, or treatment effects; keep the broader preparation roadmap item unchecked.
