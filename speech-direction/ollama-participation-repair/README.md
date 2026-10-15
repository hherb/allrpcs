# Ollama ordinary-dialogue participation repair

The owner approved the [bounded proposal](../player-microphone-qualification/ollama-participation-fix-proposal.md)
on 2026-10-16 after a screenshot showed an accepted Whisper submission with no character response.
[Authority](authority.json) preserves the exact grant and proposal hash.

## Change

Ordinary conversation asks each heard character whether to respond, interject or remain silent before
starting reply generation. Composition previously supplied that decision model only for OpenAI;
Ollama fell back to an unavailable model. The new loopback Ollama adapter and binding complete this
missing step, using the existing trusted character context, decision schema and strict validation.
Fresh selection and provider replacement install the correct participation binding. OpenAI and the
existing unsupported-provider behavior remain covered by the current composition regression.

Actual local execution also exposed a new optional Ollama terminal field,
`prompt_eval_cached_count`. Both the participation accumulator and the existing reply accumulator
now accept and bound this usage metadata. It is not added to token totals or interpreted as dialogue.
Malformed metadata, duplicate/unknown fields, invalid decisions, incomplete streams, transport
failure and cancellation still fail safely. Reply generation rules and its response schema do not
change. This small protocol correction is necessary for the selected installed runtime to reach a
reply through both parsing stages.

Hearing ranges, participation/silence policy, explicit Submit/Whisper, microphone drafts and reply
playback are unchanged. The repair supplies no automatic submission, compulsory NPC reply, new UI
layout, model download, credentials, physical capture or executable release.

## Verification

[Verification](verification.json) binds the final sources and retained checks. The standalone
`game/OllamaParticipationMainCheck.tscn` instantiates ordinary Main with isolated settings. Its
controlled HTTP modes prove explicit Whisper submission reaches the configured participation and
reply adapters, commits a real reply to the ordinary transcript, preserves valid silence, and never
fabricates a reply on provider failure. The HTTP fixtures include cached-prompt metadata. Fresh
composition and replacement are checked without credentials; the normal composition regression
also covers OpenAI transitions. The quality runner includes the three controlled Main modes.

The selected installed `gemma4:e4b` also completed a direct local Main check using the fictional
sentence in the fixture source: one participation request returned `respond`, one reply request
completed, and one reply committed to the transcript. The retained live report contains status and
counts, not personal speech. This is actual local model integration evidence, without a microphone
or physical Whisper/echo qualification.

The initial diagnostic fixture stopped on a non-speech transcript event while inference was still
pending. The corrected fixture uses a wall-clock bound and waits for the actual participation/reply
outcome. A later real failure isolated the cached-prompt metadata incompatibility. Retained diagnosis
and final evidence distinguish these observations rather than counting early runs as reply passes.

The first full run stopped at formatting in the new fixture. The retained attempt contains all logs
and its pre-format source receipt. `finish-quality.py` verifies identical C# tokens, exact other
recorded source/CI inputs, stage signatures and every archived log hash. Its rejection self-test
covers changed source, tokens, logs and failure identity. It reuses 23 unchanged boundary/restore
passes, reruns continuation, and executes formatting plus the entire remaining normal pipeline.
No failed or unperformed check is counted as a reused pass. Future verification uses ordinary
`./quality.sh`; the helper is deliberately restricted to this recorded one-time recovery.

The subsequent full smoke run encountered one intermittent assertion in the unchanged supplied-model
sage-interjection fixture. The exact failed conjunct was not captured. This fixture bypasses Ollama;
the identical isolated command and 20 disposable baseline runs passed. Attempt 2 retains the original
failure, logs and failure-only baseline diagnostic patch. `resume-smoke.py` binds 3,176 program/CI
inputs and archived logs, retains only 34 completed stages, and reruns the complete failed smoke
script and all remaining normal gate stages with unchanged assertions. Its rejection self-test
covers source/log drift and unsuccessful prefix evidence. This records a fixture intermittency;
it does not claim a diagnosed or repaired legacy gameplay defect.

The recovered full product gate passed **217 recorded stages and 5,629 unit tests**, with no skipped
unit tests. The final summary and all referenced stage logs are retained in `quality/final/`.
The repair is [closed](closure.json); the human microphone trial remains open. Historical frozen fixtures, validators and
receipts remain unchanged; current composition regression expectations change only where Ollama
now has a participation adapter. The unrelated `game/project.godot` editor rewrite is excluded.

## Reproduction

Run `dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj` with a filter for
`OllamaCharacterTurnParticipation`, `OllamaCachedPromptMetadata` and
`OllamaCharacterModelStreamAccumulator`, then build `game/Allrpcs.Game.csproj`.
Run `python3 quality.py --self-test`, `python3 scripts/check-continuation.py --self-test` and `./quality.sh`.

For the standalone Main scene, set `ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1`,
`ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1`, `ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0`,
`ALLRPCS_OLLAMA_PARTICIPATION_MODE` and an absolute `ALLRPCS_OLLAMA_PARTICIPATION_REPORT`, then run
`godot --headless --path game res://OllamaParticipationMainCheck.tscn`. Modes are `respond`, `silence`,
`failure` and explicitly enabled `live`. Missing reply/player speech config paths keep this diagnostic
independent of local speech composition. Live mode uses local `gemma4:e4b`; other modes use controlled
HTTP with no backend traffic. Do not confuse live model inference with human acoustic qualification.

## Remaining human trial

Resume the [active microphone trial](../player-microphone-qualification/README.md) using the existing
owner-operated grant. Stand beside the intended character for Whisper (48-pixel hearing range),
select the intended language, and observe the actual submission/reply outcome. Earlier recognition,
correction, Stop mic, Space release and brief-tap successes stay accepted. Human lifecycle/device,
quiet/accent and echo cases remain separately incomplete.
