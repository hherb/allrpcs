# Allrpcs Handover

## Current state

PR #7 merged Phase 1e into `origin/main` at merge commit `6724c3f`. All five credential-free
jobs in its final pull-request run `33342873481` passed. The current branch,
`feat/interaction-focus-foundation`, is based directly on that merge.

Phase 1f, interaction highlighting and focused-target indicator foundation, is implemented on
the current branch. Pure tests, the full zero-warning build, and all three independent Godot
smokes pass. On 2026-08-31, the user confirmed the visible native macOS focus behavior and the
unchanged movement, collision, camera, and healer-command paths. The interaction-highlight
roadmap item is ticked.

No pull request has been opened yet. The implementation is uncommitted at this handover-writing
point; re-query the final branch head, commit, draft pull request, and hosted checks after
publication rather than treating this sentence as final publication evidence. The pull request
is not authorized to merge. Public visibility, merging, releases, artifact distribution,
credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view feat/interaction-focus-foundation --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks feat/interaction-focus-foundation --repo hherb/allrpcs
```

## Phase 1f implementation

The slice remains within its original boundary:

- `src/Application/Interaction/InteractionFocusSelector.cs` accepts at most 32 temporary
  candidates, validates finite Seeker and candidate positions plus a finite positive inclusive
  range, and selects only visible, enabled candidates in range. Nearest wins and exact distance
  ties preserve authored input order. It returns only the caller's list index and distance; it
  introduces no Godot dependency, object identity, stable identifier, perception state, or
  interaction outcome.
- `tests/Application.Tests/InteractionFocusSelectorTests.cs` adds 19 focused cases covering the
  inclusive boundary, nearest selection, deterministic ties, hidden and disabled candidates,
  empty and fully ineligible lists, the candidate cap, and invalid positions and ranges. The
  Application suite now has 100 passing cases.
- `game/GreyboxInteractionTarget.cs` adapts one temporary authored marker to the pure candidate
  record and owns only its visible yellow focus indicator. The hidden marker is also absent from
  scene presentation.
- `game/InteractionFocusController.cs` evaluates four authored markers against the named
  112-pixel range every frame, presents exactly one selected indicator and matching temporary
  status, or clears both when no marker is eligible. Authored child order is the presentation-only
  tie-break order.
- `game/Main.tscn` adds only two obvious eligible markers, one hidden check marker, one visible
  disabled check marker, focus rings, and one overlay status row. These are temporary primitives
  for behavior evidence, not authoritative world objects or final interaction UI.
- `game/InteractionFocusSmokeCheck.cs` proves representative nearest selection, authored-order
  tie-breaking, focus change, out-of-range clearing, hidden target absence, disabled target
  exclusion, and exact one-indicator presentation.
- `scripts/smoke-godot.sh` runs the focus smoke in its own bounded, credential-free headless
  launch so its requested tree exit does not interfere with the existing navigation and healer
  checks.
- `README.md` documents the pure/presentation boundary, temporary focus behavior, automated
  coverage, and exclusions. `ROADMAP.md` records the completed implementation and current
  ordinary-input evidence.

## Review and validation

### Automated evidence

On 2026-08-31, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --no-restore -m:1
  /nr:false` passed all 100 Application cases after the test-first selector implementation;
- `dotnet build Allrpcs.sln --no-restore -m:1 /nr:false` built all seven projects with zero
  warnings and zero errors;
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, the one Domain test, all 100 Application tests, all six
  architecture reference tests, all three Godot smokes, and the bounded 60-frame native
  graphical launch;
- rerunning `ALLRPCS_NAVIGATION_CHECK=1 godot --headless --path game --quit-after 120`
  independently synchronized the 98-vertex, 66-polygon region at navigation map iteration 2,
  found the bridge route, rejected representative obstacles, and exited 0;
- rerunning `ALLRPCS_HEALER_COMMAND_CHECK=1 godot --headless --path game --quit-after 1500`
  independently proved follow, stable wait, invalid and excluded move rejection, valid move
  completion, automatic wait, body separation, and return to follow through the bridge at a final
  72.02504-pixel separation, then exited 0;
- `ALLRPCS_INTERACTION_FOCUS_CHECK=1 godot --headless --path game --quit-after 30` proved nearest
  selection, authored-order ties, focus change, out-of-range clearing, and hidden and disabled
  exclusions, then exited 0; and
- the final bounded graphical launch selected the Compatibility renderer on the Apple M3 Max and
  exited 0.

The initial sandboxed `dotnet test` attempt failed because the sandbox denied MSBuild's local
named-pipe binding; the host-shell rerun passed and this is not a product failure. A sandboxed
Godot launch similarly could not open its normal `user://` log and crashed in log rotation; every
reported Godot result above is from the required host-shell rerun. The intermittent full-script
stall showed only a shell stuck before a Godot process started; the same three Godot commands
passed independently, and the subsequent complete quality rerun passed end to end.

### Native visual and interaction evidence

The current branch launched in a visible native 960-by-540 macOS window with the Compatibility
renderer on the Apple M3 Max. The initial midpoint presentation was legible: only the herb patch
had the bright yellow **FOCUS** ring, the overlay said `Interaction focus: HERB PATCH`, the hidden
marker was absent, and the disabled marker had no focus indicator. There were no visible Godot
errors or warnings.

After the app was relaunched, the user confirmed on 2026-08-31 that ordinary WASD movement changed
focus between the eligible herb and stone markers and cleared it out of range, the hidden marker
stayed absent, and the disabled marker never received the indicator. The user also confirmed that
movement, collision, camera behavior, and the X/M healer command modes continued to work
correctly.

## Limitations and exclusions

- Phase 1f has not behaviorally exercised Windows, Linux, alternate keyboard layouts, window
  resizing, other resolutions, arrow keys, gamepads, accessibility, performance, packaging,
  installation, signing, or distribution.
- The pure tests validate finite input, eligibility, inclusive range, list bounds, proximity, and
  tie policy. They do not load Godot, simulate input or physics, or prove scene presentation.
  The credential-free focus smoke proves representative presentation transitions by placing the
  Seeker at deterministic positions; the current macOS manual run separately validates ordinary
  player input and presentation.
- The focus smoke is representative rather than exhaustive for every position, trajectory,
  dynamic visibility change, scene mutation, or overlapping candidate set.
- Candidate indices and authored child order are deliberately local presentation details, not
  durable identity. The four markers, labels, colors, 112-pixel range, focus ring, and status row
  are temporary greybox values.
- No interaction action or outcome, authoritative object, stable typed identifier, perception or
  visibility model, discovery state, inventory, disease, world-time pause, debug overlay,
  natural-language interpretation, dialogue, LLM call, provider adapter, autonomous planning,
  persistence, point-and-click movement, zoom/pan input, broader controller mapping, final UI,
  final art, audio, networking, packaging, credential, release, or distribution path was added.

## Exact next bounded slice

After the user reports that pull request merged, take **Phase 1g: automated Seeker
movement-boundary coverage**.

Acceptance boundary for Phase 1g:

1. Add a credential-free Godot physics check for representative outer-border, stream-bank,
   village, and tree-cluster collisions using the existing authored scene and Seeker body.
2. Prove the bridge and representative open terrain remain traversable; keep the check
   deterministic and bounded without simulating final input UX.
3. Reuse existing pure movement normalization coverage and add pure code only if a genuine
   engine-independent boundary policy is identified; do not duplicate Godot physics in
   Application.
4. Manually confirm the current native platform still has understandable WASD movement,
   collision, camera, healer-command, and interaction-focus behavior.
5. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 1g:

- Do not add point-and-click movement, interaction actions, authoritative Phase 2 objects or
  perception, dialogue, natural-language interpretation, LLM calls, providers, inventory,
  disease, persistence, final UI/art/audio, networking, or packaging.
- Keep world-decision pausing and the world-time debug overlay deferred until the authoritative
  systems they would display or pause exist; do not create fake Phase 2 or disease state early.
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
