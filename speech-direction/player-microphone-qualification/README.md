# Human microphone trial — in progress

The owner reports good recognition and a working held-Space interaction, then explicitly confirms
that both English and German are understood through Space hold-to-talk. These are qualitative
owner results. The owner also confirms editing a recognized word before explicit Submit and
Stop mic during a hold. The owner identifies the MacBook Pro built-in microphone and built-in
speakers, with both languages tried in a slightly noisy room. The resumed trial confirms live draft
updates in both English and German. The owner confirms release stops listening and leaves the draft
editable and unsent. Brief Space taps insert normal spaces without starting the microphone.
OS permission identity, remaining lifecycle cases, quiet/accent comparisons and echo safety still
need their own outcomes. The attempted Whisper/Submit check clears the draft but produces no
observed response; end-to-end Whisper delivery remains inconclusive.

[Authority](authority.json) records the enabled trial. [Session](session.json) records current local
runtime provenance, reported languages, built-in input/output and room conditions. Selector usage
and OS permission host intake remain incomplete. [Results](results.json)
accounts for 15 finite cases: three complete qualitative passes (basic bilingual recognition, held
Space interaction and tap/release behavior), five partially completed cases (device/route, English
live updates and correction/Submit, German live updates, Stop and slightly noisy recognition
subchecks), and seven not performed. This slice stays open. No personal audio or recognized speech
has been retained in this package.

## Next operator checks

1. Record selector usage and the OS microphone permission host, if known. Built-in microphone,
   built-in speakers and slightly noisy room conditions are already confirmed.
2. Correction before Submit and Stop mic during a hold are confirmed; do not repeat those checks.
   Live updates in English and German and release stopping listening with an editable, unsent draft
   are also confirmed. Brief taps insert normal spaces without opening capture. Language-specific
   final text observations remain unreported. The language used for the successful correction/Submit
   check was not specified.
3. The Whisper/Submit attempt remains inconclusive. Read-only follow-up on 2026-10-16 found saved
   provider Ollama/model `gemma3`, while the reachable local Ollama model inventory contains neither
   `gemma3` nor `gemma3:latest`. The owner chose installed `gemma4:e4b`. Saved settings were updated
   atomically with other provider settings preserved, the local model-availability check returned
   HTTP 200, and the owner-trial game restarted. This matches the existing connection tester's
   `/api/show` check; it establishes model availability, without generating a reply.
   Main's authored initial Seeker/Healer positions are 120 pixels apart, outside the 48-pixel Whisper
   range. Current positions are unknown. In the restarted session, move beside the Healer and record
   submission/response statuses for the Whisper check. Clearing follows local acceptance;
   hearing and reply/provider outcomes remain separate. No previous result is promoted to a pass.
4. Remaining cancellation checks are Close, language switch and switching to another app. Return,
   release Space and start a fresh hold; old text must not overwrite the new draft.
5. With headphones and Speak replies enabled, hold Space during a reply and confirm interruption.
   Optional Listen requires its own headphone test. Speaker feedback is a separate unperformed case:
   using speakers during recognition does not establish behavior during overlapping reply audio.

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
