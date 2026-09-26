# Allrpcs Handover

## Current state

`main` includes merged PR #399 at `2ce2ce5`. Branch
`codex/phase-8-journal-restoration` completes the Phase 8 journal through four bounded slices: pure
safe restoration, atomic scene installation, scene capture, and serialized/local-storage round trips.

`JournalSaveStateRestorationProjection` rejects null input, duplicate observation, hypothesis,
experiment, or open-question identities, and inconsistent prompts for a shared question identity
before creating output. Success returns a fresh complete safe `JournalSnapshot` using a fresh
canonical catalogue, exact section definitions, stable safe content order, the saved Seeker, and the
optional exact selected definition. It retains no authoritative world, dialogue audience, experiment
event, investigation aggregate, controller, UI, provider, or credential state.

Complete root restoration preflights that journal before any controller mutation, then installs its
fresh journal and optional selection with discovery, world-event, memory, and first-patient state.
Scene capture now requires a complete same-Seeker journal and emits fresh safe schema-version-5
content. Canonical JSON, legacy migration, and local storage preserve every safe content arm and the
selection. Ordinary startup remains inert; journal composition is still explicit through J or an
authorized caller. The Phase 8 journal roadmap item is complete.

No automatic journal source refresh, editable note, map, gameplay mutation, authoritative source
exposure, provider call, credential access, release, or signing work was added.

## Validation and review

- Test-first Application work observed the missing restoration types before implementation. Seven
  focused tests pass all five content arms, exact selection/no selection, independent immutable
  output, duplicate rejection, shared-question consistency, null rejection, and closed surfaces.
- Focused Godot checks pass root journal restoration/rejection preservation, schema-5 scene capture,
  canonical JSON/legacy migration, local storage, and the provider-switch save dependency.
- Session review covered every production/test/doc path, exact ownership, complete preflight,
  rejection preservation, stable ordering, closed surfaces, 120-character lines, sub-500-line C#
  files, and whitespace. It found and fixed the provider-switch smoke's missing explicit journal
  initialization and stale version-4 README/roadmap claims.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 826 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed every pure check before the known Godot `user://logs` restriction.

## Exact next bounded slice

Add one pure asset-neutral opening-region map-location foundation without scene controllers,
presentation, input, or persistence.

Acceptance boundary:

1. Define one immutable canonical location catalogue for the already-authored opening-region
   landmarks only, with stable identifiers, bounded player-facing labels, and stable authored order.
2. Define one immutable Seeker-owned map snapshot that distinguishes reported from verified
   canonical locations without exposing world objects, positions, navigation topology, or hidden
   locations.
3. Require exact catalogue definitions, unique location identity, and disjoint reported/verified
   membership; reject null, rebuilt, foreign, duplicate, and cross-owner input without partial output.
4. Prove empty, reported, verified, promotion, stable-order, independent immutable, rejection, and
   closed-surface behavior with focused Application tests.
5. Do not add map UI, scene ownership, coordinates, automatic discovery/testimony projection,
   editable notes, persistence/schema changes, gameplay mutation, providers, or credentials.
6. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for projecting reported locations from already-safe heard testimony.
