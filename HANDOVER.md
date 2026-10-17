# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

The owner-approved [code-health program](docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md)
is active. Phase 2 Tasks 1–12 are implemented and independently reviewed through `075d3ac2`;
5,921 solution tests, a zero-warning build/format check and the affected native checks pass.
The [Phase 2 task plan](docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md)
records commits, evidence, decisions and known limits. Fresh whole-phase review passed; its one
minor CI correction (`58c05ef9`) and actual gate/CI regression repairs (`571f1e22`, `b5d020cd`)
passed scoped reviews. The latter preserves the accepted final channel node and establishes
present speakers in two current audio fixtures (seven native passes). [Draft PR #440](https://github.com/hherb/allrpcs/pull/440)
is open. Task 13 requires one full quality run and latest hosted CI; no full Phase 2 pass is claimed yet.
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

Continue Task 13 in docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md. All
twelve tasks and whole-phase review passed; CI self-tests, actual preflight guard and four hosted
fixture regressions were repaired in 58c05ef9/571f1e22/b5d020cd and scoped reviews approved. Draft
PR #440 is open. Restart one full ./quality.sh under D2 after these reviewed repairs and confirm
latest hosted CI; preserve the earlier failed/interrupted reports. Keep live #417 and requested
owner Keychain evidence open. Do not close Phase 2, select Phase 3 or merge without satisfying exit
conditions and applicable authority.

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
