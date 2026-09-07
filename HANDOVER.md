# Allrpcs Handover

## Current state

Phases 2m15–2m18 are implemented and reviewed without findings on
`codex/phase-2m15-plus-passive-noticing`, based directly on PR #122's merge commit `7ff6967`.

The pure Domain passive-noticing chain now binds candidate recording to an existing authoritative
character and world object, then deterministically selects and atomically resolves the next
eligible retained candidate for one observer. Production requires value-equal candidate, character,
and world-object identities and delegates duplicate-free exact-instance append behavior to the
existing recording boundary. Selection preserves stable candidate order and returns the exact first
candidate for a value-equal observer that can change perception and be consumed. Resolve-next
delegates that selection to the existing atomic transition, returning snapshots in which only the
selected target changes from `Unnoticed` to `Noticed` and only that complete candidate is removed.
Distinct value-equal identifiers are accepted, while mismatches, duplicates, absent targets,
already-noticed targets, unrelated observers, and nulls are rejected without changing any input.

All four slices were test-first: each focused suite failed with `CS0103`/`CS0246` while its new type
was absent and passed after the minimal implementation. The final host-shell
`./scripts/quality.sh` passed workflow and formatting validation, a zero-warning build, 801 Domain
tests, 107 Application tests, six architecture tests, all fourteen Godot smokes, and the bounded
graphical launch. README and ROADMAP record Phases 2m15–2m18. Passive noticing remains incomplete:
no authoritative Character exists in the Godot scene, and no runtime owner yet retains candidate
state or invokes production, selection, or resolution. No Application or Godot behavior changed.

## Exact next bounded slice

Take **Phase 2m19: temporary authoritative Seeker character source**.

Acceptance boundary:

1. Add one Godot `Allrpcs.Game.TemporarySeekerCharacterSource` node that creates and retains exactly
   one immutable `Character` for the existing scene Seeker during `_Ready`.
2. Obtain the Seeker identifier from the already-ready `FocusedSearchActivityController` and retain
   that exact `CharacterId` instance in both `CharacterIdentity` and its matching human
   `CharacterControllerAssignment`. Supply only explicitly temporary minimal authored components
   needed by the existing complete `Character` aggregate; do not derive passive-noticing rules from
   those components.
3. Insert the source after the Seeker/controller and before any future passive-noticing consumer in
   `Main.tscn`. Add one opt-in credential-free smoke proving exact source/identifier/component
   retention and unchanged focused-search, perception, input, and presentation state.
4. Add the smoke to the existing headless validation script, update README and ROADMAP evidence, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not produce, record, select, resolve, or consume a passive-noticing candidate at runtime; do not
  change perception, focus, movement, input, or presentation; and do not create a second live
  perception owner.
- Do not infer capabilities, inventory, conditions, knowledge, beliefs, relationships, goals,
  distance, line of sight, environmental state, timing, or discovery from the temporary character.
- Do not add Application controllers, events, persistence, serialization, providers, plugins,
  credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
