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
6,097 tests, smoke 365 passed, 0 failed). Item 3 is complete and closed (`d8885139`): one inventory owner, conditional gathering on the normal
path, Give/Use at the station, a wrong remedy that changes nothing, and save schema 10
([plan](docs/superpowers/plans/2026-10-21-code-health-phase4-ingredients.md); 6,129 tests, smoke
366 passed, 0 failed); PR #454 is merged. Item 4 is complete and closed
([PR #455](https://github.com/hherb/allrpcs/pull/455), merged): the player proposes a hypothesis in the journal, the
structured-decision pipeline interprets it, and a dose confirms or refutes it, with outcomes
derived from saved doses and no schema change
([plan](docs/superpowers/plans/2026-10-21-code-health-phase4-hypotheses.md); 6,132 tests, smoke
367 passed, 0 failed). Item 5 is complete and closed
([PR #457](https://github.com/hherb/allrpcs/pull/457), merged): the untreated patient worsens every 3
time-advancing events, and a loss or completion end panel survives save and Continue
([plan](docs/superpowers/plans/2026-10-21-code-health-phase4-stakes-ending.md), hash-frozen: its
step 6 and the names `SliceEndingCopy`/`SliceEndPanel` predate the build, which has
`FirstPatientStakesCopy`, `SliceEndOverlay/Panel` and the check's own scenario; stakes are on only
in a plain launch or `SliceStakesEndingFlowCheck`). Item 6 is complete and closed
([PR #459](https://github.com/hherb/allrpcs/pull/459), not merged): nothing is noticed or
reinterpreted at boot, the stream residue is noticed on arriving at the stream once the map has been
opened, and by owner decision the remedy dose (not a pre-confirmed conclusion) tests the conclusion
([plan](docs/superpowers/plans/2026-10-22-code-health-phase4-boot-resolution.md), hash-frozen: its
"every existing save already records residue" is false for saves migrated to empty routes, so every
map open, not only the first, catches up verified locations and the residue; 6,160 tests, smoke 367
passed, 0 failed, no schema change). Follow-ups for Phase 5 are in
[#460](https://github.com/hherb/allrpcs/issues/460). Item 7 (owner playtest and the Phase 4 exit
gate) is next. Use approved D2 scoped checks until the Phase 4 exit.

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

**code-health-phase4-owner-playtest — Owner playtest and Phase 4 exit** (ready).

Roadmap item: - [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local
Ollama.

Missing deliverable: Program Phase 4 item 7: no owner playthrough from a fresh profile to the ending
on local Ollama is recorded, and the Phase 4 exit full quality gate has not run.

Run the Phase 4 exit full root ./quality.sh once (decision D2) and record it in
docs/superpowers/evidence/2026-10-23-phase4-exit-quality.json like the Phase 3 exit record (rename
the output if the run happens on another date). Then ask the owner to play from a fresh profile to
the ending on local Ollama (gemma4:e4b) and record the result as an issue comment or a short session
note. Fix blocking defects the playtest finds as bounded commits with tests. Tick ROADMAP 4.7 and
the Phase 4 line only after the owner accepts. Commit/push reviewed work; no merge or release.

Done when: The Phase 4 exit quality gate passes and is recorded, the owner's playtest result is
recorded, and ROADMAP Phase 4 is ticked or its remaining blocking defects are listed, with
continuation evidence synced and review-branch publication.

Expected outputs:

- `docs/superpowers/evidence/2026-10-23-phase4-exit-quality.json`

Boundary: Phase 4 item 7 and the Phase 4 exit only. No Phase 5 reorganisation, no new sabotage kind,
no physical/provider qualification, no merge or release.
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
