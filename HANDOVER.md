# Allrpcs Handover

## Current state

Roadmap Phase 0.2 is complete on `origin/main` at merge commit `cefe68f`; PR #2 merged the
final hosted-runner evidence follow-up and all five of its credential-free checks passed. The
current branch is `feat/keyboard-greybox-camera`, based directly on that merge.

Phase 1a, keyboard greybox movement and camera foundation, is complete on
`feat/keyboard-greybox-camera`. The complete host-shell quality sequence passed and the user
confirmed the native macOS visual behavior on 2026-08-31. The four completed Phase 1 roadmap
items are ticked; the broader Phase 1 acceptance remains unticked.

This session is authorized to commit and push the slice and open its draft pull request. It is
not authorized to merge it. The private remote is `https://github.com/hherb/allrpcs`. Public
visibility, merging, releases, artifact distribution, credentials, signing, and notarization
remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/keyboard-greybox-camera --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/keyboard-greybox-camera --repo hherb/allrpcs
```

The expected state is a clean branch tracking its remote with an open draft pull request and
current checks. Do not merge it without separate user authorization.

## Phase 1a implementation

The slice remains within its original boundary:

- `game/Main.tscn` authors a 1600-by-900 placeholder region from Godot polygons and lines. It
  includes a temporary stream, path, village block, outer border, gold Seeker, and only four
  invisible static walls needed to keep the Seeker inside the region. None is final art.
- `game/project.godot` defines `move_left`, `move_right`, `move_up`, and `move_down`, mapped only
  to physical A, D, W, and S keyboard keys. Gameplay code consumes the named actions and has no
  hard-coded physical key checks. No controller or point-and-click mapping was added.
- `game/Seeker.cs` moves a `CharacterBody2D` continuously at the named default speed of 240
  pixels per second and delegates direction-to-velocity calculation to the Application layer.
- `src/Application/Movement/MovementCalculator.cs` is pure C# using `System.Numerics.Vector2`.
  It preserves proportional input, normalizes over-length diagonal input, and rejects negative
  or non-finite speed and non-finite input.
- `game/FollowCamera.cs` owns the named follow-smoothing value of 6, establishes unit zoom and
  zero local offset, makes itself current, and resets smoothing for stable initial framing.
- `tests/Application.Tests` adds nine focused movement-calculation cases. The solution,
  architecture guard, local quality script, and hosted workflow all include the new test
  project.
- `README.md` documents the WASD controls, named speed and camera values, temporary greybox
  status, project boundary, tests, and explicit exclusions.

## Review and validation

### Automated evidence

On 2026-08-31, the complete host-shell `./scripts/quality.sh` passed on Apple Silicon macOS with
.NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `actionlint` accepted the hosted workflow and every shell script passed syntax validation;
- restore and `dotnet format --verify-no-changes` passed;
- all seven projects built with zero warnings and zero errors;
- the domain test passed;
- all nine Application movement tests passed;
- all six architecture reference cases passed;
- the headless main-scene smoke exited 0; and
- the bounded 60-frame native graphical launch exited 0 using the Compatibility renderer on
  the Apple M3 Max.

The restricted sandbox test host separately failed to bind its local communication socket.
The same tests passed outside that restriction, so this is recorded as a sandbox limitation,
not a product failure.

### Native visual evidence

The first direct binary launch initialized the Compatibility renderer but did not present a
window in the user's desktop session, so it was not counted as visual evidence. The project was
then relaunched through macOS with an explicit windowed 960-by-540 configuration and visible
screen position. In that visible native window, the user confirmed that:

1. W, A, S, and D move the gold placeholder Seeker continuously;
2. the camera follows smoothly; and
3. the outer border stops the Seeker from leaving the temporary region.

The pure movement tests establish that simultaneous diagonal input is normalized to the same
configured maximum speed as cardinal input; diagonal speed was not separately measured in the
native window.

## Limitations and exclusions

- Phase 1a has not exercised Windows, Linux, alternate keyboard layouts, arrow keys, gamepads,
  accessibility behavior, performance, packaging, installation, signing, or distribution.
- The pure tests establish velocity calculation. They do not simulate Godot input delivery,
  camera rendering, or physics collision.
- The stream, village block, and path are visual primitives only. Only the region perimeter has
  collision; water, buildings, trees, and other terrain remain traversable.
- No point-and-click movement, controller mappings, healer behavior, companion AI, pathfinding,
  navigation region, highlighting, inventory, dialogue, disease, provider, persistence, art,
  audio, networking, packaging, credential, release, or distribution path was added.

## Exact next bounded slice

First review the Phase 1a draft pull request and its current checks. Do not merge it without
explicit user authorization. After the user reports that pull request merged, take **Phase 1b:
bounded camera framing and greybox terrain collision**.

Acceptance boundary for Phase 1b:

1. Give the existing follow camera named world limits so the viewport does not reveal space
   beyond the 1600-by-900 test region during ordinary Seeker movement.
2. Define named minimum and maximum zoom tuning values and a stable default. Do not add zoom or
   camera-pan input until its keyboard/controller interaction is separately decided.
3. Add simple temporary collision primitives for the visible stream, village block, and a small
   representative tree cluster. Keep them obviously greybox and do not select final assets.
4. Keep bounds and validation calculations pure and independently tested where practical;
   retain all domain, Application, architecture, workflow, headless, and graphical checks.
5. Manually confirm on the current native platform that camera framing stays inside the region,
   the Seeker cannot cross the new obstacles, and ordinary movement remains smooth.
6. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1b:

- Do not add point-and-click movement, zoom/pan input, controller mappings, healer behavior,
  companion AI, pathfinding, navigation regions, interaction highlighting, inventory, dialogue,
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
