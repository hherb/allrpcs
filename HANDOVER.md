# Allrpcs Handover

## Current state

Phase 2m1 is implemented on `codex/phase-2m1-passive-noticing-candidate`, based directly on PR
#117's merge commit `2da2a28`.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidate` is one sealed immutable record containing only
an existing `CharacterId ObserverCharacterId` and `ObjectId TargetObjectId`. It rejects either null
input, retains the exact supplied identifier instances, and adds no custom equality or behavior
beyond the identifiers' existing value semantics.

Test-first evidence failed while the type was absent, then five focused tests passed for exact
nested-instance retention, distinct value-equal identifiers, different observer and target values,
both null boundaries, unchanged inputs, and the minimal two-property surface. After code review,
host-shell `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build,
666 Domain tests, 107 Application tests, six architecture tests, all fourteen Godot smokes, and the
bounded graphical launch.

README and ROADMAP record the new identifier-only boundary. The roadmap's passive-noticing item
remains incomplete: there is still no candidate collection, production, selection, eligibility,
outcome, transition, perception change, discovery grant, or runtime wiring.

## Exact next bounded slice

Take **Phase 2m2: immutable passive-noticing candidate snapshot**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.Discovery.PassiveNoticingCandidateSnapshot` that copies
   one supplied `IEnumerable<PassiveNoticingCandidate>` and exposes the entries in stable supplied
   order through one get-only `IReadOnlyList<PassiveNoticingCandidate> Entries` property.
2. Permit an empty sequence, retain the exact supplied candidate instances, and reject a null
   sequence, null entries, and duplicate candidates by their existing value semantics. Permit the
   same observer with different targets and the same target with different observers.
3. Add focused tests first for empty input, stable order and exact retention, caller-collection
   copying, value-equal duplicate rejection and permitted non-duplicates, null boundaries, input
   preservation, read-only behavior, and the minimal public API surface.
4. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not produce, discover, select, filter, rank, reorder, or look up candidates; inspect world
  objects, characters, locations, coordinates, distance, line of sight, environmental state,
  capabilities, inventory, focus, input, or time; or define eligibility, outcomes, or transitions.
- Do not change perception or visibility, grant discovery, create knowledge or evidence, or add
  Application/Godot sources, controllers, runtime wiring, events, persistence, serialization,
  providers, plugins, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
