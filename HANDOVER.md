# Allrpcs Handover

## Current state

`main` contains merged PR #220 at `04898fc`. The current branch is
`codex/phase-7-independent-task-travel`, with three reviewed implementation commits:

- `a599b6f` adds Phase 7bv provider-neutral independent-task travel plans and progress;
- `5f2b9a9` adds Phase 7bw exact scene ownership and compare-and-swap advancement;
- `ec5817d` adds Phase 7bx authored healer travel-plan composition in `Main`.

Draft PR #221 is open and mergeable.

The branch advances the existing exact active healer task without broadening its authority:

- `CharacterIndependentTaskTravelPlan` retains the exact active activity, validated world bounds
  and origin, and 1-32 stable copied waypoints; it delegates bounds and coordinate validation to
  `NavigationRequestValidator`;
- `CharacterIndependentTaskTravelProgress` starts at zero and can be advanced only by
  `CharacterIndependentTaskTravelProgressTransition` through the exact next waypoint of the exact
  same plan, one immutable snapshot at a time;
- `CharacterIndependentTaskTravelController` initializes once and installs progress only from the
  exact expected current plan and progress;
- `TemporaryCharacterIndependentTaskTravelSource` binds the exact authored active healer task to
  one fresh route from the healer's captured departure position to the existing herb-patch scene
  position;
- `Main` composes that plan only after atomic task departure and initializes travel progress without
  advancing it or changing the healer body.

Travel is not yet advanced in ordinary flow. Completion of the route does not yet gate or trigger
the independent-task occurrence. The departed healer remains visible and continues to use existing
ordinary body behavior. No elapsed-time, speed, interpolation, collision, navigation-agent,
off-screen-location, rejoin, dialogue/model-context, persistence, or provider policy was added.

## Validation and review

- Phase 7bv and 7bw were driven through focused missing-type compile failures before implementation.
- The 10 focused Application travel tests pass, and the complete Application suite now passes all
  453 tests.
- The focused travel-controller and authored-travel-source Godot smokes pass host-local.
- Review made `System.Numerics.Vector2` explicit at the Godot boundary and made the authored-source
  smoke compare against the deterministic `HealerStart` marker because ordinary healer physics can
  run before the smoke process callback.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, cumulative `git diff --check`, and
  the practical 120-character authored-code check pass.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,212 Domain, 453 Application, 532 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions; the complete headless Godot smoke matrix; and Compatibility-renderer launch.
- Restricted MSBuild reproduced the known local socket denial, and restricted Godot reproduced the
  known `user://logs` crash; the identical focused and complete commands passed host-local.
- No live-conformance environment variable was enabled and no provider was invoked.
- Initial hosted run `34741561533` completed with all five jobs at `steps: []`; it provides no
  product-test or unsigned-build evidence and should not be retried unchanged.

## Exact next bounded slice

Take **Phase 7by: require exact completed travel before independent-task occurrence**.

Acceptance boundary:

1. Add one pure Application eligibility policy that permits the existing independent-task physical
   occurrence only when travel progress is complete and its exact plan retains the exact active task
   activity. Reject null, incomplete, distinct-plan, and distinct/value-equal activity inputs.
2. Make `CharacterIndependentTaskOccurrenceController` preflight the exact current travel plan and
   progress before applying the existing atomic task, event-log, world-time, and transcript
   transition. An occurrence before arrival or from stale travel state must change nothing.
3. Add focused Application tests and extend the credential-free Godot occurrence smoke to prove no
   occurrence before arrival, exact one-step final arrival, one atomic eligible occurrence after
   arrival, and stale/mismatched/repeated failure with every surrounding snapshot preserved.
4. Do not yet auto-advance travel in `Main`; choose elapsed-time, speed, interpolation, collision, or
   navigation-agent policy; move or hide a body; rejoin a party member; change dialogue/model
   context; persist task, party, plan, or progress state; or invoke a provider.
