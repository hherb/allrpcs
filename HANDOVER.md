# Allrpcs Handover

## Current state

Phases 2m4–2m8 are implemented and reviewed without findings on
`codex/phase-2m4-plus-passive-noticing`, based directly on PR #120's merge commit `0bdc266`.

The new pure Domain chain provides exact complete-candidate lookup, candidate-to-perception
applicability, `Unnoticed`-only change detection, immutable application to `Noticed`, and separate
candidate-consumption eligibility. Lookup and consumption match both observer and target by value
while returning or recognizing the exact retained candidate. Applicability requires a value-equal
perception owner and an exact target lookup but deliberately ignores visibility. Change detection
then permits only `Unnoticed`; application replaces only that target at its stable position while
retaining the perception-owned character and object identifiers and every unrelated entry.

Each slice was test-first: its focused suite failed with `CS0103`/`CS0246` while the new type was
absent, then passed after the minimal implementation. After the final documentation and code
review, host-shell `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 727 Domain tests, 107 Application tests, six architecture tests, all fourteen Godot smokes,
and the bounded graphical launch.

README and ROADMAP record Phases 2m4–2m8. Passive noticing remains incomplete: candidates are not
produced, removed, resolved, selected, or connected to world-object authority or runtime wiring.
No Application or Godot behavior changed.

## Exact next bounded slice

Take **Phase 2m9: passive-noticing candidate consumption transition**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.PassiveNoticingCandidateConsumptionTransition.Consume` operation
   accepting an existing `PassiveNoticingCandidate` and `PassiveNoticingCandidateSnapshot` and
   returning a new candidate snapshot.
2. Delegate complete eligibility to `PassiveNoticingCandidateConsumptionPolicy`; reject a null,
   absent, or partially matching candidate without changing either input.
3. Remove only the value-equal complete retained candidate, preserving stable order and the exact
   instances and nested identifier instances of every unrelated candidate. Accept a distinct
   value-equal candidate argument and leave the original snapshot and every input unchanged.
4. Add focused Domain tests first for empty and absent rejection, both partial-match rejections,
   exact and distinct value-equal candidates, removal from each stable position, exact unrelated
   retention, null boundaries, unchanged inputs, and the minimal one-method static public surface.
5. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not apply or change perception, combine application and consumption, define resolution,
  produce, select, rank, reorder, or evaluate candidates, or inspect world objects, complete
  characters, locations, coordinates, distance, line of sight, environmental state, capabilities,
  inventory, focus, input, or time.
- Do not grant discovery, create knowledge or evidence, or add Application/Godot sources,
  controllers, runtime wiring, events, persistence, serialization, providers, plugins,
  credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
