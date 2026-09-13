# Allrpcs Handover

## Current state

`main` contains merged PR #219 at `14c86ad`. The current branch is
`codex/phase-7-independent-task-departure`.

This branch completes Phase 7bs-7bu as one reviewed scene flow over the merged bounded
independent-task Domain contracts:

- `CharacterIndependentTaskDepartureController` owns exact task and party snapshots, initializes
  once, and starts a departure only from the exact expected current pair;
- `TemporaryCharacterIndependentTaskSource` authors one exact healer herb-patch examination and
  the exact healer-first ordinary party; `Main` initializes and starts that departure without
  moving the healer;
- `TemporaryCharacterIndependentTaskOccurrenceSource` binds the resulting exact active task to
  the existing authoritative healer examination event and its exact physical-action transcript;
- `CharacterIndependentTaskOccurrenceController` completes that task and installs fresh task,
  event-log, world-time, and transcript snapshots only from all exact expected current state;
- focused credential-free smokes prove stable first selection, same-character removal, source
  snapshot preservation, stale/ineligible/repeated no-ops, exact authoritative completion, and no
  partial installation.

Ordinary scene flow does not yet trigger the task occurrence, navigate or hide the departed healer,
simulate off-screen time, rejoin a party member, change dialogue/model context, persist task or party
state, or invoke a provider.

## Validation and review

- The departure and occurrence controller/source flows were driven through focused missing-type
  compile failures before implementation; the game project then built with zero warnings.
- All three focused independent-task Godot smokes pass host-local.
- Review corrected the authored task from a sage action to the exact healer action already named by
  the authoritative event, and retained compare-and-swap guards across every mutable scene snapshot.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` and `git diff --check` pass.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,212 Domain, 443 Application, 532 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions; the complete headless Godot smoke matrix; and Compatibility-renderer launch.
- Restricted VSTest reproduced the known localhost `SocketException (13)`; the identical complete
  quality command passed host-local.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7bv: bounded provider-neutral independent-task travel progress**.

Acceptance boundary:

1. Add one immutable Application travel plan retaining the exact active independent-task activity,
   one validated origin, and 1-32 stable copied finite waypoints inside one valid
   `NavigationWorldBounds`. Delegate coordinate/bounds validation to the existing
   `NavigationRequestValidator`.
2. Add immutable progress over that exact plan plus a pure transition that advances only the exact
   next waypoint, one step at a time, and reports completion only after the final waypoint. Reject
   inactive tasks, empty/oversized routes, invalid coordinates, skipping, repeats, and plan-identity
   mismatches without changing inputs.
3. Add focused Application tests for exact task/plan retention, copied stable waypoint order,
   boundary coordinates, one-step progression, final completion, and every rejection above.
4. Do not yet add a Godot node or navigation-agent path; choose elapsed-time, speed, interpolation,
   or collision policy; move or hide a scene body; trigger the task occurrence; rejoin a party
   member; change dialogue/model context; persist state; or invoke a provider.
