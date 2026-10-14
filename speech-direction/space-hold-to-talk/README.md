# Tap Space to type; hold Space to speak

The owner's 2026-10-14 request replaces button-focus-dependent keyboard speaking with a game-wide
Space tap/hold gesture. [Authority](authority.json) records the exact request and explicit reopening.
The original [Moonshine integration](../player-asr-integration/README.md) and all frozen receipts remain
historical evidence; their hashes and validators are unchanged.

## Interaction

Hold plain **Space for 300 ms** to start voice from the world, a text field or another game control.
The dialogue draft opens automatically and is brought in front of existing reading views. No voice
mode toggle or focus on the talk button is required. Wait for **Listening** before speaking: local
recognizer startup still precedes microphone capture. Release Space to finish capture and finalize
an editable draft. Review/correct the words and use the existing explicit **Submit** and **Whisper**.

A short tap retains native Space behavior, including text/caret/selection editing and ordinary GUI
activation. A brief tap with the talk button focused does not open capture. Mouse hold-to-talk and
optional listening remain available. English/German still use the existing language selector.

Holding consumes the initial Space and its OS repeats, so it inserts no spaces or activates a focused
Submit/Close control. Normal keyboard and pointer events overlapping the initial decision are replayed
in order, through its original viewport. Game-owned popups use a small input router too;
holding dismisses the active popup without choosing a menu item. A tapped space is committed on release, so its text insertion is slightly delayed. Long holds
stop reply playback at the hold threshold; partial/final recognition never submits anything.

Stop, Close, successful load, language changes or application focus loss cancel the current gesture.
An outstanding key must be released before another hold can start. Switching to another app stops
capture; this is a shortcut within the game, not an OS-global microphone listener. Modifier shortcuts
such as Command-Space retain their native path. Existing reading views retain their draft state when
the dialogue review is brought forward.

## Implementation

`TapHoldGesture` is a clock-driven application helper with a single press, exact threshold, one start
and release/cancellation fencing. `SpaceHoldSpeechController` intercepts plain Space before GUI
activation, bounds queued events to 64, coalesces mouse motion and distinguishes native replay events
with a private event marker. It replays through the receiving viewport's ordinary input route, leaving
text editing and existing button actions to Godot. No project input map/editor rewrite is required.

The controller invokes new internal hold/finalize entry points on the existing player speech owner.
Its content-free stop generation prevents a cancelled hold from reopening input. The unchanged
provider/PCM boundaries, model hashes, language selection, draft correction, reply sink and explicit
submission owner remain in use. Application focus loss stops capture, while focus moving between
game-owned viewports is kept separate. Missing runtime/permission/device paths retain typed fallback.

## Verification

[Verification](verification.json) binds final sources, current quality, Main reports, screenshots and
exact synthetic English/German WAV inputs from the published ASR package. The final root gate
passed 211 stages and 5,602 unit tests; all six retained Main modes passed. No qualification fixture
opens a physical microphone or retains personal speech. Supplied dialogue models and synthetic reply
PCM avoid conversational-provider credentials.

Tests cover native tapped spaces, selected-text replacement, overlapping typing order, ordinary GUI
activation, holds across six focus owners and closed dialogue/another reading view, echo suppression,
focus changes during capture, release finality, no automatic submission, pending/active cancellation,
the successful-load reset notification seam and scene retirement. The supplied dialogue fixture
state cannot capture a full recovery save root; its reset notification is injected. Real restore roots
are qualified separately by the full gate, so this seam test is not an end-to-end held-key save/load trial. Actual local Moonshine is additionally exercised with the same
published English/German synthetic input, while physical gesture timing and echo remain human-trial work.

The original button-focused ASR fixture runs unchanged in snapshot `ad10fcd3`; its cold restore/build/
import supplements the existing frozen Pocket fixture. The new current Main checks qualify this
successor. Historical validators and receipts are preserved rather than rewritten for new semantics.

```sh
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --filter FullyQualifiedName~TapHoldGestureTests
python3 scripts/check-continuation.py --self-test
python3 quality.py --self-test
./quality.sh
```

The Main fixture requires `ALLRPCS_SPACE_HOLD_CHECK=1`, a mode in `ALLRPCS_SPACE_HOLD_MODE` and an
absolute `ALLRPCS_SPACE_HOLD_REPORT`. Modes: `headless`, `unavailable`, `native-1280`, `native-1920`,
`moonshine-english-1280`, `moonshine-german-1280`. Disable default provider composition, set the shared
participation supplier to 0 and use missing player/reply configuration paths for injection. Actual
Moonshine modes use `ALLRPCS_SPACE_HOLD_RUNTIME` and `ALLRPCS_SPACE_HOLD_FILE`. Native tests run serially;
the fixture caps rendering to 60 FPS and uses a 7,200-frame watchdog with an explicit completion marker.

The owner previously reported that live recognition understood their words well. That qualitative
report does not establish full device, permission, German, noise/accent or echo qualification. This
successor's synthetic evidence also does not certify those cases, M1/other-platform behavior or a
packaged distribution. The original cold-start limitation and editable-text safeguard still apply.
