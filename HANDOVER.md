# Allrpcs Handover

## Current state

`main` contains merged PR #226 at `54dec55`. The current branch is
`codex/phase-7-scene-concealment-state` and advances four bounded Phase 7 scene seams:

- `CharacterObjectSabotageConcealmentController` owns exact optional concealment and authoritative
  evidence snapshots, initializes them once, and compare-and-swaps complete Domain apply/recovery
  results without partial state;
- `TemporaryCharacterObjectSabotageSource` authors one isolated exact eligible enemy `conceal`
  candidate, explicit physical access, one authoritative target, empty initial state, and fresh
  noticed non-critical evidence without applying an effect;
- `TemporaryCharacterObjectSabotageFlowController` initializes that exact source state without an
  effect and applies the candidate only on an explicit call, once;
- its recovery path projects a caller-supplied perception snapshot through the existing
  `DiscoveryEvidenceProjection`, rejects unseen evidence, retains authoritative evidence, and
  recovers only the exact target after the evidence becomes noticed.

The three focused checks use isolated `.tscn` scenes, so `Main.tscn` remains unchanged. No behavior
or timing is selected automatically. No existing scene world object, node visibility, position,
perception snapshot, evidence definition, truth, critical route, event, time, transcript, save, or
provider contract is mutated or added.

## Validation and review

- Each slice began with the expected missing-type or missing-operation compilation failure.
- The game project builds with zero warnings and zero errors.
- All three isolated host-local headless Godot checks pass, covering exact one-time initialization,
  stable unrelated entries, stale/value-equal expected snapshot rejection, null and ineligible
  operations, minimal surfaces, exact one-shot apply, perception-projected recovery, retained
  evidence, and atomic preservation on every failure.
- The restricted focused Godot run reproduced the known `user://logs` crash before project code;
  the identical host-local checks pass.
- The cumulative host-local `./scripts/quality.sh` spine passes workflow validation, restore,
  formatting, a zero-warning solution build, 2,276 Domain, 478 Application, 532 Infrastructure,
  and 8 Architecture tests; the scenario validator accepts all 5 evidence definitions, all
  credential-free headless Godot smokes pass, and the Compatibility-renderer graphical launch
  passes on the Apple M3 Max.
- Authored C# remains below 500 lines per file and within 120 characters per line;
  `git diff --check` passes.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7dc: bind authored concealment to exact existing scene world state without applying
it**.

Acceptance boundary:

1. Add one temporary water-cask `WorldObject` at the stable tail of
   `TemporaryWorldObjectSource.WorldObjects`, preserving the exact existing herb-patch and
   stone-marker instances and their order.
2. Change `TemporaryCharacterObjectSabotageSource` to retain that exact water-cask instance and the
   complete exact scene world-object snapshot instead of authoring a separate object or snapshot.
3. Add the temporary sabotage source, concealment controller, and flow controller to `Main.tscn` in
   dependency order. Initialization must retain exact empty source state and must not apply the
   candidate automatically.
4. Add one opt-in main-scene smoke proving exact world-object identity, unchanged existing scene
   objects, initialized empty state, eligible candidate composition, and absence of an automatic
   concealment or evidence effect.
5. Do not call `TryApply` or `TryRecover`, choose behavior or timing, hide or reveal a node, grant
   coordinate-derived access, mutate perception, add a discovery route, events/time/transcripts,
   persistence, another sabotage kind, or a provider.
