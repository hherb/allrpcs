# Allrpcs Handover

## Current state

PR #20 merged Phase 2c9 into `origin/main` at merge commit `a90f9b0` on 2026-09-01.
The current branch, `codex/phase-2c10-character-aggregate`, is based directly on that verified
merge.

Phase 2c10 is implemented, reviewed, and locally validated. It completes the structural
character-model roadmap item by composing all existing character components into one immutable
Domain aggregate. No mutable state or runtime mechanics were added.

The private remote has open draft PR #21 for this branch. Reviewed slice commit `63b5fab` passed
all five jobs in hosted run `33499876492`. This handover-only evidence update creates a new branch
head, so re-query its final checks rather than assuming the earlier run applies. Hosted desktop
outputs are unsigned development evidence only; pull-request merge remains separately authorized.

At the start of the next session, verify the live branch, pull request, and checks before acting:

```sh
git status --short --branch
git log -4 --oneline --decorate
git fetch origin main
gh pr view codex/phase-2c10-character-aggregate --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c10-character-aggregate --repo hherb/allrpcs
```

## Phase 2c10 result

- `Character` retains exactly one existing identity, public background, private background,
  capability set, limitation set, controller assignment, condition set, inventory, knowledge,
  belief set, relationship set, and goal set.
- Every component is required. The controller assignment's `CharacterId` must equal the
  identity's `Id` by value; equivalent identifier instances are valid.
- The sealed aggregate exposes only twelve get-only component properties. It retains the
  supplied immutable instances and adds no other cross-field policy, methods, or events.
- Six focused tests cover exact composition, valid empty child values and collections, all twelve
  null cases, equal identifier values, mismatched identifiers, and the complete read-only public
  surface.
- `Domain` retains no project references. `README.md` documents the completed boundary and
  exclusions; `ROADMAP.md` now ticks the verified character-model item.

Review confirmed that every character value listed by the roadmap is composed and that the
aggregate exposes no mutation, lifecycle, event handling, runtime registration, relationship or
goal mechanics, capability authorization, behavior selection, filtered view, executable policy,
or engine integration. No correctness, architecture, or scope issue remained after review.

## Validation and limitations

On 2026-09-01, the focused test-first run failed to compile because `Character` did not exist.
After implementation, the identical host-shell run passed all six focused tests. The restricted
full sequence produced a zero-warning build, then its .NET test host could not bind local
transport (`SocketException (13): Permission denied`).

The identical host-shell `./scripts/quality.sh` sequence passed on Apple Silicon macOS with .NET
SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 178 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, location-correctness, or runtime-behavior claim. Releases, artifact
distribution, credentials, signing, notarization, repository visibility changes, and pull-request
merge remain unauthorized.

## Exact next bounded slice

After the user reports that the Phase 2c10 pull request merged, take **Phase 2d1: immutable
location identity and description boundary**. This starts the next roadmap item without defining
spatial, discovery, or runtime mechanics.

Acceptance boundary for Phase 2d1:

1. Add an immutable `LocationIdentity` Domain value that composes exactly one existing
   `LocationId` and one authored display name. Require a non-whitespace display name of at most 80
   UTF-16 code units and preserve valid text exactly without trimming or normalization.
2. Add a structurally separate immutable `LocationDescription` Domain value. Permit empty text,
   reject null and text over 4,000 UTF-16 code units, and preserve valid authored text exactly.
   Treat the description as authoritative authored state, not proof that a character can perceive
   or understand it.
3. Add focused pure tests for boundaries, exact text preservation, value equality, the deliberately
   limited public surfaces, and structural separation from character text and identifiers.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Keep the broader
   location/object/item/environmental-state roadmap item unticked.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2d1:

- Do not add a location aggregate, hierarchy, containment, connections, coordinates, regions,
  navigation topology, terrain, access rules, environmental state, or mutable occupancy.
- Do not add public/private or per-character filtered views, visibility states, perception,
  discovery, reinterpretation, focused search, or interaction behavior.
- Do not add object or item models, inventory mechanics, scenario content, persistence,
  serialization, database/save schemas, migrations, Godot resources or nodes, UI, providers, or
  plugin surfaces.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not change repository visibility, merge pull requests, create releases, distribute artifacts,
  or add credentials without explicit user authorization.

## Working conventions

- Start from the verified merged base and take only the exact bounded slice above.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Run `./scripts/quality.sh`, inspect all intended paths, and stage only the reviewed slice.
- Report native/manual, signing, release, and distribution evidence without overstating it.
