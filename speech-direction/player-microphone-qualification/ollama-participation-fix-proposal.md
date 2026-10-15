# Proposed prerequisite: Ollama ordinary-dialogue participation

Status: concrete runtime repair proposal, awaiting owner authorization on 2026-10-16.
The active microphone trial remains open; this proposal does not reopen accepted speech, controls,
artwork or dialogue rules.

## Observed problem and cause

The owner screenshot shows Whisper enabled, `Dialogue submission: ACCEPTED`, English selected,
`Character response: NONE`, microphone off and reply speech ready. This establishes visible local
acceptance, without proving a listener, participation decision or generated reply. The screenshot
contains no recognized sentence; no image or personal transcript is copied into this package.

The selected local dialogue model is now installed `gemma4:e4b`, and its availability check passed.
An independent source defect still prevents the ordinary Ollama conversation path:

- `game/TemporaryCharacterModelCompositionController.cs`, `TryApplyParticipationBindings`, binds
  participation only for OpenAI. Other providers leave that source unbound or clear its binding.
- `game/TemporaryCharacterTurnParticipationModelSource.cs`, `TakeModel`, consequently returns an
  unavailable participation model for ordinary Ollama operation. That model throws before reply
  generation.
- `game/OrdinaryPlayerDialogueParticipationCoordinator.cs` starts a reply turn only after a completed
  successful participation round; failed rounds are reset without starting a reply.

This is a source-confirmed missing Ollama binding, not a captured exception from the owner's exact
submission. Listener positions remain unknown. Whisper range stays 48 pixels; authored initial
Seeker/Healer separation is 120 pixels, so being beside the intended listener is still required.

## Bounded repair to authorize

Supply an Ollama implementation of the existing `ICharacterTurnParticipationModel`, its request and
bounded native response handling, and a matching binding factory. Reuse the current trusted-context
instructions, closed decision schema and strict request-bound decision validation. Compose this
binding for Ollama in `TryApplyParticipationBindings`, including selection/replacement/clearing.
Keep OpenAI behavior, the current response generator, hearing ranges, valid remain-silent decisions,
explicit Submit/Whisper, editable speech drafts and existing playback unchanged. Do not force NPCs
to reply as a workaround or bypass participation policy.

Named remaining roadmap item: Phase 10, “Test device loss, permission denial, network failure,
feedback loops, accents, and noisy input.” The missing deliverable is a functioning ordinary local
reply prerequisite for the active trial's reply/interruption/feedback cases. This repair is limited
to that blocker; it does not complete the physical qualification item by itself.

Expected implementation is confined to new Ollama participation infrastructure, its focused tests,
current scene composition and a current Main integration fixture. Add current checks without editing
frozen accepted fixtures or receipts. No new UI layout, provider credential access, model download,
model comparison, microphone capture/persistence, domain-rule change or packaged release is included.

## Finite completion and validation

1. Test strict request serialization, exact decision/motive validation, malformed/incomplete/oversized
   native responses, HTTP/transport failure and cancellation using controlled local fixtures.
2. Exercise ordinary Main submission with supplied Ollama HTTP replies: a valid respond decision
   reaches the existing reply generation/text commit; valid silence remains silent; failures do not
   fabricate a reply. Cover fresh binding and provider replacement without OpenAI credentials.
3. Run the selected local `gemma4:e4b` through a non-personal diagnostic request and report the actual
   outcome honestly. Then resume the owner's nearby-listener Whisper trial; do not convert synthetic
   checks into physical passes.
4. Run focused tests, continuation regression probes and the required root `./quality.sh` for product
   changes. Document the outcome and limits, commit and push under standing publication authority.

Complete when the Ollama participation path is wired and verified, the exact local-model outcome is
recorded, and the human trial is ready to resume without this missing-provider prerequisite. Failure
of a live model response remains a reported failure, not permission to broaden model/runtime work.

## Authority required

`CONTINUATION.json` explicitly excludes “runtime fixes” from the current human trial. `AGENTS.md`
requires applicable authority for a genuinely new runtime scope. The owner decision needed is approval
of this bounded Ollama participation repair and its dependent verification/documentation/publication.
