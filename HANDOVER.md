# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

Phase 3 of the code-health program is complete and closed. Structured decisions
([PR #443](https://github.com/hherb/allrpcs/pull/443), review fixes [#444](https://github.com/hherb/allrpcs/pull/444)),
the Main coordinators ([PR #449](https://github.com/hherb/allrpcs/pull/449)) and SmokeCheckHost
([PR #450](https://github.com/hherb/allrpcs/pull/450)) are on main. The
[thin-Main slice](docs/superpowers/plans/2026-10-18-code-health-phase3-thin-main.md) left `Main.cs`
at 262 lines with gameplay unchanged (`ca88d200`), and the exit's full root `./quality.sh` passed
there: 218 stages, no retries ([record](docs/superpowers/evidence/2026-10-18-phase3-exit-quality.json)).
Main checks are `game/SmokeCheckHost.cs` entries, never `Main.tscn` nodes.

The owner then asked to continue towards a playable game
([authority](docs/superpowers/evidence/2026-10-19-phase4-authority.json)), so Phase 4 began.
Item 1 is complete and closed (`e9ec6684`): a plain launch opens Seeker customization before play,
proof scenes and requested checks keep their ordinary start, and Forester opens the closed forest
path ([plan](docs/superpowers/plans/2026-10-19-code-health-phase4-new-game-flow.md); 6,068 tests,
smoke 364 passed, 0 failed); both are merged (PR #451). Item 2 is complete and closed (`6c10af88`,
[PR #453](https://github.com/hherb/allrpcs/pull/453), merged): memory is written from turns
and examinations, save schema 9 keeps the Seeker profile and positions, and a launch with a save
offers Continue ([plan](docs/superpowers/plans/2026-10-19-code-health-phase4-memory-save-continue.md);
6,097 tests, smoke 365 passed, 0 failed). Item 3 is complete and closed (`d8885139`, branch
`claude/phase4-ingredients`, not merged): one inventory owner, conditional gathering on the normal
path, Give/Use at the station, a wrong remedy that changes nothing, and save schema 10
([plan](docs/superpowers/plans/2026-10-21-code-health-phase4-ingredients.md); 6,129 tests, smoke
366 passed, 0 failed). Item 4 (hypotheses) is active. Use approved D2 scoped checks until the
Phase 4 exit.

The owner [explicitly deferred](docs/superpowers/evidence/2026-10-18-phase3-authority.json) live
OpenAI #417 capture and the already requested Keychain save check to begin Phase 3.
Phase 2 and #417 remain open; neither live result is claimed. Do not repeat the pending question.

Keep all persistent closures and frozen acceptance evidence. Microphone qualification remains
paused at 3 pass / 5 partial / 7 unperformed; physical controls remain deferred pending hardware
and authority, and VoiceOver remains unperformed. Accepted controls/dialogue, art, audio, speech
repairs, privacy and provider disclosures stay closed. Future walking work uses the accepted
[reference gait model](art-direction/reference-gait-model.md).

Standing publication authority: “ALWAYS commit and push when you finish a task.” Retain
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json).
This authorizes review-branch publication; no merge or release is authorized here.

<!-- active-slice:start -->
## Next bounded slice

**code-health-phase4-hypotheses — Let the player propose and test a hypothesis** (ready).

Roadmap item: - [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local
Ollama.

Missing deliverable: Program Phase 4 item 4 (slice step 6): InvestigationHypothesis types have no
game wiring and the journal is pre-filled, so a player cannot propose a hypothesis in the journal,
have it interpreted through the Phase 3 structured-decision pipeline, or test it through an
experiment.

Write docs/superpowers/plans/2026-10-21-code-health-phase4-hypotheses.md after measuring the
InvestigationHypothesis* Domain types, RepresentativeJournalCompositionCoordinator and the journal
pre-fill, the Phase 3 StructuredDecisionPipeline, and how experiment records (now including wrong-
remedy doses) reach the journal. Add a journal input that proposes a hypothesis, interpret it
through the pipeline, and test the result via an experiment. Move each touched state machine into
src/Application with xUnit tests (rolling rule); keep save-version handling fail-closed. Use D2
format/build/test and the affected native launches; commit/push reviewed work; no merge or release.

Done when: A player proposes a hypothesis in the journal, the structured-decision pipeline
interprets it, and an experiment confirms or refutes it, with the result surviving save and
continue, tests and smoke checks passing and continuation evidence synced with review-branch
publication.

Expected outputs:

- `docs/superpowers/plans/2026-10-21-code-health-phase4-hypotheses.md`

Boundary: Phase 4 item 4 only. No stakes/ending or boot-time auto-resolution work (items 5-6), no
new sabotage kind, no physical/provider qualification, no merge or release.
<!-- active-slice:end -->

## Verification and local exceptions

After ledger maintenance run `python3 scripts/check-continuation.py --sync`, its checker and
`--self-test`. Speech evidence and reproduction: [reply playback](speech-direction/dialogue-integration/verification.json) and
[player recognition](speech-direction/player-asr-integration/verification.json) and
[Space gesture](speech-direction/space-hold-to-talk/verification.json).
Runner checks: `python3 quality.py --self-test`. Full quality gate (required for product/CI changes;
slow — hundreds of stages). Exception: under the owner-approved code-health program (decision D2 in
[the plan](docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md)) the full gate runs
once per phase exit; between phase exits run `dotnet format`/`build`/`test` plus the affected smoke
launches. Reuse completed product evidence for subsequent prose-only planning.

Frozen validators remain unchanged in private historical source trees/indexes, including accepted
dialogue `e7b17575`, controls `1b0cab40` and herb `db298c8f`. Cold checks restore before build/import; cooling seeds its
four accepted UID sidecars. Run Godot on the host, serialize native focus fixtures, and retry only
diagnosed native mutex shutdowns identically. No OS reader setting or broad reader claim is authorized.

The Godot editor rewrite of `game/project.godot` is committed at `a55e9e8d` (`EDITOR_REVISION` in
`quality.py` pins the historical snapshot overlays), so `game/project.godot` may now change normally.
Preserve/exclude `.DS_Store` and concurrent `speech-direction/` work. UID cleanup removes only newly generated sidecars of unchanged tracked
GD sources. The existing C# executable-export limitation remains; no executable release is verified.
