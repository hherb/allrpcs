# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the active
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence; retain every persistent closure.

## Current checkpoint

Accepted art, ground/channel presentation and the whole cloak correction stay closed.
Future walking work uses the accepted [reference gait model](art-direction/reference-gait-model.md).
Historical pending fields never reopen accepted views, placements or sounds.

Audio policy/controls, accepted auditions, material sounds, Seeker footsteps, named
positional cues, optional music, bounded sickhouse/laundry audio, water/cask/hand-tool
actions and Healer footsteps are complete. Their persistent closures and linked evidence
remain in the ledger. Their runtime, resources, geometry and gameplay authority are preserved.

The owner-approved **fixed garden foliage** slice is complete: F01 at the unchanged
(2232,1328) planting, exclusive 192-pixel Ambience admission, twelve-second starts,
anonymous tenth caption and independent history. See [review](audio-direction/foliage-runtime-review/README.md),
[verification](audio-direction/foliage-runtime-verification.json) and
[grant](audio-direction/foliage-runtime-authority.json). Full root quality passes
82 stages and 5,287 unit tests, with native output/UI, complete motion/save/truth
parity and 37-stream packaging. Preserve all 100 current audio archive entries
and 26 notice files in 18 groups. No further approval remains for this owner.

The **fixed southern-road wind runtime** is complete under its 2026-10-12
[grant](audio-direction/wind-runtime-authority.json). The owner qualified G01 with
“sounds right”. One unchanged (1500,1400) owner plays four seconds every 18 active
seconds within the exclusive 256-pixel range, with an anonymous eleventh caption and
independent wind history. See [review](audio-direction/wind-runtime-review/README.md)
and [verification](audio-direction/wind-runtime-verification.json). Full root quality
passes 95 stages (all 82 retained stages) and 5,335 unit tests; 48 focused cases,
58 native output cases, both-size native UI, seven-mode complete motion/save/truth
parity and mounted packaging pass. The 38-stream audio archive preserves all 100
prior entries and includes 29 notice files in 19 groups. The whole wind slice is
closed; no repeat source, runtime, UI or listening approval remains.

Standing publication authority: “ALWAYS commit and push when you finish a task.” Carry
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json)
forward without asking again. The owner also granted ongoing permission to search for
and download game sounds; see the foliage grant. Neither grants another runtime scope or release.

<!-- active-slice:start -->
## Next bounded slice

**phase9-positional-woodland-rain-owner-scope — Plan the first fixed woodland rain source and emitter** (ready).

Roadmap item: - [ ] Add positional ambience and meaningful off-screen sound cues.

Missing deliverable: A concrete read-only dedicated rain source/rights proposal and one exact
retained woodland-clearing emitter owner, with finite implementation checks and the one new source-
use/runtime grant.

Prepare a read-only proposal for one new dedicated rain passage and one exact existing
Main/OpeningRegionWoodlandClearingSceneSource owner. Establish source lineage/rights, binding
prerequisites, spatial schedule, anonymous caption/history, failure/reset behavior, finite checks
and the missing implementation grant. Do not acquire or prepare PCM, install runtime, change
weather/gameplay/art/geometry, or reopen B03/F01/G01 or prior accepted owners. The completed wind
grant does not authorize this new runtime/source-use scope.

Done when: One concrete hash-bound read-only rain source/emitter proposal identifies the missing
dedicated deliverable, exact retained owner, permitted implementation files, finite verification and
a single new implementation decision.

Expected outputs:

- `audio-direction/rain-owner-scope.md`
- `audio-direction/rain-owner-scope.json`

Boundary: Read-only planning for a distinct remaining positional ambience deliverable. No new
acquisition, derivative/runtime/physical/weather/gameplay/voice/release authority; preserve every
closure.
<!-- active-slice:end -->

## Verification and local exceptions

Use `python3 scripts/check-continuation.py --self-test` for queue maintenance and
`python3 scripts/check-continuation.py --sync` to render its handover. Wind checks are
`python3 scripts/validate-wind-runtime.py --reviewed` and `--output-only`.
Reuse passing product evidence for prose alone. Runtime changes require their granted
focused checks and root `./quality.sh`; runner changes require `python3 quality.py --self-test`.

Frozen validators and fixtures run in isolated historical source trees: hand-tool at
`75c1aa04`, Healer at `ae06c9ab`, foliage at `c4f24ea1`. Shared fields explicitly permitted
by the wind grant retain their frozen predecessor evidence. Cold imports wait for
completion and clean only verified generated UID sidecars. See the wind review for
preparation/repair records. Run Godot on the host; retry only diagnosed native mutex
shutdowns with the identical command. Native focus fixtures run serially in the foreground.

Audio resource packaging is verified for all three presets. Godot reports the existing
missing game solution during C# export; no executable release is verified. Controlled
source/output measurements do not establish fresh-device or ordinary-play loudness
compliance. Preserve/exclude `game/project.godot` (recorded editor SHA
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`) and `.DS_Store`.
