# Allrpcs Handover

## Current state

`main` includes merged PR #405 at `add9675`. Branch `codex/phase-8-map-runtime` completes the
remaining Phase 8 map persistence and runtime-integration boundary.

Schema version 6 is now the current supported save contract. Its canonical root retains the existing
discovery, world-event, character-memory, first-patient, and journal values plus one required
visibility-safe `map` object containing only `seekerId`, reported and verified location identifiers,
and ordered location/note-text pairs. Strict reading rejects null, missing, duplicate-property,
unknown-property, wrong-kind, oversized, noncanonical-identifier, blank-note, and oversized-note
documents. Exact version-1-through-5 JSON shapes remain unchanged.

`DeserializeCurrent` now executes every migration through `SaveSnapshotMapMigration`; versions 1
through 5 reach fresh empty same-Seeker map state, while direct version-6 documents retain populated
map values without sharing collections. Version 6 is enabled only after this complete load chain.

Scene capture now requires complete initialized same-Seeker journal, map, and note state, captures
both through their pure projections, and emits version 6. Scene restoration preflights first-patient,
journal, and map state before mutating controllers, then installs the fresh canonical catalogue, map,
and notes as one exact composition through an explicit restoration target. The concrete map
controller retains its established closed ordinary public surface.

Serialized-save and isolated-storage checks exercise populated report, verification, and note round
trips, fresh canonical rebinding, version-1 empty-map migration, unknown-location and hidden-note
rejection, and preservation after every rejected document. The ordinary provider-switch smoke now
initializes explicit map state before schema-6 capture. `README.md` records the current contract and
the Phase 8 map roadmap item is complete.

## Validation and review

- Test-first schema-6 persistence checks failed at the deliberate unsupported-version boundary, then
  passed after JSON, migration, and support-policy wiring.
- Focused map JSON tests cover canonical writing, independent round trips, versions 1 through 5,
  missing/unknown/duplicate properties, wrong kinds, malformed identifiers, bounded arrays, and
  blank or oversized notes.
- Focused restoration-target tests prove the exact fresh catalogue/map/note composition is installed
  and null targets fail closed.
- The restricted full suite passed all managed tests, then failed only at Godot's known
  `user://logs` sandbox boundary. The identical host-local run passed workflow validation,
  formatting, a zero-warning build, 2,824 Domain, 878 Application, 1,249 Infrastructure, and 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the graphical
  Compatibility launch.
- Session-wide review found and fixed one API-surface regression: restoration installation is now an
  explicit interface implementation rather than a sixth ordinary public map-controller method.
- All changed C# files are below 500 lines, changed C# lines are at most 120 characters, and
  `git diff --check` passes.

## Preserved boundaries

- Ordinary startup remains inert. No automatic visit capture, testimony reporting, map opening,
  location inference, or background activity was added.
- Persistence exposes no hidden location definitions or labels, coordinates, topology, testimony,
  scene objects, controllers, or UI state.
- No new map UI, investigation-route runtime, first-stabilization authoring, sage recovery flow,
  provider call, credential access, release, signing, or distribution work was added.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Add an asset-neutral, provider-neutral opening-region investigation-route catalogue only. Do not
activate routes in the scene yet.

Acceptance boundary:

1. Add one immutable route value with a canonical route key plus nonempty ordered location and
   evidence identifier lists. Copy inputs; reject null entries and duplicates within each list.
2. Add one immutable catalogue that retains routes in stable authored order, rejects duplicate route
   keys, and contains exactly these three ID-only representative routes:
   - stream/laundry: `stream`, `abandoned-laundry`; evidence `residue`;
   - shrine: `old-shrine`; evidence `partial-inscription`;
   - road/wagon: `southern-road`, `abandoned-wagon`; evidence `tool-gated-clue`.
3. Add a pure validation policy against one exact
   `RepresentativeOpeningRegionMapLocationSource` catalogue and one exact
   `RepresentativeDiscoveryScenarioSource` scenario. Every retained location/evidence identifier
   must resolve exactly once; routes must remain distinct; validation must not mutate either source.
4. Keep route values ID-only: no labels, descriptions, coordinates, topology, testimony text,
   background requirement, provider data, UI state, controller, or scene node.
5. Do not start discovery, reveal evidence, write the journal/map, infer route completion, change
   first-patient state, or tick the multiple-investigation-routes roadmap item in this slice.
6. Work test-first, run focused tests and `./scripts/quality.sh`, review the complete diff, update
   `README.md`, and leave the next exact slice for explicit scene-owned route progress only.
