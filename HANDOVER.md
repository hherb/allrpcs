# Allrpcs Handover

## Current state

`main` includes merged PR #411 at `bd26a25`. Branch
`codex/phase-8-route-lifecycle-persistence` adds the bounded Phase 8 save-schema 7 slice:

- Schema 7 retains only canonical route keys, ordered recorded location/evidence IDs, ordered
  accepted-completion route keys, and one optional consumed-completion route key. Labels,
  descriptions, coordinates, discovery text, and runtime object graphs remain absent.
- Capture requires complete exact current-catalogue progress in authored order, unique exact route
  members, monotonic exact completion provenance, and optional consumption of one exact accepted
  token. It also binds the route graph to the exact live map catalogue and shrine discovery scenario.
- Restoration first rebuilds the canonical routes from the freshly restored map catalogue and exact
  shrine scenario, then rebinds progress, accepted completions, and optional consumption before any
  controller mutation. Rejection leaves every controller unchanged.
- Schemas 1-6 migrate to three canonical empty route entries with no accepted or consumed
  completion, while retaining all existing discovery, event, memory, patient, journal, and map
  migrations.

The complete initialized investigation-route lifecycle now survives JSON and isolated-storage
round trips. Ordinary startup, evidence sources, completion presentation/consumption, testimony,
stabilization, provider behavior, and the roadmap checkbox remain unchanged.

## Validation and review

- Tests cover schema-7 construction/support, schema-6 migration, complete and partial progress,
  accepted-but-unconsumed and consumed state, fresh canonical reconstruction, JSON round trips,
  malformed/unknown/duplicate/divergent provenance rejection, and exact controller preservation.
- The restricted `./scripts/quality.sh` passed workflow validation, formatting, zero-warning build,
  2,827 Domain, 905 Application, 1,255 Infrastructure, and 8 architecture tests plus scenario
  validation, then stopped at Godot's known `user://logs` sandbox boundary.
- The identical host-local `./scripts/quality.sh` passed those checks, every credential-free Godot
  smoke (including route persistence, local storage, and provider-switch save/restore), and the
  graphical Compatibility launch.
- After adding the explicit accepted-but-unconsumed case and pruning this handover, the final
  zero-warning build, formatting verification, whitespace check, and all .NET suites passed at
  2,827 Domain, 906 Application, 1,255 Infrastructure, and 8 architecture tests. Product source was
  unchanged from the complete host-local Godot run except for smoke success-message wording.
- Session review checked the complete diff, exact-reference ownership, atomic preflight/install,
  migration freshness, closed JSON shapes, ID-only data, file-size/120-character limits,
  formatting, and whitespace. It found and fixed divergent completion-lineage admission by
  requiring every accepted completion snapshot to be an exact monotonic prefix of the next/current
  snapshot. No issue remained.

## Preserved boundaries

- Route initialization still occurs only after explicit map open or save restoration. Ordinary
  startup remains inert.
- No new evidence source is routed. Existing old-shrine routing remains the only ordinary-play
  evidence submission path.
- No completion is newly consumed or presented, and no testimony, stabilization, clue-order,
  objective, UI, provider, or gameplay effect is added.
- Save files remain local, strict, credential-free, and fail closed. No provider state, credentials,
  hidden reasoning, labels, narrative text, coordinates, or object graph is serialized.
- Do not tick the multiple-investigation-routes roadmap item yet.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Submit the canonical stream `residue` evidence through the existing ordinary passive-noticing path
to the existing investigation-route observation-routing coordinator. Do not add focused-search,
testimony, wagon-clue, completion presentation, or stabilization behavior in the same slice.

Acceptance boundary:

1. Bind the existing representative discovery scenario's exact `stream-residue` world object,
   exact `residue` evidence identity, exact Seeker passive-noticing candidate, and exact filtered
   Seeker evidence view; do not author duplicate evidence or rebuild identifiers.
2. Route only after the existing passive-noticing resolution has atomically advanced that exact
   object from unnoticed to noticed and the evidence projection confirms visibility. Missing route
   progress must preserve the successful noticing without initializing routes.
3. When routes are initialized, submit the exact current progress, filtered view, and evidence ID to
   `OpeningRegionInvestigationRouteObservationRoutingCoordinator` once. Let its existing authored
   catalogue order update every matching route and observe any exact resulting completion.
4. Repeated, stale, rebuilt, foreign, unseen, wrong-character, unresolved, and non-member inputs
   remain inert without rolling back noticing, changing unrelated routes, consuming a completion,
   or adding another presentation/effect.
5. Keep ordinary startup inert and preserve save schema 7, map-open initialization, old-shrine
   routing, journal/map state, providers, UI, testimony, stabilization, and clue-order neutrality.
6. Work test-first, add a switch-gated ordinary-scene smoke for missing-progress and active-route
   modes, run focused checks and `./scripts/quality.sh`, review the complete diff, update `README.md`,
   and leave one exact next slice for the canonical wagon tool-gated-clue submission path.
