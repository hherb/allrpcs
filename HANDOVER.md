# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

PR [#440](https://github.com/hherb/allrpcs/pull/440) merged at `3c4d6c63`. Merge
[CI 39077177772](https://github.com/hherb/allrpcs/actions/runs/39077177772) passed all four jobs:
5,924 solution tests, native 363 passed / zero failed / two existing quarantines. Full logs are
retained at `/private/tmp/allrpcs-phase2-merge-ci.log`. The retained complete 218-stage root pass
on `b841afed` validates the unchanged product baseline; new Phase 3 code needs its own scoped checks.

The owner [explicitly deferred](docs/superpowers/evidence/2026-10-18-phase3-authority.json) the live
OpenAI #417 capture and already requested owner Keychain save check and authorized Phase 3.
Phase 2 and #417 remain open; neither live result is claimed. Do not repeat the pending question.
Follow the [structured-decision plan](docs/superpowers/plans/2026-10-18-code-health-phase3-structured-decisions.md).
Main extraction and SmokeCheckHost are later Phase 3 items, not this slice.

The first Phase 3 checkpoint replaces ten duplicated OpenAI model/SSE/accumulator implementations
with shared Application lifecycle and Responses transport (3,790 → 1,270 lines including facades).
All ten existing type suites, 5,946 solution tests, zero-warning build, full format verification and
sixteen affected native launches pass. Preserve legacy lifecycle and delay-recovery metadata policies.
Next: provider-neutral descriptors/bindings, Anthropic/Ollama clients, production composition, then
removal of the compatibility facades. Detailed evidence and local log paths are in the plan.
Phase 3 remains in progress; the full phase-exit gate has not been run on this new code.

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

**code-health-phase3-structured-decisions — Consolidate the structured-decision pipeline** (in_progress).

Roadmap item: - [ ] Phase 3: one structured-decision pipeline and a thin Main composition root.

Missing deliverable: Provider-neutral structured request/schema descriptors and bindings,
Anthropic/Ollama structured clients and production composition, then removal of per-decision OpenAI
compatibility facades. The shared Application pipeline and OpenAI transport/stream extraction across
all ten decision kinds are implemented and verified.

Continue docs/superpowers/plans/2026-10-18-code-health-phase3-structured-decisions.md at provider-
neutral descriptors/bindings and Anthropic/Ollama clients. The first checkpoint migrated all ten
existing OpenAI model/SSE/accumulator facades to shared Application lifecycle and Responses
transport: 5946 solution tests, zero-warning build, full format verification, 53 focused shared-
client/deadline cases and 16 affected native launches pass. Preserve the three legacy prelude-
optional policies and delay-recovery metadata-after-text rejection; keep exact identities,
document/event bounds, cancellation, disposal and redaction. Request serializers, schemas,
deserializers and domain policies are unchanged. Do not repeat completed extraction or claim full
Phase 3 completion; Main extraction and SmokeCheckHost remain later program items. Phase 2 live #417
and owner Keychain checks are explicitly deferred by
docs/superpowers/evidence/2026-10-18-phase3-authority.json, not passed. Preserve closed scopes and
frozen evidence. Use D2 scoped format/build/test and affected native checks; one full quality gate
is required at full Phase 3 exit. Commit/push reviewed changes; no merge or release.

Done when: All ten structured-decision kinds use the shared tested Application pipeline and provider
clients, provider-neutral bindings support OpenAI/Anthropic/Ollama, and no per-decision OpenAI
transport/stream implementations remain; scoped tests and native integration pass.

Expected outputs:

- `docs/superpowers/plans/2026-10-18-code-health-phase3-structured-decisions.md`
- `src/Application/Characters/StructuredDecisionPipeline.cs`
- `src/Infrastructure/Characters/OpenAiResponsesStructuredClient.cs`

Boundary: Phase 3 structured-decision consolidation only. Preserve domain decisions,
prompts/schemas, identity policies and gameplay behavior. No new sabotage types, Phase 4 gameplay,
Main/SmokeCheckHost extraction, phase exit claim, live-provider qualification or release.
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
