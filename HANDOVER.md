# Allrpcs Handover

## Current state

`main` includes merged PR #324 at `fcdf3c1`. Branch
`codex/phase-7-sabotage-outcome-eligibility` completes **Phase 7ph** and **Phase 7pi**.
Implementation commit `65079ce` is published in draft PR #325 targeting `main`.

Phase 7ph adds `CharacterSabotageOutcomeEligibilityCoordinator`, a one-shot, compile-time-typed
eligibility seam after Phase 7pg. It accepts only the exact retained generic author request,
selected typed result and candidate, branch-owned acknowledgement tokens, and still-current
authority:

- concealment, movement, substitution, and contamination delegate unchanged eligibility to their
  existing evidence-gated recovery policies and retain the exact caller-supplied evidence view;
- delay and influence accept only the exact applied result already admitted by their existing
  one-shot recovery-candidate controller; and
- misquotation retains only the existing exact exposed-evidence and witness-view tokens.

Phase 7pi adds `CharacterSabotageOutcomeRecoveryCoordinator`. Its first bounded route accepts only
Phase 7ph's exact concealment author, candidate, and evidence-view tokens while the selected result,
branch acknowledgement, concealment, and evidence remain the same instances. It then delegates
once to the existing `CharacterObjectSabotageConcealmentController.TryRecover` transition, retains
the exact recovered concealment snapshot, and preserves authoritative evidence.

`Main.tscn` dependency-orders both coordinators after Phase 7pg. The existing switch-gated
main-scene smoke now proves ordinary startup inertness; pre-acknowledgement, null, rebuilt,
invisible-evidence, distinct value-equal evidence-view, and repeated rejection; exact eligibility;
and exact one-shot concealment recovery without a second model call.

These slices add no new Domain recovery rule, recovery consideration/request, autonomous
scheduling, evidence removal, truth rewrite, persistence, provider transport, credentials,
input/UI, timing, navigation, movement, perception mutation, or dialogue delivery. Phase 7pi does
not yet route movement, substitution, contamination, misquotation, delay, or influence. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first evidence: the game build failed with `CS0246` first for the absent
  `CharacterSabotageOutcomeEligibilityCoordinator`, then for the absent
  `CharacterSabotageOutcomeRecoveryCoordinator`, before each production seam was added.
- The final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build,
  2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- The initial sandboxed quality run reached the zero-warning build and then hit the known VSTest
  loopback `SocketException (13): Permission denied`; the identical host-local run is the passing
  product evidence.
- Session-wide review covered all seven typed eligibility branches, exact author/result/candidate
  provenance, branch acknowledgement tokens, current authority, evidence gates, existing recovery
  candidate sources, one-shot behavior, partial-mutation safety, concealment recovery delegation,
  evidence preservation, scene order, ordinary-startup inertness, public-surface containment,
  documentation, file size, formatting, and whitespace.
- Review found and fixed one test-coverage gap: Phase 7pi's exact evidence-view token and lack of a
  second model call were enforced by production code but not explicitly asserted by the smoke. The
  smoke now rejects a distinct value-equal view and verifies both model call counts stay at one.
  Formatting, a zero-warning build, and the exact main-scene smoke pass after that review fix.
- Draft PR #325's Pure C#, Headless Godot, and three unsigned development-build jobs all failed
  with `steps: []`; no checkout, build, test, smoke, or export step ran. These hosted checks are
  Actions/account infrastructure state and not hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7pj: route the retained exact move eligibility through its existing recovery
transition**.

Acceptance boundary:

1. Test-first extend the explicit recovery coordinator with one strongly typed move route accepting
   only Phase 7ph's exact retained author request, `CharacterObjectSabotageMoveRecoveryCandidate`,
   and evidence-view tokens.
2. Recheck the exact generic `Move` selection, typed result and candidate, branch-owned
   acknowledgement snapshots, Phase 7pg placements/evidence outcome, and still-current move
   controller authority immediately before delegation. Require the existing recovery candidate's
   exact pre-move source placement; do not rebuild it.
3. Delegate once to the existing `CharacterObjectSabotageMoveController.TryRecover` transition and
   retain only the exact recovered placement snapshot. Preserve authoritative evidence and every
   source object.
4. Null, rebuilt, replaced, foreign, wrong-branch, distinct value-equal evidence-view, stale,
   ineligible, non-current, and repeated paths must fail closed without consuming a model or
   changing placement, evidence, source, presentation, world-object, or other branch state.
5. Add a separately switch-gated main-scene move path proving ordinary startup remains inert and
   one exact live move selection reaches eligibility and recovery once. Do not weaken or replace the
   existing concealment proof.
6. Do not route substitution, contamination, misquotation, delay, or influence; start recovery
   consideration; remove evidence; schedule sabotage; persist state; or add provider, UI/input,
   timing, navigation, movement-control, perception-mutation, dialogue-delivery, or new Domain
   behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to route the exact eligible substitution branch through its existing recovery transition.
