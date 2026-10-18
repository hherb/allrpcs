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

Current validation: 5,994 solution tests, zero-warning build and 17 affected native launches pass.
The owner explicitly approved publication on 18 October. The implementation is committed at
`19745021` and published for review in draft [PR #443](https://github.com/hherb/allrpcs/pull/443)
on `codex/phase3-provider-bindings`; hosted CI is tracked on the PR.
The [closure receipt](docs/superpowers/evidence/2026-10-18-phase3-structured-decisions.json)
retains source bindings and limits. Full Phase 3 completion is not claimed: next is program item 2,
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

**code-health-phase3-main-coordinators — Extract the conversation-turn and focused-search coordinators** (ready).

Roadmap item: - [ ] Phase 3: one structured-decision pipeline and a thin Main composition root.

Missing deliverable: A bounded coordinator-extraction plan and tested Application conversation-
turn/focused-search state machines, thin native adapters, explicit process priorities and an order
check. The structured-decision consolidation is complete and closed.

Continue Phase 3 program item 2 in docs/superpowers/plans/2026-10-17-code-health-and-playable-
slice.md. First inspect Main.cs and the current conversation/focused-search owners, then write the
finite extraction plan in docs/superpowers/plans/2026-10-18-code-health-phase3-main-coordinators.md.
Extract the existing state machines into Application tests and thin native adapters without changing
gameplay, input semantics or exact request/restore/cancellation ownership. Set explicit process
priorities and verify dependency order. Do not reopen the closed structured-decision slice or
deferred Phase 2 checks. Use approved D2 format/build/test and affected native launches; the full
root gate remains due at complete Phase 3 exit. Complete review, documentation and authorized
commit/push for the bounded work; no merge or release.

Done when: Both existing state machines are in tested Application coordinators, Main delegates
through thin native adapters, exact lifecycle and input behavior is preserved, explicit process
order is checked, and scoped format/build/test/native checks pass with synced continuation evidence
and review-branch publication.

Expected outputs:

- `docs/superpowers/plans/2026-10-18-code-health-phase3-main-coordinators.md`
- `src/Application/Dialogue/ConversationTurnCoordinator.cs`
- `src/Application/Input/FocusedSearchInputCoordinator.cs`

Boundary: Phase 3 coordinator extraction only. Preserve domain behavior, prompts/schemas, exact
identities and frozen/closed evidence. No new gameplay or sabotage kind, Phase 4, SmokeCheckHost
migration, physical/provider qualification, phase-exit claim or release.
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
