# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

The owner-approved [code-health program](docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md)
is active. Phase 2 Tasks 1–12 and the original whole-phase review passed. The retained
[218-stage root pass](docs/superpowers/evidence/2026-10-17-code-health-phase2-quality.json) on
`010439d5` is historical: product/tooling review corrections `cee9df2b` and fixture correction
`b841afed` followed it. Latest [hosted run 39069028898](https://github.com/hherb/allrpcs/actions/runs/39069028898)
on `b841afed` passed all four jobs: 5,924 solution tests, 23 runner self-tests and native
363 passed / zero failed / two existing quarantines. Full hosted logs are retained locally at
`/private/tmp/allrpcs-phase2-current-ci.log`.

Another session already owns a full `./quality.sh` on `b841afed`, started 2026-10-17
20:57:41 UTC (18 October Sydney time). Its report at `artifacts/quality-current/summary.json`
was still running with 47 completed passing stages when inspected at 21:28 UTC. Preserve that
run and its logs; do not launch a competing gate or interrupt it. Record its terminal result
when available. The [Phase 2 plan](docs/superpowers/plans/2026-10-17-code-health-phase2-player-defects.md)
retains the earlier complete/failed evidence. [PR #440](https://github.com/hherb/allrpcs/pull/440)
is now open and ready for review, no longer draft. Keep Phase 2 open and Phase 3 unselected.

The live fictional OpenAI request/captured stream (#417) remains unavailable: an existence-only
application Keychain query on 18 October returned item-not-found and retrieved no secret.
The new schema-echo regression uses the real schema in a generated fixture; it is not a captured
live stream. The owner Keychain save check was requested once and has no reported result.
Keep both conditions open without repeating the pending question.

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

Missing deliverable: Completion of the already-running full root gate on b841afed after the PR
review corrections, live fictional OpenAI request/captured stream for #417, and the already
requested owner Keychain save result. Latest hosted CI passed all four jobs.

Continue only Phase 2 Task 13 in docs/superpowers/plans/2026-10-17-code-health-phase2-player-
defects.md. New product/tooling corrections cee9df2b and b841afed postdate the retained 218-stage
root pass on 010439d5, so that pass is historical evidence rather than validation of current source.
Hosted run 39069028898 on b841afed passed all four jobs (5924 solution tests, 23 runner self-tests,
native 363 passed with two existing quarantines). Another session already owns a full ./quality.sh
run on b841afed, started 2026-10-17 20:57:41 UTC; preserve its report/logs in artifacts/quality-
current and do not start a competing run or interrupt it. Record its terminal result when available.
PR #440 is open and ready for review, no longer draft; do not merge. The 2026-10-18 existence-only
application Keychain recheck returned item-not-found without retrieving a secret. Do not repeat the
pending owner question or imply live #417/Keychain success. Keep Phase 2 in progress; do not select
Phase 3, merge, release or reopen closed scopes before satisfying exit conditions and applicable
authority. Reuse unchanged passing evidence for subsequent prose-only changes.

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
