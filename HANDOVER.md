# Allrpcs Handover

## Current state

Phase 2m2 is implemented on `codex/phase-2m2-passive-noticing-candidate-snapshot`, based directly on
PR #118's merge commit `64e1d06`.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateSnapshot` is one sealed immutable snapshot that
copies a supplied candidate sequence once into a read-only list. It permits an empty sequence,
preserves stable supplied order and exact candidate instances, rejects a null sequence or entry,
and rejects value-equal duplicate candidates. Candidates may share only an observer or only a
target.

Test-first evidence failed while the snapshot type was absent, then nine focused tests passed for
the complete acceptance boundary and minimal public surface. After code and documentation review,
host-shell `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build,
675 Domain tests, 107 Application tests, six architecture tests, all fourteen Godot smokes, and the
bounded graphical launch.

README and ROADMAP record the snapshot boundary. Passive noticing remains incomplete: there is no
candidate production, world-state inspection, observer projection, selection, eligibility,
outcome, transition, perception change, discovery grant, or runtime wiring.

## Exact next bounded slice

Take **Phase 2m3: observer-specific passive-noticing candidate projection**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.PassiveNoticingCandidateProjection.ForCharacter`
   method accepting an existing `CharacterId` and `PassiveNoticingCandidateSnapshot` and returning
   a new snapshot containing exactly the candidates whose observer identifier is value-equal to
   that character identifier.
2. Permit no matches and multiple distinct matches. Preserve stable source order and retain the
   exact matching candidate instances; omit every unrelated candidate without changing either
   input or any retained identifier instance.
3. Add focused tests first for exact and distinct value-equal observer identifiers, no and multiple
   matches, unrelated omission, stable order and exact retention, null boundaries, unchanged
   inputs, and the minimal one-method static public surface.
4. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not produce, discover, select, rank, or reorder candidates; inspect target objects, complete
  characters, locations, coordinates, distance, line of sight, environmental state, capabilities,
  inventory, focus, input, or time; or define eligibility, outcomes, or transitions.
- Do not change perception or visibility, grant discovery, create knowledge or evidence, or add
  Application/Godot sources, controllers, runtime wiring, events, persistence, serialization,
  providers, plugins, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
