# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

The owner-approved [code-health program](docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md)
is active. Phase 2 Tasks 1–12, whole-phase review and all scoped corrections passed.
The complete `./quality.sh` on `010439d5` passed all 218 stages and required completion markers
(2026-10-17 09:10–10:02 UTC). [Its unchanged summary](docs/superpowers/evidence/2026-10-17-code-health-phase2-quality.json)
is retained in Git; full logs/reports are preserved locally in the ignored SDD recovery directory.
Hosted [run 39020148424](https://github.com/hherb/allrpcs/actions/runs/39020148424) on the same
checkpoint passed all four jobs: 5,921 solution tests, 23 runner self-tests and native 363 passed /
zero failed / two existing quarantines. The [Phase 2 plan](docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md)
records all evidence, earlier failed runs and limits. [Draft PR #440](https://github.com/hherb/allrpcs/pull/440)
is published. Automated exit work is complete; only the live/owner evidence below remains.
Reuse this product evidence for documentation-only successors. Keep Phase 2 open and Phase 3 unselected.
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

Missing deliverable: Live fictional OpenAI request/captured stream for #417 and the already
requested owner Keychain save result. Automated implementation, reviews, complete 218-stage root
gate and hosted CI have passed.

Continue only the remaining live/owner evidence in Task 13 of
docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md when those prerequisites
become available. Complete ./quality.sh on 010439d5 passed all 218 stages/required markers; hosted
39020148424 on the same source passed all four jobs (5921 solution tests, 23 runner self-tests,
native363 plus two existing quarantines). The full stage summary is committed under
docs/superpowers/evidence; failed/interrupted runs and passing full logs are retained locally. Draft
PR #440 is published. Reuse unchanged passing product evidence for prose-only successors. Do not
repeat the pending owner question or imply live #417/Keychain success. Keep Phase 2 in progress; do
not select Phase 3, merge, release or reopen closed scopes without satisfying the exit conditions
and applicable authority.

Done when: All code-review-1 issues except the Phase 1 follow-ups are closed by commits with pinning
tests, the five Review Focus items in the program plan have pinning tests, and one full ./quality.sh
passes.

Expected outputs:

- `docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md`
- `docs/superpowers/evidence/2026-10-17-code-health-phase2-quality.json`

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
