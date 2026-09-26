# Allrpcs Handover

## Current state

`main` includes merged PR #400 at `a631fdd`. Branch `codex/phase-8-opening-map` advances the open
Phase 8 map roadmap item through four bounded, pure Application slices: canonical map locations,
reported testimony locations, visit-backed verified locations, and player-authored notes.

`RepresentativeOpeningRegionMapLocationSource` derives a fresh asset-neutral catalogue from all
eleven already-authored representative landmarks. Definitions expose only their exact canonical
location identifier and bounded player-facing label in stable authored order. `SeekerMapSnapshot`
exposes only one exact Seeker plus independent canonical-order reported and verified definitions;
its complete catalogue stays internal so hidden locations are not exposed. Composition rejects
null, rebuilt, foreign, duplicate, overlapping, and cross-owner state. Explicit report and verify
transitions return fresh snapshots, and verification promotes a reported location when necessary.

`SeekerMapTestimonyReportProjection` accepts only an exact audience-free authored copy already in
the exact Seeker's safe testimony snapshot, then reports the exact canonical location explicitly
supplied by the caller. It never interprets text, speaker, or delivery mode. The visit route accepts
one exact retained opening-region anchor, finite Seeker position, and a positive radius strictly
below half the projection's minimum landmark separation. It emits a coordinate-free owner/location
token, and only exact token, projection, owner, catalogue, and location provenance can verify the
map.

Player notes remain a separate immutable `SeekerMapNoteSnapshot`. Set/replace accepts bounded
nonblank text only for an exact reported or verified location, remove accepts only an existing exact
note, and results retain canonical location order. Every retained note is revalidated against the
complete supplied map before another transition, preventing same-owner cross-lineage state from
preserving a note for a now-hidden location.

No scene controller, automatic testimony refresh or visit capture, map presentation/panel/input,
editable widget, persistence/schema change, gameplay mutation, provider call, credential access,
release, or signing work was added. The Phase 8 map roadmap item remains open.

## Validation and review

- Test-first Application work observed each missing map boundary before implementation. Thirty-two
  focused tests pass canonical catalogue/label/order, empty/reported/verified snapshots, promotion,
  testimony reporting without interpretation, bounded visit tokens, note set/replace/remove,
  immutability, public-surface closure, null/bounds, and exact-reference rejection.
- Session review covered every production/test/doc path, public data exposure, owner/catalogue/source
  provenance, stable ordering, immutable inputs, bounded text and visit distance, sub-500-line C#
  files, 120-character lines, formatting, and whitespace. It found and fixed ambiguous oversized
  visit radii and cross-lineage notes retained for hidden locations.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 858 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted pre-review run passed every pure check before the known Godot `user://logs`
  restriction.

## Exact next bounded slice

Add one scene-owned, in-memory map-state controller without presentation, input, or persistence.

Acceptance boundary:

1. Own one exact opening-region map catalogue, current `SeekerMapSnapshot`, and current
   `SeekerMapNoteSnapshot`; ordinary startup leaves all three uninitialized and inert.
2. Initialize all three atomically once for one exact Seeker, with an empty map and empty notes.
3. Add explicit compare-and-swap operations that delegate exact safe testimony reporting,
   coordinate-free visit verification, note set/replace, and note removal to the existing pure
   projections/transitions before atomically replacing only current state.
4. Reject null, uninitialized, stale, rebuilt, foreign, cross-owner, repeated, hidden-location, and
   failed downstream inputs without partial mutation.
5. Add the controller to `Main.tscn` in dependency order, but do not initialize or invoke it during
   ordinary startup. Prove inert startup and one switch-gated exact flow with a credential-free smoke.
6. Do not add a map panel, input actions, automatic testimony refresh, automatic visit capture,
   coordinate/topology presentation, persistence/schema changes, gameplay mutation, providers, or
   credentials.
7. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for a safe map presentation snapshot over already-owned in-memory state.
