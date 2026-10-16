# Herb-patch examination — proposed action execution repair

The owner clarified on 2026-10-16 that ordinary questions still receive answers, while asking
the Healer to examine the herb patch produces neither a visible action nor a reply. This narrows
the latest report to action handling. It does not erase the earlier response failure or prove
which structured intention the model returned during the human trial.

## Confirmed source gaps

- `TemporaryCharacterModelGenerationInputSource` offers the Healer an `ExamineObject` affordance
  for `herb-patch`. Eligibility accepts that intention independently of speech.
- `TemporaryCharacterExaminationFlowController.ProcessExamination` and
  `CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur` both require exact recorded speech.
  A valid silent examination response cannot pass them. The completion controller then completes
  that silent turn; it does not execute its intention.
- The examination flow emits logical examination and safe-collection events. It never commands
  the Healer to walk to the patch. `Healer.TryCommandMoveToDestination` exists, but the ordinary
  dialogue action flow does not call it.
- The temporary flow retains its first examination/collection result, so it is not a reusable
  examination dispatcher. Its smoke fixture supplies a spoken examination response and checks
  logical events. It does not establish silent action execution or physical arrival.

These are source findings, not a reproduction of the exact human submission. The closed response
repair tested generation and turn liveness; its eight turns did not qualify embodied actions.

## Proposed finite successor

Remaining roadmap item: Phase 10 acceptance, “Typed and spoken input produce the same dialogue
events and intention flow, and the game remains fully playable without microphone or speech
services.” Missing deliverable: an accepted herb-patch examination intention reaches the authored
target, executes once through engine authority, and gives visible completion or failure feedback.

Proposed behavior:

1. Consume the exact validated Healer examination intention through an explicit action lifecycle,
   independently of whether the response includes speech. Preserve exact actor, target, response
   and world provenance. Do not manufacture a spoken utterance for a silent response.
2. Use the existing navigation/collision movement to approach the authored herb-patch interaction
   position. Commit examination only after arrival. Show progress, then an engine-owned examination
   result; a failed path or invalidated action must finish with visible feedback and allow later input.
3. Support another examination request after completion. Fence duplicate frame processing and stale
   work after cancellation, replacement, scene exit or load. Keep ordinary dialogue responsive while
   movement is underway, with a defined single-action ownership rule.
4. An examination-only request must not silently harvest the patch. The current temporary flow
   automatically selects safe collection after examination. Isolate that demo behavior from the new
   ordinary examination path; retain its existing test contract in scaffolding. General collection,
   conditional task planning and new inventory behavior are outside this successor.
5. Preserve character autonomy and current hearing/participation decisions. An accepted examination
   intention must execute or fail visibly; this proposal does not force every natural-language request
   to be accepted. Keep existing spoken refusal/wait behavior and silent-wait turn completion.

Expected implementation areas are the Main scene action composition, Healer navigation command
ownership, the temporary examination/completion handoff, narrow domain/application occurrence
contracts where silent action admission requires them, and existing dialogue presentation for action
status. Use a new successor package for evidence; frozen art, herb presentation validators and both
closed dialogue repair receipts remain unchanged. No ASR, speech synthesis, model/provider selection,
prompt-wide forced speech, general action planner or accepted art/layout redesign is included.

Completion requires controlled ordinary-Main cases for spoken and silent examination, actual movement
and arrival before the world event, repeated examination, unreachable/invalid target, cancellation/load,
exactly-once occurrence and continued ordinary replies. Check examine-only does not collect. Preserve
the existing conditional-collection scaffold coverage. Exercise the same submitted text from typed
and ASR draft seams without opening the microphone. Run a fictional local `gemma4:e4b` action request
and report whether it chose the offered action; do not count refusal or silence as executed examination.
Run the full root quality gate for product/CI changes, review, commit and push, then restart the trial
for owner observation. A local-model test is not a physical microphone or echo pass.

## Authority and present state

This document is a reviewable proposal only. No product, tests, CI, settings or live game were changed
for this diagnosis. The active physical slice remains open with three passes, five partial cases and
seven unperformed cases. The proposal does not add another active slice or reopen a closed owner.

`CONTINUATION.json` currently limits the active trial to “No new runtime fixes”. `AGENTS.md` says
“A genuinely new runtime/layout/physical scope still needs the applicable authority.” Approval of this
specific successor is therefore needed before implementing the above action execution changes.
After approval, record the exact grant, defer the incomplete physical slice intact, and select this
one bounded runtime successor. Keep every persistent closure and the unrelated editor rewrite.

Verification here is read-only source tracing plus documentation/continuation checks. Reuse the
published response-repair product gate because product and CI inputs are unchanged.
