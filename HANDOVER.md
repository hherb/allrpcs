# Allrpcs Handover

## Current state

`main` contains merged PR #208 at `2decced`. The current branch is
`codex/phase-7-participation-round-results` at `841d708`.

This branch adds three bounded Phase 7 slices:

- `CharacterTurnParticipationConsiderationRoundResult` retains copied read-only sequences of 0–32
  exact requests and exact accepted results in stable order, requiring positional request-instance
  alignment, distinct scheduled character identifiers, and one shared exact heard event;
- `ICharacterTurnParticipationModel` defines one provider-neutral, cancellation-aware decision
  operation over an exact consideration request;
- `CharacterTurnParticipationConsiderationController` delegates complete request-round preflight to
  the existing Domain transition before invoking that interface once per request in stable order,
  then returns only the validated complete result round.

The Phase 7 parent items remain open. This branch does not assemble or serialize participation
model context, implement a provider adapter, add retries or timeouts, select a speaker from the new
result round, mutate an autonomous budget, construct or start a follow-up, initialize or loop an
ordinary autonomous chain, wire the scene, change hearing or roles, add whispering/private
conversations or independent tasks, implement sabotage, start Phase 8, release, sign, or distribute.

Draft PR #209 contains this work.

## Validation and review

- test-first compile failures were observed before the missing round result, model interface, and
  controller implementations;
- 13 focused cases pass across the three new surfaces, covering empty, single, and maximum rounds;
  stable order; exact and distinct value-equal request identity; missing, extra, reordered,
  duplicate-character, mixed-event, oversized, null, and invalid model outputs; complete preflight;
  cancellation propagation; unchanged inputs; collection immutability; and smallest public
  surfaces;
- final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, the
  zero-warning solution build, 2,087 Domain, 409 Application, 447 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix, and the
  bounded Compatibility-renderer launch;
- `git diff --check`, practical 500-line/120-character limits, exact-identity and value-membership
  boundaries, complete preflight, immutable collection, network-free behavior, and scope review
  pass.

Restricted execution reproduced the known local-IPC denial in MSBuild/VSTest; the identical focused
commands and full quality spine passed host-local. Draft PR #209 triggered run `34685293250`; all
five jobs completed with `steps: []` without executing checkout or tests under the continuing
Actions account-budget condition. Treat that as infrastructure rather than test evidence and do not
retry or dispatch workflows.

## Exact next bounded slice

Take **Phase 7z: budget-gated selection from a complete consideration result round**.

Acceptance boundary:

1. Add one pure Domain policy accepting an exact `AutonomousConversationBudget` and exact
   `CharacterTurnParticipationConsiderationRoundResult`.
2. Delegate speaking-decision choice to the existing selection policy and, only while the budget
   permits continuation, return the exact aligned consideration result for that decision; return
   no selection for an empty/all-silent round or an exhausted/interrupted budget.
3. Add focused Domain tests for response and interjection selection, stable first-speaker order,
   exact result/decision identity, empty and all-silent rounds, exhausted and interrupted budgets,
   nulls, unchanged inputs, and smallest public surface.
4. Do not invoke a model, rebuild or revalidate a completed round, mutate the budget, create/start/
   complete a follow-up turn, initialize or loop an autonomous chain, or wire the scene.

Leave both Phase 7 parent items unticked until later slices adapt provider models and drive every
ordinary autonomous chain through the existing bounded scene seams.
