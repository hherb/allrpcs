# Allrpcs Handover

## Current state

`main` contains merged PR #202 at `365f937`. The current branch is
`codex/phase-7-hermit-roles`.

This branch completes the first five Phase 7 roadmap items:

- `RepresentativeHermitCharacterSource` creates exactly two fresh read-only, stable-order
  LLM-controlled hermits with distinct private backgrounds, knowledge, beliefs, and goals;
- `RepresentativeSageCharacterSource` creates a bounded evidence-comparison and
  assumption-challenging consultant with explicit non-omniscience limits;
- `RepresentativeEnemyAgentCharacterSource` creates a covert role with a private national
  objective, bounded capabilities and limits, competing goals, and a possible allegiance-change
  belief without adding allegiance mechanics;
- `RepresentativeInfectedCharacterSource` creates exactly two fresh stable-order dormant roles
  with distinct latent state and no disease or wake-up mechanics;
- `CharacterTurnSpeakerSchedulingPolicy` places value-equal directly addressed eligible speakers
  first, preserves stable eligible order and exact eligible instances, and returns an independent
  read-only schedule.

The branch does not add scene nodes, provider calls, generation, dialogue participation decisions,
interjections, silence policy, multi-turn loops, autonomous actions, sabotage, disease or controller
transitions, UI, persistence, Phase 8 work, release, signing, or distribution.

Draft PR #203 contains this work.

## Validation and review

- final host-local `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,951 Domain, 402 Application, 447 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all credential-free Godot smokes and the bounded Compatibility-renderer launch passed
- focused tests passed for the hermits (3), sage (2), enemy agent (3), infected roles (3), and
  speaker scheduling (5)
- test-first missing-source failures were observed before each implementation
- `git diff --check`, practical 500-line/120-character limits, exact-identity, stable-order,
  immutable-collection, bounded-input, public/private-state, declarative-role, network-free, and
  scope review passed

The restricted full quality run reproduced the known VSTest localhost-bind restriction after its
zero-warning build; the identical host-local command passed. Hosted jobs for recent PRs still had
`steps: []` under the Actions account-budget condition, so do not retry or dispatch them unless
that condition changes.

## Exact next bounded slice

Take **Phase 7f: pure character-turn participation decisions**.

Acceptance boundary:

1. Add one closed canonical participation kind with exactly `respond`, `interject`, and
   `remain-silent` singleton values in that stable order.
2. Add one immutable decision retaining an exact scheduled `CharacterId`, exact participation
   kind, and an optional exact existing `CharacterGoal` motive. Require a motive for `interject`
   and `remain-silent`; require no motive for `respond`.
3. Add focused Domain tests for canonical parsing and identity, exact-instance retention,
   invalid kind/motive combinations, null rejection, and the smallest public API surface.
4. Do not select decisions, inspect text, invoke models, schedule or start turns, integrate scenes,
   alter the existing cancellation path, add interjection frequency or loop bounds, perform
   actions, mutate goals, or add UI/persistence/provider behavior.

Leave the combined roadmap item for motivated interjections, meaningful silence, and player
interruption unticked until the later selection and ordinary-flow integration are complete.
