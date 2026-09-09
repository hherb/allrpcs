# Allrpcs Handover

## Current state

`main` contains merged PR #155 at `29d9789`. The branch
`codex/phase-4-provider-boundaries` advances Phase 4 through Phase 4c1:

- Phase 4b14 closes the context assembler's generic filtered-entry input. The assembler now
  accepts separate memory and valid-affordance entry collections, requires their exact kinds, and
  assembles derived perception, knowledge, and goals; supplied memories; derived recent speech;
  supplied valid affordances; and exact untrusted player dialogue in roadmap order.
- Phase 4c1 adds a strict bounded JSON decoder for complete character-turn responses. It retains
  the exact request, creates only provider-neutral Domain values, rejects unknown, missing, and
  duplicate structure, parses only canonical dispositions and actions, and applies existing
  structural intention validation.

The Phase 4 context-assembler roadmap item is complete. No memory store/window, affordance source,
object-existence or current-eligibility validation, provider adapter or call, deadline executor,
retry, credential, provider logging, Godot wiring, partial intention execution, persistence, or
gameplay/world-state mutation has been added.

## Validation

Both boundaries began with expected focused compile failures. The focused host-local assembler
suite passes 16 tests and the focused strict-decoder suite passes 22 tests.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, a
zero-warning build, 1,388 Domain tests, 226 Application tests, 43 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox reproduces the known .NET IPC/VSTest denial; identical host-local commands
pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4c2: reject decoded intentions that name unknown authoritative objects**.

Acceptance boundary:

1. Add one pure Application validation boundary for an exact `CharacterTurnResponse` and exact
   authoritative `WorldObjectSnapshot`; do not put provider DTOs or JSON in Application or Domain.
2. Require the exact response request expected by the caller, delegate existing structural
   validation for the immediate intention and every commitment intention, and reject each targeted
   intention whose value-equal `ObjectId` is absent from the authoritative snapshot.
3. Return deterministic provider-neutral validation errors without mutating the response, request,
   intentions, or world-object snapshot. Prove wait, immediate, commitment, unknown-object,
   distinct value-equal identifier, request-mismatch, and multiple-error behavior.
4. Do not claim that object existence proves current action eligibility; derive or serialize valid
   affordances; add destinations not present in the current intention vocabulary; call a provider;
   wire Godot; execute partial or complete intentions; add retries/timeouts/credentials/logging;
   persist; or mutate gameplay state.
