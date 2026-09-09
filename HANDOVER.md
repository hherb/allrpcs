# Allrpcs Handover

## Current state

`main` contains merged PR #156 at `6132d88`. The branch
`codex/phase-4-response-validation` advances Phase 4 through Phase 4c5:

- Phase 4c2-c3 add one pure provider-neutral response gate. It requires the exact expected turn
  request, delegates structural validation for the immediate and commitment intentions, rejects
  targets absent from the authoritative world-object snapshot, and requires the immediate action
  to be `Wait` or an exact type/value-equal target pair in a supplied current-affordance snapshot.
  Present eligibility is not incorrectly applied to future commitments.
- Phase 4c4 permits one retry only for text-free unavailable, rate-limited, or transport failures.
  Authentication, invalid responses, provider rejection, exposed text, cancellation, timeout, and
  the second failed attempt are never retried.
- Phase 4c5 executes one provider-neutral model stream under one caller-supplied deadline. It
  preserves exact completed updates, translates caller cancellation and deadline expiry into
  distinct terminal states, rejects missing termination as an invalid response, and applies the
  bounded retry policy without executing intentions.

The structured-output-validation and safe-retry roadmap items are complete. The initial intention
vocabulary has no destination-bearing action, so provider JSON cannot express an accepted
destination. No affordance derivation or serialization, streaming presentation, Godot wiring,
provider adapter or call, credentials, provider logging, partial intention execution, persistence,
or gameplay/world-state mutation has been added. The UI-visible timeout item remains open.

## Validation

Each boundary began with an expected focused compile failure. The combined focused host-local suite
passes 39 tests.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, a
zero-warning build, 1,388 Domain tests, 265 Application tests, 45 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox reproduces the known .NET IPC/VSTest denial; identical host-local commands
pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4c6: derive model-visible valid affordances from the exact validation snapshot**.

Acceptance boundary:

1. Add one pure Application projection from an exact `CharacterIntentionAffordanceSnapshot` to
   fresh `ValidAffordance` context entries in stable snapshot order.
2. Serialize only canonical intention type and target identifier text with an unambiguous bounded
   format. Prove exact value retention, distinct type/target pairs, empty input, null rejection,
   unchanged input, and no world-object description, display name, or unrelated state disclosure.
3. Replace the assembler's caller-supplied valid-affordance text entries with the exact affordance
   snapshot and derive the entries internally. Prove that the same snapshot can be passed to
   `CharacterTurnResponseValidationPolicy`, closing prompt/validation divergence without claiming
   the affordances were derived from live gameplay state.
4. Do not derive action eligibility from world state; add destination-bearing intentions; call a
   provider; wire Godot; stream presentation; execute partial or complete intentions; add settings,
   credentials, provider logging, persistence, or gameplay mutation.
