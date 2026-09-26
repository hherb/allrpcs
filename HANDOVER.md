# Allrpcs Handover

## Current state

`main` includes merged PR #403 at `5f7c752`. Branch
`codex/phase-8-map-visits-testimony` advances the open Phase 8 map roadmap item through two bounded
scene flows: exact live opening-region visit verification and explicit safe testimony-to-location
reporting.

`OpeningRegionMapVisitVerificationCoordinator` accepts only the map controller's exact current map,
the current scene Seeker's exact identifier and position, the scene source's exact opening-region
projection, one exact retained anchor, and its matching canonical map location. It delegates
proximity to `OpeningRegionLocationVisitProjection` with a named 24-pixel radius and passes only the
coordinate-free visit token to `SeekerMapController.TryVerifyVisit`. It never scans anchors or runs
during ordinary movement.

`SeekerMapTestimonyReportingCoordinator` accepts only the exact current complete safe journal, its
exact testimony section and retained authored copy, the current scene Seeker, the map controller's
exact current map, and one caller-authored canonical location. It delegates to
`SeekerMapController.TryReport` without classifying testimony text, speaker, or delivery mode.

Both coordinators are inert during ordinary startup. Their switch-gated smokes are the only callers.
Accepted visits promote reported locations or add hidden verified locations, and accepted reports
add hidden reported locations, always in canonical order with immutable prior snapshots preserved.
Stale, rebuilt, foreign, wrong-owner, wrong-location, non-finite, stale-position, out-of-range,
already-applied, repeated, and null paths fail closed without changing map notes, presentation,
panels, journal, perception, world objects, or other gameplay state.

No automatic visit polling, automatic testimony reporting, player-visible reporting choice,
coordinate or topology presentation, map persistence/schema change, provider call, credential
access, release, or signing work was added. The Phase 8 map roadmap item remains open.

## Validation and review

- Test-first work observed each missing coordinator before implementation. The two new switch-gated
  smokes cover inert startup; exact current map, Seeker, position, projection, anchor, journal,
  testimony, entry, catalogue, and location provenance; reported-to-verified promotion;
  hidden-to-verified addition; content-neutral hidden-to-reported additions; canonical order;
  immutable sources; and rejection/isolation paths.
- Session review covered every changed coordinator, smoke, scene, script, README, and handover path.
  It found and fixed one coverage gap: the visit smoke now distinguishes a stale supplied position
  from an exact current but out-of-range scene position.
- Every changed C# file is below 500 lines and every changed C# line is at most 120 characters.
  `dotnet format Allrpcs.sln --no-restore --verify-no-changes` and `git diff --check` passed.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 864 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted sandbox cannot create Godot's `user://logs`; the identical focused and full host runs
  passed.

## Exact next bounded slice

Add visibility-safe map save-state contracts and pure capture/restoration projections without root
save-schema or scene-controller wiring.

Acceptance boundary:

1. Add one map save-state value containing only the canonical Seeker identifier, reported location
   identifiers, verified location identifiers, and canonical location/note-text pairs.
2. Add a pure capture projection from exact same-Seeker `SeekerMapSnapshot` and
   `SeekerMapNoteSnapshot`; preserve canonical order and expose no hidden catalogue entries,
   coordinates, topology, testimony, or scene objects.
3. Add a pure restoration projection that creates a fresh representative catalogue plus fresh map
   and note snapshots from valid state. Reject unknown, duplicate, overlapping reported/verified,
   hidden-note, duplicate-note, invalid-text, and inconsistent-owner state atomically.
4. Prove empty and populated round trips, reported/verified/note ordering, exact fresh canonical
   definitions, immutable inputs, and all malformed-state rejection with focused Application tests.
5. Do not change the root save DTO, schema version, JSON, storage, scene controllers, startup,
   automatic visit/report behavior, UI, providers, or credentials.
6. Run the focused tests and `./scripts/quality.sh`, review the complete diff, update `README.md`, and
   leave one next slice for root schema integration and migration behavior.
