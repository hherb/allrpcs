# Allrpcs Handover

## Current state

`main` includes merged PR #290 at `81e3091`. Branch
`codex/phase-7-influence-recovery-selection` advances the recoverable influence path through four
bounded Domain slices:

- Phase 7lb adds `CharacterTargetSabotageInfluenceRecoverySelection`, which fully materializes and
  validates at most 64 candidates before returning the exact first currently eligible candidate in
  stable caller order.
- Phase 7lc adds `CharacterTargetSabotageInfluenceRecoveryConsiderationRequest`, which retains one
  exact deciding character, exact current relationship, and a defensive bounded copy of exact
  candidates without judging or applying them.
- Phase 7ld adds an exact selection-or-abstention decision, acceptance policy, and immutable result.
  Acceptance requires the exact relationship owner and an exact eligible request member.
- Phase 7le adds `CharacterTargetSabotageInfluenceRecoveryConsiderationTransition`, which leaves an
  accepted abstention inert and delegates an accepted selection through the existing recovery
  transition to fresh numerically restored relationship state.

Session review found and fixed one authority gap: an exact request actor that did not own the
directional relationship could initially select recovery. The policy now requires exact
relationship-owner identity before accepting either a selection or abstention, with regression
coverage.

No slice authors recovery candidates or evidence, starts a model, owns background work, changes
scene relationship authority, wires recovery into `Main.tscn`, persists relationships, or adds
input/UI, elapsed time, navigation, movement, perception, dialogue, or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused checks failed for each missing contract before implementation, then passed.
- The combined influence-recovery set passes all 32 focused Domain tests after the authority fix.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 551 Application tests, 959 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers complete one-pass validation, the 64-candidate cap, null and exact
  duplicate rejection, stable caller order, exact-member selection, distinct equivalent inputs,
  exact relationship ownership, stale eligibility, abstention, immutable result binding, recovery
  application, source preservation, public surfaces, file size, line length, documentation, and
  whitespace. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lf: obtain and validate one provider-neutral influence-recovery decision**.

Acceptance boundary:

1. Test-first add one Application interface that accepts the exact
   `CharacterTargetSabotageInfluenceRecoveryConsiderationRequest` plus caller cancellation and
   returns one `CharacterTargetSabotageInfluenceRecoveryDecision`.
2. Add one pure orchestration controller that rejects nulls and pre-cancellation, forwards the exact
   request and cancellation token to the model exactly once, and constructs the existing Domain
   consideration result so wrong-owner, foreign, stale, or otherwise invalid decisions fail closed.
3. Prove exact selection and abstention, exact request/token forwarding, caller cancellation,
   invalid-decision rejection, and unchanged request, relationship, candidate, and reason instances.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add a
   model binding or source, author recovery candidates or evidence, apply recovery in scene state,
   own a task, decode provider output, send HTTP, wire `Main.tscn`, persist relationships, or add
   input/UI, elapsed time, navigation, movement, perception, dialogue, or other world behavior.
