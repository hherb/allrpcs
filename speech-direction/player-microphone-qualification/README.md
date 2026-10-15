# Human microphone trial — in progress

The owner reports good recognition and a working held-Space interaction, then explicitly confirms
that both English and German are understood through Space hold-to-talk. These are qualitative
owner results. The owner also confirms editing a recognized word before explicit Submit and
Stop mic during a hold. The owner identifies the MacBook Pro built-in microphone and built-in
speakers, with both languages tried in a slightly noisy room. The resumed trial confirms live draft
updates in both English and German. OS permission identity, remaining lifecycle cases, quiet/accent
comparisons and echo safety still need their own outcomes.

[Authority](authority.json) records the enabled trial. [Session](session.json) records current local
runtime provenance, reported languages, built-in input/output and room conditions. Selector usage
and OS permission host intake remain incomplete. [Results](results.json)
accounts for 15 finite cases: two complete qualitative passes, five partially completed cases
(device/route, English live updates and correction/Submit, German live updates, Stop and slightly
noisy recognition subchecks), and eight not performed. This slice stays open. No personal audio or
recognized speech has been retained in this package.

## Next operator checks

1. Record selector usage and the OS microphone permission host, if known. Built-in microphone,
   built-in speakers and slightly noisy room conditions are already confirmed.
2. Correction before Submit and Stop mic during a hold are confirmed; do not repeat those checks.
   Live updates in English and German are also confirmed. Final text after release, brief Space taps
   and release without automatic submission remain unreported. The language used for the successful
   correction/Submit check was not specified.
3. Whisper-specific submission remains unreported. Both languages are already recognized; the
   selector chooses a separate model and does not detect language automatically.
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
