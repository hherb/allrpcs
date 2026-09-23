# Allrpcs Handover

## Current state

`main` includes merged PR #325 at `cd07417`. Branch
`codex/phase-7-sabotage-recovery-routing` completes **Phase 7pj** and **Phase 7pk**.

`CharacterSabotageOutcomeRecoveryCoordinator` now routes three retained exact eligibility branches
through their existing recovery owners:

- Phase 7pi concealment retains the exact recovered concealment and authoritative evidence;
- Phase 7pj movement additionally requires the source-owned exact recovery candidate and pre-move
  placement, then retains the exact recovered placement snapshot; and
- Phase 7pk substitution retains the exact selected substitution candidate, then retains the exact
  recovered substitution snapshot.

Every route rechecks the generic author, selected typed result and candidate, branch-owned
acknowledgements, eligibility-owned evidence view, outcome references, and current authority
immediately before one-shot delegation. Separate switch-gated main-scene smokes prove live move and
substitution selections reach their exact recovery transitions once without a second model call;
ordinary startup remains inert and the existing concealment proof is unchanged.

These slices add no Domain recovery rule, recovery consideration/request, autonomous scheduling,
evidence removal, truth rewrite, persistence, provider transport, credentials, input/UI, timing,
navigation, movement control, perception mutation, or dialogue delivery. Contamination,
misquotation, delay, and influence remain unrouted. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both new targeted main-scene smokes pass host-local with Godot 4.6.3 Mono. The first sandboxed
  move launch hit the known `user://logs` permission crash; the identical host-local run passed.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covered exact author/result/candidate provenance, branch acknowledgements,
  current authority, evidence gates, source-placement identity, one-shot behavior, partial-mutation
  safety, evidence preservation, scene order, ordinary-startup inertness, public-surface
  containment, documentation, file size, formatting, and whitespace. No further defect was found.

## Exact next bounded slice

Take **Phase 7pl: route the retained exact contamination eligibility through its existing recovery
transition**.

Acceptance boundary:

1. Test-first extend the recovery coordinator with one strongly typed contamination route accepting
   only Phase 7ph's exact retained author request, `CharacterObjectSabotageEvidence` candidate, and
   evidence-view tokens.
2. Recheck the exact generic `Contaminate` selection, typed result and candidate, branch-owned
   acknowledgement snapshots, Phase 7pg contamination/evidence outcome, and still-current
   contamination controller authority immediately before delegation; do not rebuild a token.
3. Delegate once to `CharacterObjectSabotageContaminationController.TryRecover`, retain only the
   exact recovered contamination snapshot, and preserve authoritative evidence and every source.
4. Null, rebuilt, replaced, foreign, wrong-branch, distinct value-equal evidence-view, stale,
   ineligible, non-current, and repeated paths must fail closed without consuming a model or
   changing contamination, evidence, source, presentation, world-object, or other branch state.
5. Add a separately switch-gated main-scene contamination path proving ordinary startup remains
   inert and one exact live contamination selection reaches eligibility and recovery once. Do not
   weaken or replace the concealment, move, or substitution proofs.
6. Do not route misquotation, delay, or influence; start recovery consideration; remove evidence;
   schedule sabotage; persist state; or add provider, UI/input, timing, navigation,
   movement-control, perception-mutation, dialogue-delivery, or new Domain behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to route exact eligible misquotation exposure through its existing evidence-view route.
