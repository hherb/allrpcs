# Allrpcs Handover

## Current state

PR #6 merged Phase 1d into `origin/main` at merge commit `95d2033`. The current branch,
`feat/healer-command-modes`, is based directly on that merge.

Phase 1e, deterministic healer wait and move-to-destination behavior, is complete on the current
branch. The complete host-shell quality sequence passed and the user confirmed the native macOS
behavior on 2026-08-31. The healer wait and move-to-destination roadmap item and the broader Phase
1 acceptance are ticked. Interaction highlighting, world-decision pausing, the debug overlay, and
automated movement-boundary coverage remain unticked.

The reviewed implementation is ready to commit and publish in the explicitly requested draft pull
request. This file must be updated with the resulting commit, pull request, and checks before the
slice is handed off. The pull request is not authorized to merge. Public visibility, merging,
releases, artifact distribution, credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/healer-command-modes --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/healer-command-modes --repo hherb/allrpcs
```

## Phase 1e implementation

The slice remains within its original boundary:

- `src/Application/Movement/HealerCommandSelector.cs` defines typed follow, wait, and
  move-to-destination modes. Follow and wait carry no destination; move selection reuses the pure
  navigation validator to require finite coordinates inside the inclusive world bounds. The
  selection has no Godot dependency.
- `tests/Application.Tests/HealerCommandSelectorTests.cs` adds 15 focused cases for follow, wait,
  ordinary and boundary destinations, invalid bounds, non-finite coordinates, and out-of-bounds
  destinations. The Application suite now has 81 passing cases.
- `game/Healer.cs` keeps the active validated command, makes wait stationary, accepts a move only
  when its target is also on the synchronized authored navigation map, moves to within a named
  12-pixel stop distance, transitions to wait on arrival, and resumes the existing deterministic
  follow behavior on command.
- `game/Main.cs`, `game/project.godot`, and `game/Main.tscn` add only a temporary X toggle for
  follow/wait, an M command to an obvious pink target ring, and a small **TEMP GREYBOX** panel that
  reports the current mode. They do not add dialogue or final player-facing command UX.
- `game/HealerCommandSmokeCheck.cs` replaces the narrower follow smoke. It proves initial follow,
  stable wait after separation, fail-closed rejection of an out-of-bounds target and the excluded
  village target, a valid move through the bridge, automatic wait on arrival, and return to follow
  through the bridge without body overlap.
- `scripts/smoke-godot.sh` retains the navigation-topology smoke and runs the expanded healer
  command smoke in a separate bounded, credential-free headless launch.
- `README.md` documents the temporary controls, pure boundary, automated coverage, behavior, and
  exclusions. `ROADMAP.md` records only the completed Phase 1e work and now-satisfied Phase 1
  greybox acceptance.

## Review and validation

### Automated evidence

On 2026-08-31, the complete host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with
.NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `actionlint` accepted the hosted workflow;
- restore and `dotnet format --verify-no-changes` passed;
- all seven projects built with zero warnings and zero errors;
- the Domain test passed;
- all 81 Application movement, camera-framing, navigation-request, follow-target, and healer
  command cases passed;
- all six architecture reference cases passed;
- the headless navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map
  iteration 2, found the representative bridge route, rejected stream/tree/village endpoints,
  and exited 0;
- the headless healer command smoke proved follow-to-wait, stable wait, rejection of invalid and
  excluded destinations, valid move completion, automatic wait, body separation, and return to
  follow through the bridge at a final 72.02504-pixel separation, then exited 0; and
- the bounded 60-frame native graphical launch exited 0 using the Compatibility renderer on the
  Apple M3 Max.

The visible native run produced no Godot error or warning entries after the final rebuild.

### Native visual and interaction evidence

The game was launched in a visible native 960-by-540 macOS window. On 2026-08-31, the user
confirmed that:

1. X changes the obvious status to wait and the healer does not drift while the Seeker moves;
2. M moves the healer to the visible pink ring and the mode returns to wait on arrival;
3. X then resumes follow; and
4. existing Seeker terrain collision and camera behavior continue to work under WASD input.

The prior Phase 1c confirmation that the stream outside the bridge, village, three-tree cluster,
and outer border block the Seeker remains applicable. The prior Phase 1d confirmation that the
healer follows without overlap and regroups through the bridge also remains applicable.

## Limitations and exclusions

- Phase 1e has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, arrow keys, gamepads, accessibility, performance, packaging,
  installation, signing, or distribution.
- The pure tests validate typed command selection and world bounds but do not load Godot,
  simulate physics, or prove navigation topology. The credential-free Godot smoke validates
  representative command transitions, movement, target exclusion, topology, and separation; the
  current macOS manual run validates ordinary-player interaction.
- The command smoke deliberately places characters at deterministic representative positions. It
  is not exhaustive for every position, trajectory, navigation-map mutation, or target.
- The healer body, speeds, stop distances, navigation-agent tuning, bridge, terrain, clearance,
  target ring, X/M mappings, and mode panel are temporary greybox values. There is no crowd
  avoidance, dynamic obstacle rebaking, animation, or final companion presentation.
- No natural-language interpretation, dialogue, LLM call, provider adapter, autonomous planning,
  healer inventory action, point-and-click Seeker movement, zoom/pan input, broader controller
  mapping, interaction highlighting, inventory, disease, persistence, final art, audio,
  networking, packaging, credential, release, or distribution path was added.

## Exact next bounded slice

First review the Phase 1e draft pull request and its current checks. Do not merge it without
explicit user authorization. After the user reports that pull request merged, take **Phase 1f:
interaction highlighting and focused-target indicator foundation**.

Acceptance boundary for Phase 1f:

1. Add pure Application selection and validation for a bounded set of finite, visible,
   in-interaction-range greybox candidates, with deterministic tie-breaking, focused independent
   tests, and no Godot dependency.
2. Add only the minimum obvious temporary greybox candidates needed to exercise selection; do not
   create the Phase 2 authoritative object, perception, or stable-identifier model early.
3. Adapt the Godot presentation so only the currently eligible focused target has an obvious
   indicator, the focus changes deterministically as the Seeker moves, and it clears when no
   candidate is eligible.
4. Add a credential-free Godot check proving representative selection, deterministic tie-breaking,
   focus change, out-of-range clearing, and no hidden or excluded target presentation.
5. Manually confirm on the current native platform that the focus is understandable and stable
   without breaking movement, collision, camera behavior, or healer command modes.
6. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1f:

- Do not add interaction actions or outcomes, dialogue, natural-language interpretation, an LLM
  call, provider adapters, autonomous planning, inventory, disease, persistence, or Phase 2 world
  and perception models.
- Do not add point-and-click Seeker movement, zoom/pan input, broader controller mappings, final
  UI, final art, audio, networking, or packaging.
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
