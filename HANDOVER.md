# Allrpcs Handover

## Current state

PR #5 merged Phase 1c into `origin/main` at merge commit `0784df3`. The current branch,
`feat/deterministic-healer-follow`, is based directly on that merge.

Phase 1d, the deterministic healer-follow foundation, is complete on the current branch. The
complete host-shell quality sequence passed and the user confirmed the native macOS behavior on
2026-08-31. The deterministic healer-follow roadmap item is ticked; healer wait and
move-to-destination behavior, the broader Phase 1 acceptance, and automated physics-boundary
coverage remain unticked.

The private remote has open draft PR #6 for this branch. The reviewed Phase 1d implementation is
commit `ce79507`. The pull request is not authorized to merge. Public visibility, merging,
releases, artifact distribution, credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/deterministic-healer-follow --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/deterministic-healer-follow --repo hherb/allrpcs
```

The expected state is a clean branch tracking its remote with open draft PR #6 and current checks.
Do not merge it without separate user authorization.

## Phase 1d implementation

The slice remains within its original boundary:

- `src/Application/Movement/FollowTargetSelector.cs` validates finite follower and followed
  coordinates, positive finite speed, and non-negative finite stop distance. It returns an
  immutable target selection only while the follower is outside the inclusive stop distance and
  has no Godot dependency.
- `tests/Application.Tests/FollowTargetSelectorTests.cs` adds 17 focused cases for target
  selection, the inclusive stop boundary, invalid coordinates, invalid speed, and invalid stop
  distance. The Application suite now has 66 passing cases in total.
- `game/Main.tscn` adds an obvious cyan placeholder healer at the named `HealerStart` position on
  the Seeker's side of the stream. Its 18-pixel collision radius matches its visible circular
  footprint, and its `NavigationAgent2D` uses the existing authored navigation region.
- `game/Healer.cs` adapts the pure selection into deterministic 180-pixel-per-second navigation
  movement. It follows only while outside the validated 72-pixel stop distance, moves with
  physics collision, and makes no LLM or network call.
- `game/HealerFollowSmokeCheck.cs` creates a representative lower-field separation, proves follow
  and stable stop behavior without overlap, then separates the characters across the stream and
  proves that the healer regroups through the bridge. The check fails closed and is enabled only
  by the credential-free smoke script.
- `scripts/smoke-godot.sh` retains the navigation-topology smoke and runs the new follow smoke in
  a separate bounded headless launch. Ordinary game launches do not run either assertion set.
- `README.md` documents the healer, pure boundary, automated behavior, temporary values, and
  explicit exclusions. `ROADMAP.md` records only the completed Phase 1d work.

## Review and validation

### Automated evidence

On 2026-08-31, the complete host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with
.NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `actionlint` accepted the hosted workflow;
- restore and `dotnet format --verify-no-changes` passed;
- all seven projects built with zero warnings and zero errors;
- the domain test passed;
- all 66 Application movement, camera-framing, navigation-request, and follow-target cases passed;
- all six architecture reference cases passed;
- the headless navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map
  iteration 2, found the representative bridge route, rejected stream/tree/village endpoints,
  and exited 0;
- the headless healer smoke moved from a representative separation, held a 72-pixel lower-field
  stop distance, preserved body separation, regrouped through the bridge at a final 71.08256-pixel
  center distance, and exited 0; and
- the bounded 60-frame native graphical launch exited 0 using the Compatibility renderer on the
  Apple M3 Max.

The visible native run produced no Godot error or warning entries in the application log.

### Native visual and interaction evidence

The game was launched in a visible native 960-by-540 macOS window. On 2026-08-31, the user used
WASD to exercise the requested behavior and confirmed that:

1. the cyan healer follows the Seeker smoothly and remains outside the Seeker's body;
2. ordinary Seeker movement can create separation and the healer regroups through the brown
   bridge; and
3. existing Seeker terrain collision and camera following continue to work.

The prior Phase 1c confirmation that the stream outside the bridge, village, three-tree cluster,
and outer border block the Seeker remains applicable.

## Limitations and exclusions

- Phase 1d has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, arrow keys, gamepads, accessibility, performance, packaging,
  installation, signing, or distribution.
- The pure tests validate follow selection and configuration but do not load Godot, simulate
  physics collision, or prove navigation topology. The credential-free Godot smoke validates
  representative movement and topology; the current macOS manual run validates ordinary-player
  interaction.
- The follow smoke deliberately places characters at deterministic representative positions
  before exercising actual navigation and physics movement. It is not an exhaustive proof for
  every possible character position, moving-target trajectory, or navigation-map mutation.
- The healer body, speed, stop distance, navigation-agent tuning, bridge, terrain, and clearance
  are temporary greybox values. There is no crowd avoidance, dynamic obstacle rebaking, animation,
  debug overlay, or final companion presentation.
- No healer wait or player-selected move-to-destination command, point-and-click Seeker movement,
  zoom/pan input, controller mapping, LLM call, provider adapter, dialogue, interaction
  highlighting, inventory, disease, persistence, final art, audio, networking, packaging,
  credential, release, or distribution path was added.

## Exact next bounded slice

First review the Phase 1d draft pull request and its current checks. Do not merge it without
explicit user authorization. After the user reports that pull request merged, take **Phase 1e:
deterministic healer wait and move-to-destination foundation**.

Acceptance boundary for Phase 1e:

1. Add pure Application command selection and validation for follow, wait, and a finite in-bounds
   move destination, with focused independent tests and no Godot dependency.
2. Add only the minimum obvious temporary greybox command trigger needed to exercise wait and
   move-to-destination behavior; do not present it as final dialogue or player-facing command UX.
3. Adapt the healer controller so wait remains stationary, move-to-destination uses the existing
   navigation region and stops at a validated distance, and follow can resume deterministically.
4. Add a credential-free Godot check proving representative follow-to-wait, wait stability,
   valid move completion, invalid or excluded target rejection, and return-to-follow behavior.
5. Manually confirm on the current native platform that the modes are understandable, wait does
   not drift, a valid destination is reached without overlap or obstacle regression, and follow
   resumes without breaking existing Seeker collision or camera behavior.
6. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1e:

- Do not add natural-language command interpretation, dialogue, an LLM call, provider adapters,
  autonomous planning, healer inventory actions, or final command UI.
- Do not add point-and-click Seeker movement, zoom/pan input, controller mappings beyond any
  minimum temporary greybox command trigger, interaction highlighting, inventory, disease,
  persistence, final art, audio, networking, or packaging.
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
