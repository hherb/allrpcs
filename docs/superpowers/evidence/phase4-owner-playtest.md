# Phase 4 owner playtest (item 7)

Status: **in progress, not accepted.** The owner is playing a plain launch (`godot --path game`) on
local Ollama `gemma4:e4b`. Phase 4 is ticked only after the owner accepts a run from a fresh profile
to an ending.

## Findings and outcomes (2026-10-23)

| Finding | Cause | Outcome |
| --- | --- | --- |
| Dialogue only reachable through Tab > settings > Dialogue | No world action | Fixed on main (`b575b7d2`): Talk on Enter |
| Idle command-panel lines and a fixed caption box covered the world | Fixed layouts | Fixed on main (`edd6d5dd`, `b575b7d2`) |
| Reply speech (TTS) silent | Local Pocket venv in `/private/tmp` had been partly deleted by macOS | Environment: rebuilt at ignored `artifacts/pocket-python`, `artifacts/speech-runtime.json` repointed; Pocket Main check passes |
| Speech input (STT) "Listening", no text | Microphone delivered no usable audio | Environment: works after the microphone was fixed; no code change |
| First reply speech starts late | Pocket worker starts on first use | Open, not blocking |
| Healer seemed to answer twice, the line overwritten almost at once | Not reproduced; a likely cause is a second turn from the 2-turn autonomous conversation | Open: check a saved transcript if it recurs |
| Save refused until the map had been opened ("PRESS M FIRST"); M is swallowed while dialogue has focus | Save required map and route state that exists only after the first map open | Fixed in PR #464: save schema 11 records an unopened map; loading it resets an opened map |
| Customization form at every launch | No save could be made, so Continue never appeared; no profile memory | Fixed in PR #464: the confirmed Seeker is remembered and a launch without a save skips the form; New game asks again, prefilled |

The fixes in PR #464 were followed by a full root `./quality.sh` pass at `2df5855c` on 2026-10-23:
218 stages, no retries, no failures.

## Still to record

The owner's run from a fresh profile to an ending, and acceptance or the remaining blocking defects.
