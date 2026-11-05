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
greybox markers one at a time: the stone marker tablet (PR #483), herb patch sprigs (PR #484) and wagon clue
broken strap (PR #485) merged and were closed ([stone](art-direction/stone-marker-art/owner-acceptance.json),
[herb](art-direction/herb-patch-art/owner-acceptance.json), [wagon](art-direction/wagon-clue-art/owner-acceptance.json)).
Remaining greybox markers (water cask, closed forest gate, exit markers) need their own owner request; no accepted
art matches them closely (no cask art exists). The #486 smoke flake fix merged as PR #487 and was closed on
2026-10-31 ([acceptance](docs/superpowers/evidence/2026-10-31-ci-flake-486-owner-acceptance.json)). The #476
shutdown guard was verified against CI, merged as PR #488 and closed on 2026-11-05
([acceptance](docs/superpowers/evidence/2026-11-05-shutdown-abort-476-owner-acceptance.json)). The active
slice below keeps one backup when a save overwrites an existing one (#467,
[authority](docs/superpowers/evidence/2026-11-05-save-backup-467-authority.json)).

Phase 3 and Phase 4 implementation items 1–6 are closed on main. The Phase 4 exit full
quality gate passed on 2026-10-23 (218 stages, no retries;
[record](docs/superpowers/evidence/2026-10-23-phase4-exit-quality.json)). The fresh-profile
owner playthrough to an ending on local Ollama remains unfinished
([findings](docs/superpowers/evidence/phase4-owner-playtest.md)); it is deferred while the
owner prioritises fitting existing artwork into gameplay, starting with buildings.
Do not infer playtest acceptance from automated checks. A fresh profile removes both
`user://save-v1.json` and `user://seeker-profile-v1.json` (their `.bak` backups do not affect
launch); preserve real user saves.

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

**seeker-reference-gait-462 — Play the accepted Seeker reference gait in Main's runtime walk** (in_progress).

Roadmap item: - [ ] Play the accepted Seeker reference gait in Main's runtime walk (#462).

Missing deliverable: Main's Seeker walk still plays the older 6-frame-per-direction cycle (same leg
always leading); the owner-accepted 48-frame reference gait was never installed in ordinary
gameplay.

Byte-copy the 48 accepted frames into game/art/runtime/characters/seeker/<direction>/walk-NN.png,
point the runtime walk_* animations at them at 90 ms front/back and 45 ms sides (720 ms cycle), keep
the 180 px side frames' feet on the idle ground line, update the runtime walk smoke check, capture
Main, then publish a review PR that closes #462.

Done when: Main's four walk animations play the accepted reference-gait frames byte for byte in
their phase order and timing, feet stay on the idle/interaction ground line, the runtime walk smoke
check pins this and passes with the affected smoke launches, and the branch is pushed with a PR that
closes #462 on merge.

Expected outputs:

- `game/art/runtime/characters/seeker/walk-animations.tres`
- `game/SeekerWalkAnimationPresenter.cs`
- `game/RuntimeSeekerWalkSmokeCheck.cs`
- `art-direction/seeker-reference-gait-runtime/README.md`

Boundary: Runtime Seeker walk frames, their SpriteFrames entries, presenter frame anchoring, the
runtime walk smoke check and dependent documentation/publication only. No new or edited artwork, no
idle/interaction change, no Healer/Sage walk re-authoring (#480), no movement or collision change.
No merge or release.
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
