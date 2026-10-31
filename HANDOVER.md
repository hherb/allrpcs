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
2026-10-31 ([acceptance](docs/superpowers/evidence/2026-10-31-ci-flake-486-owner-acceptance.json)). The active
slice below verifies the already-merged #476 shutdown guard against CI
([authority](docs/superpowers/evidence/2026-10-31-shutdown-abort-476-authority.json)).

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

**shutdown-abort-476 — End the intermittent .NET shutdown abort after passing smoke launches** (in_progress).

Roadmap item: - [ ] Smoke launches stop aborting at .NET shutdown after a passing check (#476).

Missing deliverable: The CheckDeadline shutdown guard (c35d8b64, merged with PR #475 on 2026-10-29)
was never verified against CI, so #476 stayed open.

Verified on branch claude/fix-shutdown-abort-476 (2026-10-31): 0 'Leaked unsafe
reference'/'script_bindings' lines in 23 CI runs and 1 superseded re-run attempt since the fix,
against 4 of 26 runs before it (docs/superpowers/evidence/2026-10-31-shutdown-
abort-476-verification.json). No code change. Remaining: owner review of PR #488 (it closes #476 on
merge); then close the scope on owner direction.

Done when: CI history since the fix shows no shutdown-abort signature, the verification record is
committed, and the branch is pushed with a PR that closes #476 on merge.

Expected outputs:

- `docs/superpowers/evidence/2026-10-31-shutdown-abort-476-verification.json`

Boundary: Verification of the merged CheckDeadline shutdown guard plus dependent
documentation/publication only. Product code, check assertions and accepted behaviour stay
unchanged. No merge or release.
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
