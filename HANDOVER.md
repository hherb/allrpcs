# Allrpcs Handover

## Current state

`main` contains merged PR #222 at `09605b1`. The current branch is
`codex/phase-7-independent-task-return`, with seven reviewed implementation commits:

- `eeab97e` defines exact completed-task return plans and immutable progress;
- `0e1779f` scene-owns return plans and compare-and-swap waypoint advancement;
- `7067533` authors the exact healer route from the herb patch to its outbound origin;
- `eadd139` initializes that route atomically with the accepted final arrival occurrence;
- `ce54e54` defines stable bounded party rejoin policy and transition;
- `a5a2637` composes an exact final return waypoint with party rejoin;
- `7ae32bb` installs final return progress and party membership atomically in the scene.

Phase 7 now has a complete explicit outbound-task-return contract:

- `CharacterIndependentTaskReturnTravelPlan` retains the exact successful arrival-occurrence
  result, exact completed task activity, validated bounds and origin, and 1-32 copied waypoints;
- return travel must start at the outbound final waypoint and end at the outbound origin, and
  incomplete or mismatched outbound/task results fail closed;
- immutable return progress advances only through the exact next waypoint and rejects skips,
  repeats, completion, and distinct-plan identity;
- `CharacterIndependentTaskReturnTravelController` scene-owns one exact plan and progress;
- `TemporaryCharacterIndependentTaskReturnTravelSource` authors only the exact completed healer
  route from the herb patch back to the captured departure origin;
- the final outbound arrival-occurrence operation retains its exact combined result and initializes
  the authored return controller only after all task, travel, event, time, and transcript input
  validates;
- Domain party rejoin appends the exact absent character after existing stable members only while
  the 32-member party has capacity;
- the pure return-rejoin transition composes exactly one final return advance with that rejoin;
- the return controller and task controller compare-and-swap exact return, completed-task, and party
  state before installing the fresh completed progress and party together.

`Main` still initializes outbound travel but does not advance it, trigger the task occurrence,
advance return travel, or rejoin the healer. The healer remains visible and continues ordinary body
behavior. No elapsed-time, speed, interpolation, collision, navigation-agent,
off-screen-location, body-presentation, dialogue/model-context, persistence, or provider policy was
added.

## Validation and review

- Each slice was driven through a focused missing-type or missing-member compile failure before its
  implementation.
- Focused host-local tests pass: 4 Domain party-rejoin tests, 8 Application return-plan/progress
  tests, and 4 Application final-return/rejoin tests.
- The focused return controller, authored return source, expanded arrival-occurrence, and final
  return-rejoin Godot smokes pass host-local.
- The validation spine passed workflow validation, restore, formatting, a zero-warning solution
  build, 2,216 Domain, 473 Application, 532 Infrastructure, and 8 architecture tests.
- The script's `dotnet run` ScenarioValidator apphost then stalled with no output; it was interrupted
  and the same built validator passed through `dotnet exec`, validating all 5 scenario definitions.
- All 100 credential-free headless Godot smoke invocations pass host-local, including the four new
  or expanded return-flow checks.
- The Compatibility-renderer graphical launch passes on the Apple M3 Max.
- Restricted VSTest reproduced the known loopback socket denial, and restricted Godot reproduced
  the known `user://logs` crash; the identical focused commands passed host-local.
- Cumulative `git diff --check` and the practical 120-character authored-code check pass.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7ci: coordinate one explicit independent-task round-trip step**.

Acceptance boundary:

1. Add one scene-owned coordinator that delegates exactly one next outbound or return waypoint from
   the exact current controller state supplied by the caller.
2. At the outbound final waypoint, delegate the existing atomic arrival-occurrence operation so the
   exact return route initializes; at the return final waypoint, delegate the existing atomic
   return-rejoin operation. Do not duplicate Domain or Application transition rules.
3. Add one credential-free smoke proving the authored one-waypoint outbound route and one-waypoint
   return route progress through task occurrence and rejoin in order, while stale, mismatched,
   repeated, and null input leaves every scene-owned snapshot unchanged.
4. Do not call the coordinator from `Main`; select elapsed time, speed, interpolation, collision,
   navigation-agent, or off-screen scheduling policy; move or hide a body; change
   dialogue/model-context; persist state; or invoke a provider.
