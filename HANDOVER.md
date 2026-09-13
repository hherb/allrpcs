# Allrpcs Handover

## Current state

`main` contains merged PR #223 at `8a3463b`. The current branch is
`codex/phase-7-round-trip-coordination`, with eleven reviewed implementation commits after that
merge.

Draft PR #224 is open and mergeable. Hosted run `34758317069` completed with all five jobs at
`steps: []`; it provides no product-test or unsigned-build evidence and should not be retried
unchanged.

Phase 7 now runs the authored healer independent task as a complete bounded live round trip:

- one scene coordinator delegates exact outbound and return waypoints through the existing atomic
  travel, occurrence, return-route, and party-rejoin operations;
- a complete caller-supplied scene snapshot selects the only active travel leg and rejects stale,
  mismatched, repeated, null, or caller-selected phase input before mutation;
- a provider-neutral cadence and immutable clock retain bounded elapsed progress and expose at most
  one due step per advance;
- the scene clock compare-and-swaps exact timing state, and a due interval is consumed only when its
  exact route step succeeds;
- `Main` initializes a three-second cadence, supplies bounded frame elapsed time, completes the
  one-waypoint outbound occurrence and one-waypoint return/rejoin on consecutive due intervals, and
  stops consuming time after completion;
- exact separated party state hides the healer body, exact rejoin restores it, and its scene
  position remains unchanged.

The next fair-sabotage foundation has also started:

- `CharacterPhysicalAccessSnapshot` retains one character and at most 64 stable value-unique
  physically accessible world-object identifiers;
- `CharacterActionAccessRequirement` and its pure policy require explicit physical access, one
  authored capability key, and one referenced knowledge identifier together;
- `CharacterSabotageActionKind` defines only the closed `conceal`, `move`, `substitute`,
  `contaminate`, `misquote`, `delay`, and `influence` identity values.

No sabotage proposal or effect, generalized concealment, off-screen position model, interpolation,
speed, collision or navigation-agent change, dialogue/model-context change, persistence, or provider
policy was added. The healer's hidden body retains its existing collision state. Access eligibility
is not occurrence authority, and action kinds do not identify an actor, target, or effect.

## Validation and review

- Every slice began with a focused missing-type, missing-member, or failing live smoke before its
  implementation.
- Focused host-local tests pass: 5 Application timing tests, 5 Domain physical-access tests, 8 Domain
  action-access tests, and 9 Domain sabotage-kind tests.
- All 13 credential-free independent-task smokes pass together, including exact timed progression,
  body hiding/restoration, stale input, and completed-state no-op coverage.
- The complete host-local `./scripts/quality.sh` spine passes workflow validation, restore,
  formatting, a zero-warning solution build, 2,238 Domain, 478 Application, 532 Infrastructure, and
  8 Architecture tests.
- The scenario validator accepts all 5 evidence definitions.
- All 104 credential-free headless Godot smoke invocations pass, and the Compatibility-renderer
  graphical launch passes on the Apple M3 Max.
- The long ordinary-model timeout smoke now waits for the legitimate background round trip before
  capturing its isolation baseline; it still proves the 30-second provider timeout adds no dialogue,
  action, fallback, or mutation.
- Restricted VSTest reproduced the known loopback `SocketException (13)` and restricted Godot
  reproduced the known `user://logs` crash; the identical focused and full commands pass host-local.
- Cumulative `git diff --check` and the practical 120-character authored-code check pass.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7cs: bind object-targeted sabotage proposals to exact access requirements**.

Acceptance boundary:

1. Add one immutable provider-neutral object-targeted sabotage proposal retaining an exact actor
   `CharacterId`, one exact `CharacterActionAccessRequirement`, and only the `conceal`, `move`,
   `substitute`, or `contaminate` kind.
2. Reject `misquote`, `delay`, and `influence` so character-, statement-, and action-target shapes
   are not falsely modeled as `ObjectId` targets.
3. Add one pure eligibility policy that requires a value-matching proposal actor and delegates the
   complete existing `CharacterActionAccessPolicy` without changing any input.
4. Add focused Domain tests for exact-instance retention, closed object-kind acceptance, wrong actor,
   missing physical/capability/knowledge access, nulls, and unchanged inputs.
5. Do not define sabotage effects or occurrence, mutate a world object or ingredient, author live
   enemy-agent access, add scene/provider wiring, change dialogue/model context, persist state, or
   broaden non-object action target shapes.
