# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence. Preserve every persistent closure.

## Current checkpoint

The owner requested both screenshot repairs: fitted settings and removal of retained Stream
crossing/cap placeholders. [The current successor](art-direction/gameplay-view-repairs/README.md)
keeps provider/input/physics ownership and predecessor art evidence unchanged. Current
focused checks and eight UI-visible captures pass. The complete root gate passed all 218
stages with 0 retries: 6,181 solution tests and 370 smoke checks
(3 existing quarantines). The two-defect repair is closed; building visual acceptance
remains the existing owner-review boundary.

Phase 3 and Phase 4 implementation items 1–6 are closed on main. The Phase 4 exit full
quality gate passed on 2026-10-23 (218 stages, no retries;
[record](docs/superpowers/evidence/2026-10-23-phase4-exit-quality.json)). The fresh-profile
owner playthrough to an ending on local Ollama remains unfinished
([findings](docs/superpowers/evidence/phase4-owner-playtest.md)); it is deferred while the
owner prioritises fitting existing artwork into gameplay, starting with buildings.
Do not infer playtest acceptance from automated checks. A fresh profile removes both
`user://save-v1.json` and `user://seeker-profile-v1.json`; preserve real user saves.

The [building integration](art-direction/buildings-gameplay-integration/README.md) reuses
six approved structure exports and three existing derived assets from `cfed60a8`, fitted
to current Main at five sites. Current room layouts drive cutaways, collision segments
drive low walls, and existing gameplay/geometry remains intact. Full root quality passed all 218 stages with no retries on 2026-10-24; 6,181 solution
tests and 369 smoke checks pass (3 existing quarantines). Twelve runtime captures and
grouped review boards are retained with the verification record. Owner runtime visual acceptance remains
explicit; historical candidate wording never reopens accepted source artwork.

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

**buildings-gameplay-integration — Fit existing building artwork into gameplay** (in_progress).

Roadmap item: - [ ] Fit existing building artwork into ordinary gameplay at the opening-region
structure sites.

Missing deliverable: The five-site building implementation and the two requested screenshot repairs
are verified. The current full root gate passed 218 stages, 6181 solution tests and 370 smoke checks
(3 existing quarantines). Twelve building runtime captures and eight normal-UI repair captures are
retained. Only owner runtime visual acceptance of the complete building exterior/cutaway group
remains; do not repeat source-art approval or the passing gate for prose alone.

Present the complete building and cutaway review group in art-direction/buildings-gameplay-
integration. The implementation, expanded native player/companion/presence checks and full quality
gate are complete. Record a whole-slice owner visual result when supplied; revise only specifically
requested building presentation, and rerun affected checks/full quality only for changed product
inputs. Preserve all accepted art, routes, physical owners, clue/gameplay state and the deferred
Phase 4 owner playtest. Publish reviewed branch changes under standing authority; no merge or
release.

Done when: The five building sites and party-aware cutaways are implemented, current native and full
quality checks pass with retained evidence, the owner accepts the complete runtime visual group, and
the reviewed branch is committed/pushed.

Expected outputs:

- `art-direction/buildings-gameplay-integration/README.md`
- `art-direction/buildings-gameplay-integration/verification.json`
- `art-direction/buildings-gameplay-integration/building-review.png`
- `art-direction/buildings-gameplay-integration/cutaway-review.png`

Boundary: Buildings presentation and dependent verification/documentation/publication only. No new
art generation, terrain replacement, actor animation, effects/UI integration, new geography, merge
or release.
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
