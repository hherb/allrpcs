# Player microphone privacy update — 2026-10-14

[The new Moonshine integration](../speech-direction/player-asr-integration/README.md) adds optional
local microphone input to the closed [privacy baseline](privacy.md) and
[Pocket playback update](voice-privacy-update.md).

Microphone input starts off. **Hold to talk** or **Listen (headphones)** explicitly starts a local
capture process; the OS controls microphone permission. Capture uses the default input device,
mono PCM and a separate configured local Moonshine process. Each session is limited to approximately
30 seconds. Stop, close, focus loss, load, language change, input/device failure or successful Submit
retires capture and recognition. The indicator distinguishes starting, listening, finalizing and
pending retirement; it claims off after retirement finishes.

Ordinary operation writes no microphone WAV or recognition log. PCM and partial/final text are
transient process/UI data. Stop keeps the editable draft, and manual editing fences later automatic
writes. **Submit** is the existing explicit player action: submitted words enter the existing dialogue,
provider, transcript, memory and save pathways disclosed in the baseline. Selecting a local recognizer
does not change the selected conversational model's data flow. Test reports contain only fixed synthetic
review phrases; the qualification fixture never opens the physical microphone.

Model assets load from explicit pinned local paths; there is no automatic model downloader, cloud ASR,
voice-command execution or microphone monitor/output path. OS network blocking and real permission,
device, accent/noise and echo behavior remain unqualified. Listening while replies play is intended
for headphones during prototype trials. Audio activity alone cannot distinguish the player from game
speakers; explicit corrected-text submission remains required.
