# Human microphone trial — in progress

The owner reports good recognition and a working held-Space interaction, then explicitly confirms
that both English and German are understood through Space hold-to-talk. These are qualitative
owner results. Device identity, permission prompts, correction/submission, noise tolerance and
echo safety still need their own outcomes.

[Authority](authority.json) records the enabled trial. [Session](session.json) records current local
runtime provenance and the reported languages and pending selector/microphone/output intake. [Results](results.json)
accounts for 15 finite cases: two basic owner-reported passes and 13 not performed. This slice stays
open. No personal audio or recognized speech has been retained in this package.

## Next operator checks

1. Record selector usage, microphone and headphones/speakers for the successful bilingual trial.
2. English recognition is confirmed. Next observe draft updates during a hold,
   release, edit a word and explicitly Submit. Confirm release alone sends nothing. Brief Space taps
   in the text field should still type spaces.
3. German recognition is confirmed. Next correct a German draft and explicitly Submit with
   Whisper enabled. Switching the selector chooses a separate model; it does not detect language.
4. During separate holds, try Stop mic, Close, language switch and switching to another app. Return,
   release Space and start a fresh hold; old text must not overwrite the new draft.
5. With headphones and Speak replies enabled, hold Space during a reply and confirm interruption.
   Optional Listen requires its own headphone test. Speaker feedback is a separate unperformed case.

Report outcomes and conditions; there is no need to send audio or exact recognized sentences.
Save/load needs a disposable save context. Input-loss, permission-denial and network-setting trials
remain unperformed until their concrete setup is available and any required OS change is enabled.
Do not change privacy settings merely to complete this checklist.

## Verification and limits

Current metadata inspection found macOS 27.0.1 arm64, Python 3.12.3, Moonshine Voice 0.1.5,
sounddevice 0.5.6 and numpy 2.5.4, with every configured local runtime path present. This inspection
opened no microphone and did not measure the earlier trial's route. Session provenance links the
published model manifest and Space verification without re-auditioning or changing those packages.

The existing Space gate passed 211 stages and 5,602 unit tests, with synthetic/native Main coverage.
Its held-key load check used the reset notification seam; actual held-key save/load remains open.
No product/CI input changed for this record, so full quality is reused. JSON parsing, local evidence
bindings, case totals, continuation sync/check and all 11 continuation regression probes passed for
this documentation update. The editor exception hash is unchanged.
