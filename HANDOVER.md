# Allrpcs Handover

## Current state

`main` contains merged PR #225 at `eeb97ef`. The current branch is
`codex/phase-7-recoverable-concealment`, with two reviewed Phase 7 commits after that merge:

- `1a02faf` applies one eligible `conceal` occurrence together with its exact fresh evidence;
- `dc85737` recovers only that concealed target through visible exact authoritative evidence.

Draft PR #226 is open and mergeable. Hosted run `34760675229` completed with all five jobs at
`steps: []`; it provides no product-test or unsigned-build evidence and should not be retried
unchanged.

Phase 7 now has a bounded recoverable concealment loop in engine-independent Domain code:

- `WorldObjectConcealmentSnapshot` copies at most 64 value-unique exact `ObjectId` instances in
  stable order; absence means not concealed;
- `CharacterObjectSabotageConcealmentPolicy` accepts only a completely eligible evidence-bound
  `conceal` candidate whose target is not already concealed;
- its transition returns fresh concealment and evidence snapshots as one immutable result, retaining
  the exact target identifier, candidate, evidence definition, and unrelated entries;
- `CharacterObjectSabotageConcealmentRecoveryPolicy` requires that target to remain concealed, the
  candidate's exact target-bound evidence definition to remain authoritative, and its identifier to
  be visible in one character evidence view;
- its transition removes only the value-matching target into a fresh concealment snapshot and
  retains the exact candidate, evidence view, authoritative evidence, and unrelated concealment.

No authoritative `WorldObject`, position, perception, evidence definition, truth, or critical route
is mutated or removed. No other sabotage kind, evidence consumption, discovery route, world event,
time, transcript, live enemy behavior, scene wiring, save migration, or provider contract was added.

## Validation and review

- Both slices began with focused missing-type compilation failures.
- All 14 focused concealment and recovery Domain tests pass host-local.
- The cumulative host-local `./scripts/quality.sh` spine passes workflow validation, restore,
  formatting, a zero-warning solution build, 2,276 Domain, 478 Application, 532 Infrastructure, and
  8 Architecture tests.
- The scenario validator accepts all 5 evidence definitions.
- The complete credential-free headless Godot smoke suite passes, and the Compatibility-renderer
  graphical launch passes on the Apple M3 Max.
- Hosted run `34760675229` executed no job steps; its five failures are Actions/account
  infrastructure state, not product-test results.
- Restricted VSTest reproduced the known loopback `SocketException (13)`; identical focused and
  complete validation passes host-local.
- Staged diffs were reviewed, `git diff --cached --check` passed, and authored code stays within the
  practical 120-character limit.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7cy: scene-own recoverable concealment state without authoring an occurrence**.

Acceptance boundary:

1. Add one Godot controller that owns optional `WorldObjectConcealmentSnapshot` and
   `DiscoveryEvidenceSnapshot` values and initializes both exactly once from caller-owned inputs.
2. Add one compare-and-swap `TryApply` operation. Require exact expected current snapshots, delegate
   the complete Domain concealment transition, and install both fresh resulting snapshots only
   after success.
3. Add one compare-and-swap `TryRecover` operation. Require exact expected current snapshots,
   delegate the complete Domain recovery transition with the caller's exact evidence view and
   candidate, and install only its fresh concealment snapshot after success while retaining the
   exact authoritative evidence snapshot.
4. Add one focused headless smoke covering initialization, exact apply, exact recovery, stable
   unrelated entries, stale/value-equal expected snapshot rejection, ineligible operations, nulls,
   minimal surface, and atomic preservation of both states on every failure.
5. Do not author or select a sabotage candidate, change `Main.tscn`, hide or reveal a node, mutate
   world objects or perception, prove a discovery route, add events/time/transcripts, handle another
   sabotage kind, persist state, or invoke a provider.
