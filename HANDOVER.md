# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

PR [#442](https://github.com/hherb/allrpcs/pull/442) merged at `a33cdc12`; merge
[CI 39086701001](https://github.com/hherb/allrpcs/actions/runs/39086701001) passed all four jobs.
The [structured-decision slice](docs/superpowers/plans/2026-10-18-code-health-phase3-structured-decisions.md)
is now complete and closed in the ledger: all ten kinds have shared descriptors, Application
lifecycle and OpenAI/Anthropic/Ollama clients; per-kind OpenAI compatibility files are removed.
Production composition retains exact replacement/one-shot ownership across all ten sources.
Anthropic requires explicit caller-owned credential injection; its credential store/UI remains absent.

The implementation (`19745021`) merged to main through [PR #443](https://github.com/hherb/allrpcs/pull/443)
at `4692ed82`; its [closure receipt](docs/superpowers/evidence/2026-10-18-phase3-structured-decisions.json)
retains source bindings and limits. The owner then reopened the scope only to apply the PR #443
review fixes. Because #443 had already merged, [PR #444](https://github.com/hherb/allrpcs/pull/444)
publishes them on top of main (a plain revert cannot pass the ledger checker), and the scope is
closed again ([receipt](docs/superpowers/evidence/2026-10-18-pr443-review-fixes.json)): 6,007 tests
and the full smoke suite (363 passed, 0 failed) pass. Live Anthropic qualification and two follow-ups in
closed areas are tracked as GitHub issues.

Phase 3 items 2 and 3 are also complete and closed: the Main coordinators merged via
[PR #449](https://github.com/hherb/allrpcs/pull/449), and the
[SmokeCheckHost slice](docs/superpowers/plans/2026-10-18-code-health-phase3-smoke-check-host.md)
left `Main.tscn` with no check node ([receipt](docs/superpowers/evidence/2026-10-18-phase3-smoke-check-host.json);
review branch `codex/phase3-smoke-check-host`). Main checks are now `game/SmokeCheckHost.cs`
entries, never scene nodes. Phase 3 is complete and closed: the
[thin-Main slice](docs/superpowers/plans/2026-10-18-code-health-phase3-thin-main.md) left `Main.cs`
at 262 lines with gameplay unchanged, and the exit's full root gate passed at `ca88d200`
([record](docs/superpowers/evidence/2026-10-18-phase3-exit-quality.json); review branch
`codex/phase3-thin-main`). The owner then asked to continue towards a playable game
([authority](docs/superpowers/evidence/2026-10-19-phase4-authority.json)), so Phase 4 item 1 is
active. Use approved D2 scoped checks until the Phase 4 exit.

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

**code-health-phase4-new-game-flow — Open Seeker customization at the start of a new game** (ready).

Roadmap item: - [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local
Ollama.

Missing deliverable: Program Phase 4 item 1: the Seeker customization menu opens only on the U key,
so a new player can walk past slice step 1. It must open at the start of a new game; a chosen
background already gates one route-access consumer (Forester opens the closed forest path).

Write docs/superpowers/plans/2026-10-19-code-health-phase4-new-game-flow.md, then open the existing
SeekerCustomizationMenuController at the start of a normal launch through its existing open path,
inert whenever a SmokeCheckHost gate is requested so existing checks keep their start state. Add one
host-entry smoke check that a plain launch shows the menu with the Seeker suspended and that
confirming Forester opens the closed forest gate, registered in scripts/smoke-godot.sh. Keep U while
nothing is selected. Saving customization and continue-on-launch belong to item 2. Use D2
format/build/test and the affected native launches; commit/push reviewed work; no merge or release.

Done when: A plain launch opens the customization menu before play, confirming a background applies
it and resumes the Seeker, the new smoke check and every existing smoke launch pass, and
continuation evidence is synced with review-branch publication.

Expected outputs:

- `docs/superpowers/plans/2026-10-19-code-health-phase4-new-game-flow.md`

Boundary: Phase 4 item 1 only. No save-schema change (item 2), no new sabotage kind, no
physical/provider qualification, no merge or release.
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
