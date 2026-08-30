# Allrpcs Handover

## Current state

PR #4 merged Phase 1b into `origin/main` at merge commit `927556e`. The current branch,
`feat/navigation-region-foundation`, is based directly on that merge.

Phase 1c, the greybox navigation-region foundation, is complete on the current branch. The
complete host-shell quality sequence passed and the user confirmed the native macOS behavior on
2026-08-31. The navigation-region and navigation-request-validation roadmap items are ticked;
the broader Phase 1 acceptance and automated physics-boundary item remain unticked.

The branch is intended to track the private remote and have an open draft pull request. It is
not authorized to merge. Public visibility, merging, releases, artifact distribution,
credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/navigation-region-foundation --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/navigation-region-foundation --repo hherb/allrpcs
```

The expected state is a clean branch tracking its remote with an open draft pull request and
current checks. Do not merge it without separate user authorization.

## Phase 1c implementation

The slice remains within its original boundary:

- `src/Application/Navigation/NavigationRequestValidator.cs` validates finite ordered world
  bounds plus finite origin and destination coordinates contained by those bounds. It returns an
  immutable request and has no Godot dependency.
- `tests/Application.Tests/NavigationRequestValidatorTests.cs` adds 23 focused cases for valid
  requests, inclusive world edges, invalid bounds, non-finite coordinates, and out-of-bounds
  origins and destinations. The Application suite now has 49 passing cases in total.
- `game/Main.tscn` adds an obvious temporary brown bridge and divides the stream collision into
  west and east polygons, leaving a 112-pixel crossing for the 36-pixel-diameter Seeker.
- `game/GreyboxNavigation.cs` deterministically authors one `NavigationRegion2D` from 98 shared
  vertices and 66 convex polygons. The surface is inset from the outer walls, follows the stream
  banks, connects through only the temporary bridge, and leaves conservative clearance around
  the village and three-tree cluster.
- `game/NavigationSmokeCheck.cs` waits for the asynchronous navigation map to synchronize, then
  uses the pure request validator and `NavigationServer2D` paths to prove a representative route
  across the bridge. It also proves that representative stream, tree, and village targets do not
  resolve to their requested endpoints.
- `scripts/smoke-godot.sh` enables that credential-free behavioral check and exits non-zero on a
  failed map, route, or obstacle assertion. Ordinary game launches do not run the assertions.
- `README.md` documents the crossing, navigation surface, automated check, project boundary, and
  explicit exclusions. `ROADMAP.md` records only the completed Phase 1c work.

## Review and validation

### Automated evidence

On 2026-08-31, the complete host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with
.NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `actionlint` accepted the hosted workflow and every shell script passed syntax validation;
- restore and `dotnet format --verify-no-changes` passed;
- all seven projects built with zero warnings and zero errors;
- the domain test passed;
- all 49 Application movement, camera-framing, and navigation-request cases passed;
- all six architecture reference cases passed;
- the headless navigation smoke synchronized a 98-vertex, 66-polygon region at navigation map
  iteration 2, found the representative bridge route, rejected stream/tree/village endpoints,
  and exited 0; and
- the bounded 60-frame native graphical launch exited 0 using the Compatibility renderer on the
  Apple M3 Max.

The restricted sandbox's headless launch could not open Godot's normal user log and crashed
inside log rotation before loading the project. The identical smoke passed in the host
environment, so this is recorded as a sandbox limitation rather than a product failure. The
visible native run produced no Godot error or warning entries in the application log.

### Native visual evidence

The game was launched in a visible native 960-by-540 macOS window. On 2026-08-31, the user
confirmed that:

1. WASD movement crosses the brown bridge in both directions;
2. the stream elsewhere, village, three-tree cluster, and outer border still block the Seeker;
   and
3. movement and camera following remain smooth.

The prior Phase 1b confirmation that ordinary movement never reveals space outside the outlined
1600-by-900 region remains applicable.

## Limitations and exclusions

- Phase 1c has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, arrow keys, gamepads, accessibility, performance, packaging,
  installation, signing, or distribution.
- The pure tests validate request bounds and coordinates but do not simulate Godot input,
  physics collision, or navigation-server topology. The credential-free Godot smoke validates
  representative topology; the current macOS manual run validates player collision.
- The navigation surface is deliberately conservative greybox geometry for future companion
  pathfinding. It is not a final world mesh, dynamic obstacle system, movement controller, or
  proof that every possible point pair has an authored route.
- The bridge, stream collision, village rectangle, tree circles, and navigation clearance are
  temporary primitives. There is no navigation-agent radius negotiation, rebaking, runtime
  obstacle update, debug overlay, bridge art, or path-connectivity claim beyond the representative
  checks above.
- No point-and-click movement, zoom/pan input, controller mappings, healer behavior, companion
  controller, LLM call, interaction highlighting, inventory, dialogue, disease, provider,
  persistence, final art, audio, networking, packaging, credential, release, or distribution path
  was added.

## Exact next bounded slice

First review the Phase 1c draft pull request and its current checks. Do not merge it without
explicit user authorization. After the user reports that pull request merged, take **Phase 1d:
deterministic healer follow foundation**.

Acceptance boundary for Phase 1d:

1. Add one obvious placeholder healer body to the greybox, with collision matching its visible
   footprint and a named starting position on the Seeker's side of the stream.
2. Keep follow-target selection, stop distance, and finite-coordinate/speed validation in pure
   Application code with focused independent tests.
3. Add deterministic Godot movement that follows the Seeker only while outside the validated
   stop distance, uses the existing navigation region for routes, and makes no LLM or network
   call.
4. Add a credential-free Godot check proving representative follow, stop-distance, separation,
   and cross-bridge regroup behavior without relying only on a rendered debug overlay.
5. Manually confirm on the current native platform that the healer follows smoothly, does not
   overlap the Seeker, can become separated by ordinary movement, and regroups through the bridge
   without breaking existing Seeker collision or camera behavior.
6. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1d:

- Do not add healer wait or player-selected move-to-destination commands; those remain the next
  roadmap item after the follow foundation.
- Do not add point-and-click Seeker movement, zoom/pan input, controller mappings, LLM calls,
  provider adapters, dialogue, interaction highlighting, inventory, disease, persistence, final
  art, audio, networking, or packaging.
- Do not change repository visibility, merge pull requests, create releases, distribute
  artifacts, or add credentials without explicit user authorization.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact bounded slice and preserve its exclusions.
- Tick roadmap items only after current automated and manual evidence exists.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Keep changes narrow, inspect tracked and untracked files, and stage only intended paths.
- Record branch, commit, pull request, checks, native evidence, and limitations without
  overstating unavailable platform or release validation.
