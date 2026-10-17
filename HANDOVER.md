# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

The owner-approved [code-health program](docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md)
is active. Phase 2 Tasks 1–12 and fresh whole-phase review passed. Provider Ready lifecycle
correction `37bc8fcd` passed actual startup/slow-cadence probes and scoped review. Latest hosted
run 39014801698 on `9dfa4b7b` passed those provider checks and all non-Godot jobs, but failed the
music exploration fixture (362 native passes, one failure, two existing quarantines).
`b4eab186` establishes exact cooldown eligibility after its fractional clock preparation;
controlled failure/recovery, real-cooldown rejection, eight integration checks, mode parity and
scoped review pass. The exact earlier hosted residual was not logged and remains inferred.
The [Phase 2 task plan](docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md)
records every failure and correction. [Draft PR #440](https://github.com/hherb/allrpcs/pull/440)
is open. Task 13 still needs a complete root quality result and latest hosted confirmation;
earlier failed/interrupted reports are retained and do not count as passes.
Merged Phase 1 main `032c5b30` has an identical tree to starting `e55463b0`; its model-timeout
fixture failure was repaired and verified in Phase 2 Task 3.

The live fictional OpenAI request/captured stream (#417) remains unavailable: an existence-only
query found no application-specific Keychain item and retrieved no secret. The owner Keychain
save check was requested once and has no reported result. Keep these conditions open.

The [microphone qualification](speech-direction/player-microphone-qualification/README.md)
remains paused: preserve its 3 pass / 5 partial / 7 unperformed cases and all qualitative owner
reports. Closed reply, recognition, Space, Ollama, examination and conversation-context work stays
closed; its records remain linked from the ledger and qualification package. Do not infer a physical
pass from synthetic or local-model checks. [Physical controls](input-direction/physical-controls-deferral.json)
remain deferred pending hardware and authority; VoiceOver qualification remains unperformed.

Keep all persistent closures and frozen acceptance evidence. Privacy/provider disclosures and
accepted controls/dialogue accessibility remain closed. Future walking work uses the accepted
[reference gait model](art-direction/reference-gait-model.md).

Standing publication authority: “ALWAYS commit and push when you finish a task.” Retain
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json).
This authorizes review-branch publication; no merge or release is authorized here.

<!-- active-slice:start -->
## Next bounded slice

**code-health-phase2-player-defects — Fix the player-facing code-review-1 defects** (in_progress).

Roadmap item: - [ ] Phase 2: player-facing code-review-1 defects are fixed with pinning tests.

Missing deliverable: One full ./quality.sh phase-exit result and latest hosted CI after reviewed
fixes; live fictional OpenAI request/captured stream for #417 and the requested owner Keychain save
result remain open.

Continue Task 13 in docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md. Tasks
1-12 and whole-phase review passed. Provider Ready correction 37bc8fcd and current music cooldown
eligibility correction b4eab186 passed controlled regressions, affected integration checks and
scoped reviews. Latest hosted 39014801698 passed provider checks but failed earlier music
exploration admission; the exact original pre-transition numeric value was unrecorded. Draft PR #440
is open. Run one complete ./quality.sh under D2 and confirm latest hosted CI; retain every
failed/interrupted report and diagnostic limit. Keep live #417 and requested owner Keychain evidence
open. Do not close Phase 2, select Phase 3 or merge without satisfying exit conditions and
applicable authority.

Done when: All code-review-1 issues except the Phase 1 follow-ups are closed by commits with pinning
tests, the five Review Focus items in the program plan have pinning tests, and one full ./quality.sh
passes.

Expected outputs:

- `docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md`

Boundary: Player-facing defect fixes and their tests only; no new gameplay features (Phase 4) and no
pipeline consolidation (Phase 3).
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
