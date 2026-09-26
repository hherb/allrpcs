# Allrpcs Handover

## Current state

`main` includes merged PR #406 at `1362836`. Branch
`codex/phase-8-investigation-route-catalogue` adds two bounded Phase 8 slices: the representative
opening-region investigation-route catalogue and inert explicit scene-owned route progress.

`OpeningRegionInvestigationRoute` retains one canonical key plus nonempty ordered location and
evidence identifier lists. It copies caller collections, retains exact identifier instances, and
rejects null or value-duplicate entries. `OpeningRegionInvestigationRouteCatalogue` requires the
complete canonical order `stream-laundry`, `shrine`, then `road-wagon`; no empty, partial,
reordered, duplicate-key, or unknown-key catalogue is constructible.

`RepresentativeOpeningRegionInvestigationRouteSource` binds three ID-only routes to exact
identifiers from one supplied representative map catalogue and discovery scenario:

- stream/laundry: `stream`, `abandoned-laundry`; evidence `residue`;
- shrine: `old-shrine`; evidence `partial-inscription`;
- road/wagon: `southern-road`, `abandoned-wagon`; evidence `tool-gated-clue`.

The pure validation policy requires every retained identifier to resolve by exact reference once
in those supplied sources and rejects repeated route ID shapes without changing either source.

`OpeningRegionInvestigationRouteProgressTransition` creates one empty entry per exact route and
records only an exact unrecorded member location or evidence identifier. Successful changes return
fresh bounded snapshots, retain the exact catalogue and route, and preserve unrelated entries by
reference. They do not retain history or infer completion. The scene controller remains inert at
ordinary startup; explicit initialization validates one exact catalogue, and explicit location or
evidence recording uses expected-state compare-and-swap. It does not read or change map, journal,
discovery, first-patient, provider, presentation, or UI state.

`README.md` documents both slices. The multiple-investigation-routes roadmap item remains unticked
because authoritative map/discovery state is not yet bound into route progress and no route can yet
be exercised through ordinary play.

## Validation and review

- Test-first focused checks failed at the absent route and progress contracts, then passed after
  implementation. Eleven focused Application tests cover exact authored content, defensive and
  read-only collections, canonical completeness/order, exact-source validation, duplicate route
  shapes, immutable progress, member-only recording, unrelated-entry retention, and null, unknown,
  foreign, rebuilt, repeated, and stale rejection.
- The restricted `./scripts/quality.sh` run passed all managed tests and scenario validation, then
  stopped only at Godot's known `user://logs` sandbox boundary. The identical host-local suite
  passed workflow validation, formatting, a zero-warning build, 2,824 Domain, 889 Application,
  1,249 Infrastructure, and 8 architecture tests, scenario validation, every credential-free
  Godot smoke, and the graphical Compatibility launch.
- The focused route-progress Godot smoke proves inert startup, exact one-shot initialization,
  explicit compare-and-swap recording, closed public surface, invalid/rebuilt/foreign/stale/repeated
  rejection, clean shutdown, and map/journal isolation.
- Session-wide review found and fixed two issues: the specialized catalogue now rejects empty or
  partial shapes and enforces all three canonical keys in order; progress snapshots no longer retain
  an unbounded predecessor chain. Focused build, tests, formatting, whitespace, and Godot smoke pass
  after those fixes.
- All changed C# files are below 500 lines, changed C# lines are at most 120 characters, and
  `git diff --check` passes.

## Preserved boundaries

- Ordinary startup does not initialize or advance investigation-route progress.
- No route completion, clue-order prescription, automatic visit capture, testimony classification,
  evidence reveal, discovery start, map/journal write, first-patient change, provider call, UI,
  coordinate, topology, asset, or background requirement was added.
- Save schema 6 remains unchanged; route definitions and progress are not persisted.
- Do not tick the multiple-investigation-routes roadmap item until the routes are authoritatively
  exercisable and its acceptance is actually met.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Bind explicit authoritative map and discovery state to the existing pure route-progress transition.
Do not add scene wiring or infer route completion yet.

Acceptance boundary:

1. Add one pure verified-location progress operation accepting an exact current progress snapshot,
   exact route, exact current `SeekerMapSnapshot`, and exact `MapLocationDefinition`. Record only
   when the definition is in the map's exact verified locations and its exact identifier is retained
   by the route; otherwise fail closed without changing either input.
2. Add one pure visible-evidence progress operation accepting an exact current progress snapshot,
   exact route, exact `CharacterEvidenceViewSnapshot`, and exact route evidence identifier. Record
   only when that exact identifier is retained by the filtered character view and the route;
   otherwise fail closed without changing either input.
3. Delegate successful changes to `OpeningRegionInvestigationRouteProgressTransition`; preserve
   exact catalogue/route provenance, stable record order, and unrelated entry instances.
4. Prove null, stale-progress member, rebuilt, foreign, unverified/unseen, nonmember, repeated, and
   mismatched-source paths remain inert. Do not pool partial routes or expose a completion Boolean.
5. Do not change `OpeningRegionInvestigationRouteProgressController`, `Main.tscn`, save schema,
   journal/map/discovery owners, input, UI, providers, first-patient state, or the roadmap checkbox in
   this slice.
6. Work test-first, run focused tests and `./scripts/quality.sh`, review the complete diff, update
   `README.md`, and leave one next slice for an explicit scene coordinator over these accepted
   authority bindings only.
