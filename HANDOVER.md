# Allrpcs Handover

## Current state

`main` contains merged PR #218 at `1a9935d`. The current branch is
`codex/phase-7-independent-tasks`. Draft PR #219 is open and mergeable with implementation commit
`8a962f6`.

This branch completes Phase 7bk-7br as one reviewed Domain foundation for bounded independent tasks
and party separation:

- canonical `CharacterIndependentTaskId`, exact non-wait examine/collect assignments, and copied
  32-entry assignment snapshots;
- closed assigned, active, completed, and cancelled lifecycle state with legal forward-only
  transitions;
- bounded activity snapshots with unique task identities and at most one active task per
  value-equal character;
- stable first-assigned task selection and exact contained-activity replacement;
- exact active-task binding to existing authoritative physical-action world-event occurrences and
  atomic task/event-log/world-time/transcript completion;
- bounded ordered party membership, pure separation, and atomic task start plus same-character
  departure.

The slice does not source authored assignments; scene-own task or party state; drive tasks,
movement, or off-screen time; infer world events; rejoin characters; add model/provider context;
persist task or party state; invoke live providers; dispatch workflows; release; sign; or
distribute.

## Validation and review

- Every contract was driven through a focused failing compile state before implementation.
- The complete focused independent-task and party suite passes host-local: 94 tests, zero failures.
- Review tightened snapshot construction so two value-equal active tasks for one character are
  rejected even when callers bypass the scheduler.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,212 Domain, 443 Application, 532 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions; the complete headless Godot smoke matrix; and Compatibility-renderer launch.
- Restricted VSTest reproduced the known localhost `SocketException (13)`; identical focused tests
  passed host-local.
- No live-conformance environment variable was enabled and no provider was invoked.
- Draft PR #219 is open and mergeable. Hosted run `34735264239` completed with all five jobs at
  `steps: []`; it provides no product-test or unsigned-build evidence and should not be retried
  unchanged.

## Exact next bounded slice

Take **Phase 7bs: scene-owned independent-task departure state**.

Acceptance boundary:

1. Add a focused Godot node controller that owns one exact
   `CharacterIndependentTaskActivitySnapshot` and one exact `CharacterPartySnapshot`, with
   one-time atomic initialization.
2. Add one compare-and-swap-style `TryStartAndSeparate` operation. It must require the exact
   expected current snapshots, delegate to `CharacterIndependentTaskDepartureTransition`, and
   install both resulting snapshots plus the exact active activity only after full success.
3. Add a credential-free Godot smoke proving stable first-task selection, same-character party
   removal, stale-snapshot rejection, ineligible no-op behavior, and no partial installation.
4. Do not yet author assignment sources; drive task occurrences, navigation, movement, or
   off-screen simulation; rejoin a party member; change dialogue/model context; persist state; or
   invoke a provider.
