# Allrpcs Handover

## Current state

PR #274 is merged into `main` at `09caa51`. Branch
`codex/phase-7-misquotation-evidence-presentation-snapshot` advances Phase 7 through bounded
truth-neutral presentation projection and inert main-scene ownership:

- `CharacterDialogueMisquotationEvidencePresentationSnapshot` retains one exact accepted
  selection, witness identifier, source dialogue event, and delivered misquotation event without
  copying or formatting text or declaring either event truthful;
- `CharacterDialogueMisquotationEvidencePresentationProjection` returns that snapshot only for a
  selected exact current witness view. Accepted abstention and stale or distinct equivalent views
  return no presentation; and
- `CharacterDialogueMisquotationEvidencePresentationController` processes each exact current
  selection at most once, retains abstention without fabricating a snapshot, and hides a selected
  snapshot as soon as its exact selection or view is no longer current. The main scene owns this
  controller after its view and selection dependencies but never invokes it automatically.

No text formatting or UI was added. No evidence is discovered, corrected, consumed, or persisted.
No dialogue, truth, transcript, character, event, audience, world object, perception, or other
authoritative state is mutated. No automatic task, credential or provider I/O, input, retry,
timing, scheduling, navigation, or movement behavior was added. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7je began with the expected missing-snapshot/projection build failure. Four focused Domain
  tests prove exact selected projection, valid abstention, stale and distinct equivalent rejection,
  exact nested identity retention, unchanged inputs, null rejection, and closed immutable surfaces.
- Phase 7jf began with the expected missing-controller build failure. Its isolated credential-free
  Godot smoke proves inert startup, exact selected projection, explicit abstention, stale and
  distinct equivalent rejection, stale hiding, exact nested identity retention, null guards, and a
  closed scene surface.
- Phase 7jg began with the expected missing-main-node smoke failure. The updated main-scene smoke
  proves ordinary startup, evidence selection, abstention, acknowledgement, and offline failure do
  not automatically project presentation or change source dialogue evidence.
- The focused Domain tests pass 4/4, the focused game build succeeds with zero warnings, and both
  focused Godot smokes pass on the host. The sandbox-only VSTest loopback denial and Godot
  `user://logs` crash were retried unchanged on the host.
- Session-wide review verified that the public presentation fails closed as soon as either its
  exact selection or view becomes stale, that accepted abstention is recorded privately without a
  fabricated snapshot, and that dependency order is explicit in both isolated and main scenes. No
  unresolved issue remains.
- The host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. `dotnet format Allrpcs.sln --no-restore --verify-no-changes
  --verbosity minimal` and `git diff --check` also pass.

## Exact next bounded slice

Take **Phase 7jh: explicitly orchestrate one exact witness presentation choice**.

Acceptance boundary:

1. Add one isolated `TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController`
   that accepts an explicit exact member or `null` abstention for the scene-owned current witness
   view, delegates the existing selection owner, and then delegates the exact resulting selection
   to the existing presentation owner.
2. Prove inert startup, one explicit selected presentation or abstention per exact view, exact
   nested witness/evidence/event identity, stale, distinct equivalent, non-member, repeated, and
   null-view rejection, unchanged source state, and a closed surface in one isolated
   credential-free Godot smoke.
3. Do not compose this flow into `Main.tscn`; automatically choose or present evidence; render or
   format UI; label either statement true or false; discover, correct, consume, or persist evidence;
   deliver dialogue; change transcripts, characters, events, audiences, world state, or perception;
   or add provider, input, retry, timing, scheduling, navigation, or movement behavior.
