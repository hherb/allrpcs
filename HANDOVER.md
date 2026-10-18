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
closed areas are tracked as GitHub issues. Full Phase 3 completion is not claimed: next is program item 2,
conversation-turn/focused-search coordinator extraction, then SmokeCheckHost and the phase-exit gate.
Use approved D2 scoped checks until that exit. Do not repeat the completed extraction.

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

**code-health-phase3-smoke-check-host — Instantiate only the requested check through a SmokeCheckHost** (ready).

Roadmap item: - [ ] Phase 3: one structured-decision pipeline and a thin Main composition root.

Missing deliverable: A bounded SmokeCheckHost plan and host that instantiates only the requested
engine check, removing the always-present check nodes from Main.tscn. The structured-decision and
Main-coordinator items are complete and closed.

Continue Phase 3 program item 3 in docs/superpowers/plans/2026-10-17-code-health-and-playable-
slice.md. First count the check nodes in Main.tscn and how each is gated and launched
(scripts/smoke-godot.sh, tests/Architecture.Tests/CheckRegistrationTests.cs, quality.py current-
revision stages), then write the finite plan in docs/superpowers/plans/2026-10-18-code-health-
phase3-smoke-check-host.md. Migrate checks in batches behind their existing launches without
changing what each asserts; MainProcessOrderCheck pins Main's process order and must keep passing.
Do not reopen closed structured-decision or coordinator scopes or the deferred Phase 2 checks. Phase
3 exit also requires Main.cs under 400 lines (730 after item 2) and one full root gate.
ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=retry is a known pre-existing intermittent failure, quarantined
as #448 so it no longer fails the suite. Use D2 format/build/test and affected native launches;
commit/push reviewed work; no merge or release.

Done when: Main.tscn holds no check nodes, every check still launches through the host with
unchanged assertions, check registration still passes, and scoped format/build/test/native checks
pass with synced continuation evidence and review-branch publication.

Expected outputs:

- `docs/superpowers/plans/2026-10-18-code-health-phase3-smoke-check-host.md`
- `game/SmokeCheckHost.cs`

Boundary: Phase 3 SmokeCheckHost only. Preserve check assertions, frozen/closed evidence and
gameplay. No new gameplay or sabotage kind, Phase 4, physical/provider qualification, phase-exit
claim or release.
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
