# Allrpcs Handover

## Current state

Phase 2m3 is implemented on `codex/phase-2m3-passive-noticing-candidate-projection`, based directly
on PR #119's merge commit `d9cc4a1`.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateProjection.ForCharacter` is one pure static
operation accepting an existing `CharacterId` and `PassiveNoticingCandidateSnapshot`. It returns a
new snapshot containing exactly the candidates whose observer identifier is value-equal to that
character identifier. It permits no matches and multiple distinct matches, preserves stable source
order, retains the exact matching candidates and their nested identifier instances, omits unrelated
candidates, rejects null inputs, and leaves both inputs unchanged.

Test-first evidence failed with `CS0103`/`CS0246` while the projection type was absent, then all seven
focused tests passed. After code and documentation review, host-shell `./scripts/quality.sh` passed
workflow and formatting validation, a zero-warning build, 682 Domain tests, 107 Application tests,
six architecture tests, all fourteen Godot smokes, and the bounded graphical launch.

README and ROADMAP record the projection boundary. Passive noticing remains incomplete: there is no
candidate production, lookup, world-state inspection, selection, eligibility, outcome, transition,
perception change, discovery grant, or runtime wiring.

## Exact next bounded slice

Take **Phase 2m4: exact passive-noticing candidate lookup**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.PassiveNoticingCandidateLookup.Find` operation
   accepting an existing `PassiveNoticingCandidate` and `PassiveNoticingCandidateSnapshot` and
   returning the exact value-equal retained candidate, or `null` when none exists.
2. Match the complete candidate by value, including both observer character and target object.
   Reject same-observer/different-target and different-observer/same-target candidates, ignore
   unrelated entries, and rely on the snapshot's duplicate-candidate invariant.
3. Preserve the exact retained candidate and nested identifier instances; reject either null input
   and leave every input unchanged.
4. Add focused tests first for empty and unrelated snapshots, a distinct value-equal candidate,
   both partial mismatch cases, an exact match among surrounding unrelated entries, exact retention,
   null boundaries, unchanged inputs, and the minimal one-method static public surface.
5. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not produce, discover, select, rank, reorder, or evaluate candidates; inspect world objects,
  complete characters, locations, coordinates, distance, line of sight, environmental state,
  capabilities, inventory, focus, input, or time; or define eligibility, outcomes, or transitions.
- Do not change perception or visibility, grant discovery, create knowledge or evidence, or add
  Application/Godot sources, controllers, runtime wiring, events, persistence, serialization,
  providers, plugins, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
