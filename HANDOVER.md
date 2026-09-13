# Allrpcs Handover

## Current state

`main` contains merged PR #224 at `39170f0`. The current branch is
`codex/phase-7-sabotage-proposals`, with four reviewed Phase 7 commits after that merge.

Draft PR #225 is open and mergeable. Hosted run `34759529279` completed with all five jobs at
`steps: []`; it provides no product-test or unsigned-build evidence and should not be retried
unchanged.

Phase 7 now carries object-targeted sabotage from a closed kind through exact access, authoritative
target, and evidence-only state:

- `CharacterObjectSabotageProposal` retains one exact actor and action-access requirement, and
  accepts only `conceal`, `move`, `substitute`, or `contaminate`;
- its pure eligibility policy requires a value-matching actor and delegates the complete unchanged
  physical, capability, and knowledge gate;
- `CharacterObjectSabotageOccurrence` retains one exact proposal and target, and its policy requires
  the proposal target to match by value, the exact world object to be authoritative, and the complete
  proposal to remain eligible;
- `CharacterObjectSabotageEvidence` binds that occurrence candidate to one exact discovery-evidence
  definition, whose identifier must be fresh and whose object must match the occurrence target;
- `CharacterObjectSabotageEvidenceTransition` returns a fresh stable snapshot with that exact
  evidence appended only after the complete evidence-bound occurrence policy succeeds.

`misquote`, `delay`, and `influence` remain deliberately unmodeled as object targets. No sabotage
effect, world-object replacement, concealment or placement state, ingredient mutation, world event,
time, transcript, live enemy behavior, scene wiring, save migration, or provider contract was added.
The evidence transition does not itself prove that an authored discovery route can reveal the new
evidence.

## Validation and review

- Each slice began with a focused missing-type compilation failure.
- All 24 focused object-sabotage Domain tests pass host-local.
- The complete host-local `./scripts/quality.sh` spine passes workflow validation, restore,
  formatting, a zero-warning solution build, 2,262 Domain, 478 Application, 532 Infrastructure, and
  8 Architecture tests.
- The scenario validator accepts all 5 evidence definitions.
- The complete credential-free headless Godot smoke suite passes, and the Compatibility-renderer
  graphical launch passes on the Apple M3 Max.
- Hosted run `34759529279` executed no job steps; its five failures are Actions/account
  infrastructure state, not product-test results.
- Restricted VSTest reproduced the known loopback `SocketException (13)`; the identical focused
  tests and complete quality spine pass host-local.
- Cumulative `git diff --check` and the practical 120-character authored-code check pass.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7cw: apply the first recoverable concealment effect atomically with exact sabotage
evidence**.

Acceptance boundary:

1. Add one bounded immutable world-object concealment snapshot whose value-unique entries retain
   exact `ObjectId` instances; absence means not concealed.
2. Add one pure policy for a `conceal` evidence-bound candidate only. Require complete existing
   evidence eligibility, require the target not already be concealed, and reject `move`,
   `substitute`, or `contaminate` without changing any input.
3. Add one immutable result and transition that return both a fresh concealment snapshot containing
   the exact target and the existing fresh evidence result as one all-or-nothing operation.
4. Add focused Domain tests for exact retention, stable unrelated entries, value-equal duplicate
   concealment, repeated concealment, wrong kind, stale evidence identity, missing access, nulls,
   failure atomicity, and unchanged inputs.
5. Do not change the authoritative `WorldObject`, model position or movement, remove an object or
   critical route, implement reveal/recovery yet, apply substitution or contamination, record world
   events/time/transcripts, author live enemy access, add scene/provider wiring, or persist state.
