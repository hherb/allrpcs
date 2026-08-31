# Allrpcs Handover

## Current state

PR #8 merged Phase 1f into `origin/main` at merge commit `e516398`. All five credential-free
jobs in its final pull-request run `33383155284` passed. The current branch,
`feat/seeker-movement-boundary-coverage`, is based directly on that merge.

Phase 1g, automated Seeker movement-boundary coverage, is implemented on the current branch.
The new engine-side check and the complete local quality sequence pass. It changes no production
movement rule, input mapping, collider, or scene geometry. On 2026-08-31, the user confirmed the
current native WASD collision and bridge traversal plus camera, healer-command, and
interaction-focus paths all behaved normally. The movement-boundary roadmap item is ticked.

No commit, push, or pull request exists for Phase 1g yet. Public visibility, merging, releases,
artifact distribution, credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
```

## Phase 1g implementation

The slice remains within its original boundary:

- `game/SeekerMovementBoundarySmokeCheck.cs` uses deterministic, test-only Godot physics motions
  from the existing Seeker `CharacterBody2D`. It verifies all four authored outer walls, a stream
  crossing outside the bridge, the village block, and the west tree in the representative tree
  cluster report the exact expected authored collider.
- The same check proves direct traversal over the bridge in both directions and across
  representative open lower terrain without a collision. It disables the Seeker and healer
  physics callbacks only while the explicit smoke environment variable is active, so ordinary
  launches retain their existing behavior.
- `game/Main.tscn` registers only the inert-by-default smoke node and script. No production
  collider, collision layer, body shape, position, input action, camera value, navigation
  polygon, healer behavior, or interaction-focus behavior changed.
- `scripts/smoke-godot.sh` adds the bounded
  `ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path game --quit-after 30` launch to local
  and hosted Godot coverage.
- Existing pure `MovementCalculatorTests` already cover no input, cardinal and proportional
  velocity, normalized diagonal speed, invalid speed, and non-finite input. No engine-independent
  boundary policy was identified, so this slice does not duplicate Godot physics in Application.
- `README.md` documents the coverage, direct command, test-only scope, and Application/Godot
  ownership boundary. `ROADMAP.md` records the completed automated and ordinary-input evidence.

## Review and validation

### Automated evidence

On 2026-08-31, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` and
  `dotnet build Allrpcs.sln --no-restore -m:1 /nr:false` passed; all seven projects built with
  zero warnings and zero errors;
- `ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path game --quit-after 30` passed all four
  outer walls, the representative stream, village, and tree collisions, both bridge directions,
  and open lower terrain;
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, the one Domain test, all 100 Application tests, all six
  architecture reference tests, all four Godot smokes, and the bounded 60-frame native graphical
  launch;
- the navigation smoke again synchronized the 98-vertex, 66-polygon region at navigation map
  iteration 2, found the bridge route, and rejected representative obstacle targets;
- the healer-command smoke again proved follow, stable wait, invalid and excluded move rejection,
  valid move completion, automatic wait, body separation, and return to follow through the bridge
  at a final 72.02504-pixel separation; and
- the interaction-focus smoke again proved nearest selection, authored-order ties, focus change,
  out-of-range clearing, and hidden and disabled exclusions.

The focused format/build initially ran in the restricted environment and completed serially in
2 minutes 8.50 seconds. Godot verification and the complete quality sequence ran from the host
shell because Godot requires access to its normal `user://` log and native runtime. No sandbox
failure is being reported as a product result.

### Native visual and interaction evidence

The complete quality sequence launched the current native scene with Godot's Compatibility
renderer on the Apple M3 Max and exited cleanly after 60 frames. A longer native run then used
ordinary input in the current branch. The user confirmed understandable WASD movement and
collision at the authored boundaries and obstacles, bridge traversal, camera following, X/M
healer-command behavior, and interaction-focus changing and clearing all behaved normally.

## Review findings

The reviewed diff is narrow and follows the intended trust boundary: the new file is a
credential-free test adapter under `game`, the scene adds one disabled-by-default check node, and
the smoke script is the only execution-path change. The check uses the authored Seeker body and
exact collider nodes rather than copying scene coordinates into Application collision policy.

The test-only sweeps start clear of every obstacle, request a motion through the expected shape,
and assert the reported collider identity. Clear-path cases use the same body and fail on any
collider. The bridge is exercised in both directions. The check exits on its first failure,
remains bounded by the shell timeout, and cannot affect ordinary launches without its explicit
environment variable.

No correctness, security, architecture, or scope issue remained after review. The authored test
coordinates are intentionally coupled to the greybox scene: a later deliberate geometry change
must update this regression evidence with review.

## Limitations and exclusions

- Phase 1g has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, arrow keys, gamepads, accessibility, performance, packaging,
  installation, signing, or distribution.
- Test-only `MoveAndCollide` sweeps prove the existing body and authored shapes block or permit
  representative motions. They do not synthesize keyboard events, time a player's multi-frame
  trajectory, exercise sliding at every contact angle, or exhaust every pixel of every collider.
- The stream, village, and tree checks each use one representative approach. All four outer walls
  and both directions through the bridge are covered. This is greybox regression evidence, not a
  general geometry validator or a final input-UX test.
- The bounded graphical launch proves native startup and renderer selection. The separate current
  macOS run supplies ordinary-input regression evidence, but it is one qualitative manual pass,
  not exhaustive automation or cross-platform evidence.
- No point-and-click movement, interaction action or outcome, authoritative object, stable typed
  identifier, perception or visibility model, discovery state, inventory, disease, world-time
  pause, debug overlay, natural-language interpretation, dialogue, LLM call, provider adapter,
  autonomous planning, persistence, zoom/pan input, broader controller mapping, final UI, final
  art, audio, networking, packaging, credential, release, or distribution path was added.

## Exact next bounded slice

After the user reports that the Phase 1g pull request merged, take **Phase 2a: stable typed
identifier primitives**.

Acceptance boundary for Phase 2a:

1. Add small engine-independent Domain value types for character, object, location, evidence,
   knowledge, and world-event identifiers, with one explicit canonical text and validation
   contract shared across the types.
2. Add focused pure tests for valid construction, invalid values, type-safe equality, and
   canonical text round trips. Keep identifiers opaque; do not attach mutable entity state.
3. Preserve `Domain -> []` and avoid Godot, provider, persistence, UI, serialization-framework,
   or platform dependencies.
4. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 2a:

- Do not add character or world-object models, controller types, perception, discovery,
  inventory, event-log storage, save schemas, database adapters, dialogue, LLM calls, providers,
  credentials, or UI.
- Do not invent scenario content or migrate temporary Phase 1 marker indices and authored order
  into durable identity before their authoritative models exist.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
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
  overstating unavailable platform, manual, or release validation.
