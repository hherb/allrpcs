# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence. Preserve every persistent closure.

## Current checkpoint

The owner accepted the five-site [building group](art-direction/buildings-gameplay-integration/README.md),
closed the [sickhouse bed/patient slice](art-direction/sickhouse-patient-bed/README.md) and, after merging
PR #478, closed the [Healer/Sage art slice](art-direction/healer-sage-runtime/README.md) on 2026-10-29
([acceptance](art-direction/healer-sage-runtime/owner-acceptance.json)). Still open from it: walks not
re-authored to the reference gait ([#480](https://github.com/hherb/allrpcs/issues/480), not signed off) and the
untriggered Healer inspect gesture ([#481](https://github.com/hherb/allrpcs/issues/481)). The
[depth-order slice](art-direction/character-depth-order/README.md) (#479, #477) merged as PR #482 and was closed
on 2026-10-30 ([acceptance](art-direction/character-depth-order/owner-acceptance.json)). The owner is replacing
greybox markers one at a time: the stone marker tablet merged as PR #483 and was closed on 2026-10-30
([acceptance](art-direction/stone-marker-art/owner-acceptance.json)); the herb patch is the active slice below
([authority](art-direction/herb-patch-art/authority.json)). Remaining greybox markers (water cask, wagon clue,
closed forest gate, exit markers) need their own owner request. Of those, only the wagon clue (clue-objects) has
accepted near-matches; no cask art exists.

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

**wagon-clue-art — Replace the wagon clue placeholder with the approved broken strap** (in_progress).

Roadmap item: - [ ] Replace the wagon clue placeholder with the approved broken strap, in depth
order.

Missing deliverable: The wagon clue in Main shows the approved objects-batch clue-objects-05 broken
strap instead of a tan polygon, drawn behind a Seeker south of it and in front of one north of it,
with a byte/import validator, a native check, captures and a package.

Work on branch claude/wagon-clue-art: replace the WagonClue Marker polygon with a Sprite2D byte copy
of clue-objects-05 whose foot stands on the old polygon's base line; keep the node position (1900,
1640). Add the clue to CharacterDepthOrderPresentation's fixed props, keeping the four figures at z
1..4 and the herb patch and stone marker unchanged. Other greybox markers (water cask, forest gate,
exit markers) need their own owner request.

Done when: Ordinary Main shows the approved broken strap at the unchanged wagon clue point in
correct depth order with the Seeker, affected checks pass with retained evidence, and the branch is
committed/pushed with a PR.

Expected outputs:

- `art-direction/wagon-clue-art/README.md`
- `art-direction/wagon-clue-art/verification.json`

Boundary: Wagon clue presentation, its draw order and dependent
verification/documentation/publication only. Interaction point, ranges, perception, focused search,
tool gating, the wagon shell, collision, navigation, saves, the herb patch, the stone marker and the
four figures' ranks stay unchanged. No other placeholder replacement, merge or release.
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
