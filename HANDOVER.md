# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence. Preserve every persistent closure.

## Current checkpoint

The owner accepted the five-site [building group](art-direction/buildings-gameplay-integration/README.md)
and the two screenshot repairs (both closed). The current
[sickhouse bed/patient slice](art-direction/sickhouse-patient-bed/README.md) installs a new
owner-picked Codex `image_gen` bed (sickbed-01) and Mira's approved dormant poses 1↔2 as a
breathing loop, on PR #473. Its native check, validator and captures pass; the PR review fixes
(check watchdog, capture failure exit, import-settings check) are in. The full root gate result
goes into its `verification.json`. #471 is addressed on PR #473 (closes on merge): the accepted
building/view-repair whole-tree validator replays in `quality.py` at `5c99c7c4`, and live smoke
fences only those packages' accepted files.
Other greybox object/actor markers remain; replacing them needs a new owner request.

Phase 3 and Phase 4 implementation items 1–6 are closed on main. The Phase 4 exit full
quality gate passed on 2026-10-23 (218 stages, no retries;
[record](docs/superpowers/evidence/2026-10-23-phase4-exit-quality.json)). The fresh-profile
owner playthrough to an ending on local Ollama remains unfinished
([findings](docs/superpowers/evidence/phase4-owner-playtest.md)); it is deferred while the
owner prioritises fitting existing artwork into gameplay, starting with buildings.
Do not infer playtest acceptance from automated checks. A fresh profile removes both
`user://save-v1.json` and `user://seeker-profile-v1.json`; preserve real user saves.

Live OpenAI #417 capture and the already requested Keychain save result remain
[owner-deferred](docs/superpowers/evidence/2026-10-18-phase3-authority.json). Phase 2 and
#417 remain open; do not repeat the pending question or claim those live results.
Microphone qualification is paused at 3 pass / 5 partial / 7 unperformed; physical controls
are deferred pending hardware and authority, and VoiceOver remains unperformed. Accepted
controls/dialogue, art, audio, speech repairs, privacy and provider disclosures stay closed.
Future walking work uses the [reference gait model](art-direction/reference-gait-model.md).

Standing publication authority: “ALWAYS commit and push when you finish a task.” Retain
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json).
It authorizes review-branch publication; no merge or release is authorized here.

<!-- active-slice:start -->
## Next bounded slice

**sickhouse-patient-bed — Replace the sickhouse bed/patient placeholder with artwork** (in_progress).

Roadmap item: - [ ] Replace the sickhouse bed/patient placeholder with a new bed and Mira's approved
breathing dormant poses.

Missing deliverable: The owner-approved sickbed-01 module and the runtime bed plus Mira dormant
poses 1/2 breathing loop are installed with their native check, validator and captures, and the PR
#473 review fixes are applied. Missing: the full root gate on the reviewed branch head recorded in
verification.json, and its publication on PR #473.

Do not regenerate or re-present the bed; sickbed-01 is owner-approved. Keep the installed bed and
unchanged Mira dormant exports under OpeningRegionFirstPatientSceneSource without moving that node
or changing interaction ranges, collision, navigation, clue/disease state or saves; cutaway
visibility stays via SelfModulate. The native proof is scripts/check-sickhouse-patient-bed.gd
(check-building-gameplay.gd is unchanged). Run the full gate on the branch head, record it in
verification.json and publish under standing authority; no merge or release.

Done when: The bed module is owner-approved, the sickhouse cutaway shows the bed and breathing Mira
in ordinary Main, native and full quality checks pass with retained evidence, and the branch is
committed/pushed with a PR.

Expected outputs:

- `art-direction/sickhouse-patient-bed/README.md`
- `art-direction/sickhouse-patient-bed/verification.json`

Boundary: Sickhouse bed/patient presentation, the one new bed module and dependent
verification/documentation/publication only. No other placeholder replacement, merge or release.
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
