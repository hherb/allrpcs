# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence. Preserve every persistent closure.

## Current checkpoint

The owner accepted the five-site [building group](art-direction/buildings-gameplay-integration/README.md)
and closed the [sickhouse bed/patient slice](art-direction/sickhouse-patient-bed/README.md) (PRs #473/#475
merged). The owner then requested the Healer and Village Sage (Orin) characters
([authority](art-direction/healer-sage-runtime/authority.json)). That slice is implemented on branch
`claude/healer-sage-runtime` ([package](art-direction/healer-sage-runtime/README.md)) and awaits owner
review of PR #478. The water-cask marker overlapping the Sage is [#477](https://github.com/hherb/allrpcs/issues/477);
tree-order drawing of the tall sprites is [#479](https://github.com/hherb/allrpcs/issues/479); the walks are not
re-authored to the reference gait model and await owner sign-off ([#480](https://github.com/hherb/allrpcs/issues/480)); the Healer's inspect gesture is
wired but untriggered ([#481](https://github.com/hherb/allrpcs/issues/481)).
Other greybox markers (herb patch, stone marker, water cask, wagon clue, closed forest gate,
exit markers) remain; replacing them needs a new owner request.

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

**healer-sage-runtime — Replace the Healer and Sage greybox bodies with approved character art** (in_progress).

Roadmap item: - [ ] Replace the Healer and Village Sage greybox bodies with their approved
characters-batch artwork.

Missing deliverable: Runtime copies of the approved healer-* and orin-* idle/walk/inspect frames,
their animation wiring on the Healer and SageHearingPosition nodes in Main, a native check, a
validator binding runtime bytes to the characters-batch acceptance, and captures.

Implemented on branch claude/healer-sage-runtime (2026-10-29): 96 approved frames copied byte-
unchanged (idle/inspect from characters-batch/exports, walks from walk-rig-polish, timed to the
reference gait's 720 ms cycle), Healer/WalkAnimation and SageHearingPosition/Figure in Main, check
and validator in smoke, captures retained in the package. The walks follow the reference gait
model's 720 ms timing but are not re-authored to its poses (art-direction/reference-gait-model.md
still governs walking); that deviation awaits owner sign-off (#480). Remaining: owner review of the
PR; then close the scope on owner direction. Do not replace other placeholders without owner
direction (the water-cask overlap is #477, tree-order drawing of tall sprites #479, the untriggered
Healer inspect gesture #481).

Done when: Healer and Sage show their approved directional art in ordinary Main, native and full
quality checks pass with retained evidence, and the branch is committed/pushed with a PR.

Expected outputs:

- `art-direction/healer-sage-runtime/README.md`
- `art-direction/healer-sage-runtime/verification.json`

Boundary: Healer and Sage presentation, their runtime art copies and dependent
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
