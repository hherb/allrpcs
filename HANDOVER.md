# Allrpcs Handover

## Current state

PR #273 is merged into `main` at `0f35540`. Branch
`codex/phase-7-misquotation-evidence-presentation-selection` advances Phase 7 through bounded
truth-neutral presentation selection and inert main-scene ownership:

- `CharacterDialogueMisquotationEvidencePresentationSelection` retains one exact witness view and
  either one exact member evidence entry or explicit abstention. It rejects null views and
  non-member or distinct value-equal evidence without recreating nested dialogue objects;
- `CharacterDialogueMisquotationEvidencePresentationSelectionPolicy` accepts only the selection's
  exact current witness-view instance, so stale or distinct equivalent views fail closed while a
  current abstention remains valid; and
- `CharacterDialogueMisquotationEvidencePresentationSelectionController` accepts at most one
  explicit choice per exact current scene-owned view. Its public selection becomes null as soon as
  that view advances, and a choice for the fresh exact view may then replace the private stale one.
  The main scene owns the controller after its witness-view dependency but never invokes it
  automatically.

Implementation commit `f93fed0` is published in draft PR #274 targeting `main`.

No UI is rendered and no evidence is presented, discovered, corrected, consumed, or persisted.
Ordinary startup and completed selection, abstention, acknowledgement, and offline-failure flows
remain presentation-free. No dialogue, truth, transcript, character, event, audience, world
object, perception, or other authoritative state is mutated. No automatic task, credential or
provider I/O, input, retry, timing, scheduling, navigation, or movement behavior was added. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7jb began with the expected missing-selection build failure. Four focused Domain tests
  prove exact selection and abstention, exact membership, non-member and distinct value-equal
  rejection, current and stale view behavior, nested identity retention, null guards, and closed
  immutable surfaces.
- Phase 7jc began with the expected missing-controller build failure. Its isolated credential-free
  Godot smoke proves inert startup, one explicit selection or abstention per exact view, stale,
  distinct equivalent, non-member, repeated, uninitialized, and null rejection, exact nested
  identity retention, and a closed scene surface.
- Phase 7jd composes only the inert selection owner into `Main.tscn`. The updated main-scene smoke
  proves it remains empty through exact evidence selection, abstention, acknowledgement, and
  offline failure while source dialogue and every authoritative state remain unchanged.
- Session-wide review found and fixed one stale-state exposure: the controller now returns no
  public selection after its exact view advances instead of exposing the prior selection and
  relying on every caller to reapply the policy. No unresolved issue remains.
- The host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,558 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. `git diff --check` also passes.
- Draft PR #274 workflow run `35052433693` failed at account infrastructure before execution: all
  five jobs reported `steps: []`. It provides no hosted product-test or build evidence and was not
  retried unchanged; the complete host-local quality evidence above remains authoritative.

## Exact next bounded slice

Take **Phase 7je: project one accepted selected evidence item into an exact truth-neutral event
pair**.

Acceptance boundary:

1. Add one immutable `CharacterDialogueMisquotationEvidencePresentationSnapshot` retaining the
   exact accepted presentation selection, witness identifier, source dialogue event, and delivered
   misquotation event without copying or formatting their text or declaring either event truthful.
2. Add one pure projection from an exact current witness view and presentation selection. Return no
   presentation for accepted abstention, stale or distinct equivalent views, and otherwise retain
   the selected evidence's exact witness, proposal source event, and delivered event instances.
3. Prove exact selected projection, valid abstention, stale and distinct equivalent rejection,
   nested identity retention, unchanged inputs, null rejection, and closed immutable surfaces in
   focused Domain tests. Do not compose it into Godot; render UI; label either statement true or
   false; discover, correct, consume, or persist evidence; deliver dialogue; change transcripts,
   characters, events, audiences, world state, or perception; or add provider, input, retry,
   timing, scheduling, navigation, or movement behavior.
