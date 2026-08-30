# Allrpcs Handover

## Current state

PR #3 merged Phase 1a into `origin/main` at merge commit `2f97f5d`. The current branch,
`feat/bounded-camera-terrain-collision`, is based directly on that merge.

Phase 1b, bounded camera framing and greybox terrain collision, is complete on the current
branch. The complete host-shell quality sequence passed and the user confirmed the native
macOS behavior on 2026-08-31. The bounded-camera and terrain-collision roadmap items are ticked;
the broader Phase 1 acceptance remains unticked.

The branch is intended to track the private remote and have an open draft pull request. It is
not authorized to merge. Public visibility, merging, releases, artifact distribution,
credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/bounded-camera-terrain-collision --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/bounded-camera-terrain-collision --repo hherb/allrpcs
```

The expected state is a clean branch tracking its remote with an open draft pull request and
current checks. Do not merge it without separate user authorization.

## Phase 1b implementation

The slice remains within its original boundary:

- `src/Application/Camera/CameraFramingCalculator.cs` validates finite positive world bounds,
  viewport size, ordered zoom tuning, and the minimum zoom needed to keep the viewport inside
  the world. It uses only pure C# and `System.Numerics.Vector2`.
- `game/FollowCamera.cs` gives the existing follow camera named world limits of left/top zero and
  right/bottom 1600/900 pixels. It enables limit smoothing, retains the named follow-smoothing
  speed of 6, and establishes named minimum/default/maximum zoom values of 0.75/1.0/2.0.
- Zoom and camera-pan input remain unmapped. The minimum and maximum are validated tuning bounds,
  while the stable default is the only zoom applied by this slice.
- `game/Main.tscn` adds deliberately simple collision geometry for the visible stream band, the
  complete village block, and a visible three-tree cluster. The existing outer boundary remains
  collidable and the path remains traversable. None of these primitives is final terrain or art.
- `tests/Application.Tests/CameraFramingCalculatorTests.cs` adds 17 focused cases covering valid
  framing, exact world coverage, invalid bounds, invalid viewports, invalid zoom ordering, and
  excessive zoom-out. The Application suite now has 26 passing cases in total.
- `README.md` documents the camera values, collision behavior, project boundary, and explicit
  exclusions. `ROADMAP.md` records only the two completed Phase 1b items.

## Review and validation

### Automated evidence

On 2026-08-31, the complete host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with
.NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `actionlint` accepted the hosted workflow and every shell script passed syntax validation;
- restore and `dotnet format --verify-no-changes` passed;
- all seven projects built with zero warnings and zero errors;
- the domain test passed;
- all 26 Application movement and camera-framing cases passed;
- all six architecture reference cases passed;
- the headless main-scene smoke exited 0; and
- the bounded 60-frame native graphical launch exited 0 using the Compatibility renderer on
  the Apple M3 Max.

The restricted sandbox's first headless launch could not open Godot's normal user log and
crashed inside log rotation before loading the project. The identical smoke passed in the host
environment, so this is recorded as a sandbox limitation rather than a product failure. The
visible native run produced no Godot error or warning entries in the application log.

### Native visual evidence

The game was launched in a visible native 960-by-540 macOS window. On 2026-08-31, the user
confirmed that:

1. ordinary WASD movement never reveals space outside the outlined 1600-by-900 region;
2. the Seeker is blocked by the blue stream, the complete grey village block, and all three
   visible tree shapes; and
3. movement and camera following remain smooth.

The prior Phase 1a confirmation that the outer border blocks the Seeker remains applicable.

## Limitations and exclusions

- Phase 1b has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, the unused zoom range, arrow keys, gamepads, accessibility,
  performance, packaging, installation, signing, or distribution.
- The pure tests validate camera configuration and velocity calculation. They do not simulate
  Godot input delivery, rendered camera edges, or physics collision; those behaviors have only
  current macOS manual evidence.
- Stream collision is a simple polygon following the visible band, the village is one rectangle,
  and each tree uses one circle. The stream has no bridge or ford yet, and no navigation region
  or path-connectivity claim is made.
- No point-and-click movement, zoom/pan input, controller mappings, healer behavior, companion
  AI, pathfinding, navigation region, highlighting, inventory, dialogue, disease, provider,
  persistence, final art, audio, networking, packaging, credential, release, or distribution
  path was added.

## Exact next bounded slice

First review the Phase 1b draft pull request and its current checks. Do not merge it without
explicit user authorization. After the user reports that pull request merged, take **Phase 1c:
greybox navigation-region foundation**.

Acceptance boundary for Phase 1c:

1. Add one obvious temporary bridge or ford through the stream and revise only the necessary
   stream collision so the main greybox region has a representative connected crossing.
2. Add a baked or deterministically authored `NavigationRegion2D` suitable for future companion
   pathfinding across the current 1600-by-900 region. It must exclude the stream outside the
   crossing, village block, tree cluster, and outer impassable boundary.
3. Keep navigation request bounds and finite-coordinate validation in pure Application code and
   add focused independent tests. Do not add companion decision-making or LLM calls.
4. Add a credential-free Godot check that establishes the navigation map and representative
   reachable/unreachable path results without relying only on a rendered debug overlay.
5. Manually confirm on the current native platform that ordinary Seeker movement and collision
   remain intact and the temporary crossing is traversable.
6. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1c:

- Do not add point-and-click movement, zoom/pan input, controller mappings, healer follow/wait
  behavior, companion controllers, LLM calls, interaction highlighting, inventory, dialogue,
  disease, providers, persistence, final art, audio, networking, or packaging.
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
