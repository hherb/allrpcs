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
smoke 364 passed, 0 failed). Both are on review branch `codex/phase3-thin-main`, not yet merged.
Item 2 is active. Use approved D2 scoped checks until the Phase 4 exit.

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

**code-health-phase4-memory-save-continue — Keep memory, customization and positions across save and continue** (ready).

Roadmap item: - [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local
Ollama.

Missing deliverable: Program Phase 4 item 2 (slice step 9): episodic memory is not written from
dialogue and observation into CharacterMemoryController, the save omits the transcript, Seeker
customization and character positions, and nothing offers continue-on-launch, so a restarted session
can pick a different background after loading.

Write docs/superpowers/plans/2026-10-19-code-health-phase4-memory-save-continue.md after measuring
the current save schema (SaveSnapshotCapture/Restoration/Json controllers, user://save-v1.json),
memory owners and the new-game start from item 1. Move each touched state machine into
src/Application with xUnit tests (rolling rule). A continued game must not reopen customization.
Keep save-version handling fail-closed and existing restoration checks passing. Use D2
format/build/test and the affected native launches; commit/push reviewed work; no merge or release.
#452 records the PR #451 relaunch-to-load gap this item closes (the saved guidance currently asks
the player to finish customizing first).

Done when: A save taken after customization, dialogue and movement restores the transcript,
character memory, customization and positions in a relaunched session that offers continue, with
tests and smoke checks passing and continuation evidence synced with review-branch publication.

Expected outputs:

- `docs/superpowers/plans/2026-10-19-code-health-phase4-memory-save-continue.md`

Boundary: Phase 4 item 2 only. No ingredients, hypotheses, stakes/ending or boot-time auto-
resolution work (items 3-6), no new sabotage kind, no physical/provider qualification, no merge or
release.
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
