# Allrpcs Handover

## Current state

`main` includes merged PR #410 at `29d4026`. Branch
`codex/phase-8-old-shrine-route-evidence` adds two bounded Phase 8 slices:

- The ordinary old-shrine I-key discovery submits only its exact committed
  `partial-inscription` evidence identity and exact filtered Seeker evidence view to the existing
  catalogue-routing coordinator when route progress is already active. Missing progress preserves
  the successful discovery without initializing routes, and repeated discovery cannot route twice.
- The first successful explicit M-key map open initializes the three representative investigation
  routes once from the map controller's exact catalogue and the old-shrine scene's exact discovery
  scenario. Startup, blocked modal input, released/unrelated input, and later map opens remain inert.

The exact old-shrine route can now complete through ordinary map verification plus discovery after
the player has opened the map. Other evidence sources still do not submit route observations, no
completion is consumed or presented, and the multiple-route roadmap item remains unticked.

## Validation and review

- Both slices were test-first. The new active-route old-shrine mode failed before the discovery
  coordinator was wired; the map-input smoke then failed because explicit map opening did not
  initialize route progress. Both passed after the minimum implementations.
- The old-shrine smoke now runs in two fresh processes. One proves the existing discovery remains
  successful with missing progress. The other pre-records the exact shrine location, proves the
  exact filtered evidence is recorded once, retains unrelated routes empty, and observes one exact
  completion token without consuming it.
- The map-input smoke proves inert startup and modal rejection, exact one-shot route creation from
  the live map/discovery sources, empty authored route order, exact identifier provenance, retained
  progress across map edits and reopens, and no accepted completion.
- Session review found and fixed a rejected-map-open ordering issue by delaying route mutation until
  the existing map and presentation preflights pass. It also strengthened repeated shrine discovery
  checks to retain exact post-success route and completion references, and extracted a focused smoke
  verifier so the main old-shrine smoke remains below 500 lines.
- The restricted complete suite passed all managed checks and stopped only at Godot's known
  `user://logs` sandbox boundary. The identical host-local `./scripts/quality.sh` passed formatting,
  a zero-warning build, 2,824 Domain, 898 Application, 1,249 Infrastructure, and 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the graphical Compatibility
  launch.
- Final review checked the complete diff, exact-reference ownership, missing-progress behavior,
  one-shot routing/initialization, rejected-input preservation, closed public surfaces, file-size
  and 120-character limits, formatting, and whitespace. No issue remained.
- Initial hosted PR run `36286879474` failed all five jobs before execution with `steps: []`. This is
  Actions/account infrastructure evidence, not hosted product validation; the complete host-local
  suite above remains the current product evidence.

## Preserved boundaries

- Route initialization happens only after the first successful explicit map-open input. Ordinary
  startup does not initialize or advance routes, and opening the map does not itself record a clue.
- The initialized routes use the exact live map catalogue and the exact discovery scenario retained
  by the old-shrine scene. Rebuilt or foreign route graphs are not substituted.
- Old-shrine routing occurs only after its existing discovery graph, identified presentation, and
  filtered evidence exposure commit atomically. Routing failure never rolls back or fabricates that
  already-successful discovery.
- Focused search, passive noticing, testimony, and every evidence source except the old-shrine
  layered inscription remain unwired. No preferred route or clue order is inferred.
- Accepted completion tokens remain exact existing provenance. This branch does not consume or
  present them, trigger testimony/stabilization, or add another gameplay effect.
- Save schema 6 remains unchanged. Route catalogue/progress, accepted completions, and consumed
  completion are not persisted and must not be described as surviving save/restore.
- Discovery, journal, first-patient, provider, environmental, and background-route owners remain
  unchanged. Do not tick the multiple-investigation-routes roadmap item yet.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Persist the complete initialized investigation-route lifecycle through a new save schema 7. Do not
add another evidence source or gameplay effect in the same slice.

Acceptance boundary:

1. Add bounded ID-only save state for the exact initialized route progress, ordered accepted
   completion route keys, and optional consumed completion route key. Do not serialize labels,
   descriptions, map coordinates, discovery text, or object graphs.
2. Capture only a complete internally consistent lifecycle: every progress entry belongs to the
   exact current catalogue in authored order; every recorded location/evidence identity is an exact
   route member; accepted completions bind exact complete current/earlier snapshots; and consumption
   binds one exact accepted token. Fail closed before producing a document.
3. During restoration, create the canonical route catalogue from the restored map catalogue and the
   exact scene discovery scenario, rebind every saved identifier/key exactly once, rebuild progress,
   accepted completions, and optional consumption, and preflight the whole graph before mutating any
   scene controller.
4. Migrate schemas 1-6 to canonical empty route progress with no accepted or consumed completion.
   Preserve all existing discovery, event, memory, patient, journal, and map migrations.
5. Prove JSON and isolated-storage round trips, malformed/unknown/duplicate/stale route rejection,
   complete and partial progress, accepted-but-unconsumed and consumed states, repeated restoration,
   and unchanged controllers after rejection.
6. Keep ordinary startup and all evidence sources unchanged. Do not consume/present a completion,
   add testimony or stabilization, infer clue order, expose route UI, or add provider activity.
7. Work test-first, run focused persistence/controller tests and `./scripts/quality.sh`, review the
   complete diff, update `README.md`, and leave one next slice for the canonical stream-residue
   passive-noticing submission path.
