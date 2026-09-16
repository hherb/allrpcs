# Allrpcs Handover

## Current state

PR #272 is merged into `main` at `6a591a1`. Branch
`codex/phase-7-misquotation-evidence-view` advances Phase 7 through bounded exact-witness evidence
views and inert scene ownership:

- `CharacterDialogueMisquotationEvidenceViewSnapshot` retains one exact witness and defensively
  copies at most 64 exact entries in stable exposure order. It rejects nulls, non-identical
  witnesses, and duplicate exact delivered events while allowing distinct value-equal deliveries;
- `CharacterDialogueMisquotationEvidenceViewProjection` filters an exposure snapshot only by exact
  witness-identifier reference, retaining exact entry, occurrence, proposal, and event identities;
- `CharacterDialogueMisquotationEvidenceViewController` initializes once and refreshes only from
  its exact expected source exposure, rejecting stale and same-snapshot changes; and
- the exposure coordinator advances the exact view with successful selection, preserves it for
  abstention and failures, and acknowledges only while exposure and view remain aligned. The main
  scene initializes the representative witness's empty view without starting work.

No evidence is presented, discovered, corrected, consumed, or persisted. Ordinary startup remains
inert. No dialogue, truth, transcript, character, event, audience, world object, perception, or
other authoritative state is mutated. No automatic task, credential or provider I/O, input, retry,
timing, scheduling, navigation, or movement behavior was added. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7iz began with the expected missing-view build failure. Focused Domain coverage proves
  exact witness filtering, empty views, stable order, defensive copying, the 64-entry bound, null
  and duplicate rejection, distinct value-equal delivery allowance, mismatched and value-equal
  witness rejection, nested-reference retention, and closed immutable surfaces.
- Phase 7ja began with the expected missing-controller build failure. Its isolated credential-free
  Godot smoke proves one-time initialization, exact exposure CAS refresh, stale and same-snapshot
  rejection, exact witness filtering, nested-reference retention, null guards, and a closed scene
  surface. The updated coordinator and main-scene smokes prove aligned view refresh, inert startup,
  selection, abstention, acknowledgement, cancellation, failure, duplicate, capacity, and stale
  behavior without changing source dialogue.
- Session-wide review tightened the public view invariant so direct construction also rejects
  evidence belonging to another or distinct value-equal witness, and extracted the smoke scenario
  helper to keep authored source files within the practical size limit. No unresolved issue remains.
- The host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,554 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. `git diff --check` also passes. Draft PR status will be recorded
  here after publication.

## Exact next bounded slice

Take **Phase 7jb: define one explicit truth-neutral presentation selection from an exact witness
view**.

Acceptance boundary:

1. Add one immutable presentation selection that retains the exact witness view and either one
   exact member evidence entry or explicit abstention; reject nulls and non-member or distinct
   value-equal evidence without recreating any nested dialogue object.
2. Add one pure acceptance policy requiring the selection's exact witness-view identity to be the
   supplied exact current view, so stale or distinct value-equal views fail closed while a current
   abstention remains valid.
3. Prove selection, abstention, stale view, exact membership, distinct value-equal rejection,
   nested identity retention, null rejection, and closed immutable surfaces in focused Domain
   tests. Do not compose it into Godot; render UI; present, discover, correct, consume, or persist
   evidence; deliver dialogue; change truth, transcripts, characters, events, audiences, world
   state, or perception; or add provider, input, retry, timing, scheduling, navigation, or movement
   behavior.
