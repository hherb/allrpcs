# Voice privacy update — 2026-10-13

The [baseline privacy information](privacy.md) describes the development source before optional
Pocket reply speech was composed into Main. The [new integration](../speech-direction/dialogue-integration/README.md)
adds the following behavior; provider traffic, saved dialogue authority and credential storage are
unchanged.

**Speak replies** starts disabled for each session. Enabling it passes an already committed,
audible character reply to the configured local Pocket process, which produces audio for the
existing Speech mixer channel. English/German selection is explicit. Reply text is not sent to a
new cloud speech service, and ordinary playback writes no WAV or speech transcript file. Existing
game dialogue/memory saving can still retain those words as described in the baseline disclosure.
The developer's explicit test mode records only fixed review phrases and native mixer output.

Stop, new input/reply, disable, language change, load, focus loss and applicable audio/range failures
cancel obsolete generation and flush queued playback. Text remains with the existing dialogue
presentation/transcript owners. The local process uses the existing staged-asset/offline-loading
service; OS network isolation and future packaging remain separately unverified.

No microphone is opened by this integration. The ASR comparison uses existing synthetic speech,
noise and silence, not player recordings, and there is no game recognizer yet. A future microphone
feature must require explicit player enablement, show listening state, keep transcripts editable,
reject stale updates and preserve explicit Submit. Voice activation also needs a real echo-control
or headphones boundary; VAD alone does not distinguish game speech from the player.
