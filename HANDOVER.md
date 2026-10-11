# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the active
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions
and terminal acceptance take precedence; retain all persistent closures.

## Current checkpoint

Accepted ground/channel/Clearing presentation, parked studies, character import proof
and the whole cloak correction stay closed. Future walking work uses the accepted
[reference gait model](art-direction/reference-gait-model.md). Historical pending fields
never reopen accepted views, placements or sounds.

Audio policy, buses/controls, sixteen audition excerpts, six-sound material runtime,
Seeker terrain footsteps, named positional cues, optional M01/M02 music and bounded
sickhouse/laundry interior audio are complete. Their frozen evidence remains linked
from [music review](audio-direction/music-runtime-review/README.md) and
[interior review](audio-direction/interior-runtime-review/README.md).

The approved **Touch water** action remains closed; see
[water review](audio-direction/water-contact-runtime-review/README.md).
The owner-approved **Adjust cask cover** action is now complete: existing village cask,
new wooden-lid/cotton-cover binding, inclusive 32-pixel supported reach, exact transient
commits/four-second active cooldown, contextual mouse/Tab/Enter/Space, unchanged A02-C01
passage, own status/seventh caption and packaged source notices. No contents, inventory,
geometry/art, durable save, perception, sabotage recovery or provider change is included.
See [cask review](audio-direction/cask-cover-runtime-review/README.md),
[verification](audio-direction/cask-cover-runtime-verification.json) and
[grant](audio-direction/cask-cover-runtime-authority.json).

The approved **Tap practice block** action is complete: fixed Village Edge site,
newly declared communal wooden mallet/block, inclusive supported 32-pixel reach,
exact transient human commits/one-second active cooldown, contextual controls/status,
independent eighth caption and exact A03-T01 feedback/notices. Tools stay at the site;
there are no inventory items, depicted props, geometry, durable truth or perception
changes. See [hand-tool review](audio-direction/hand-tool-runtime-review/README.md),
[verification](audio-direction/hand-tool-runtime-verification.json) and
[grant](audio-direction/hand-tool-runtime-authority.json).

Full current quality passes 62 stages and 5,227 unit tests, all 293 retained Godot
launches, four interior modes and six modes each for water, cask and hand-tool/control.
Twenty actual hand-tool Master cases, complete save/transcript and serialized protected
truth parity, native controls/views/focus loss, mounted native archive decoding, resource
packaging and six rejection groups pass. The complete approved hand-tool implementation
and dependent review are closed; no further unchanged selection or approval is pending.
Wider actions, positional emitters/off-screen cues, regional audio identity, full room
acoustics and voice remain unfinished.

The read-only [Healer exterior positional-footstep proposal](audio-direction/healer-footstep-owner-scope.md)
and [exact contract](audio-direction/healer-footstep-owner-scope.json) are complete. They define
a separate post-collision cadence, four exterior materials, the existing Seeker listener,
192-pixel range/native pan, anonymous ninth caption and separate bounded transient history.
No Healer runtime change is installed. One new implementation/material/emitter/caption/history
grant remains pending; accepted samples and every completed owner remain closed.

Standing publication authority: “ALWAYS commit and push when you finish a task.” Carry
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json)
forward without asking again. It does not authorize new gameplay/layout/physical scope
or a release.

<!-- active-slice:start -->
## Next bounded slice

**phase9-positional-healer-footstep-runtime-integration — Implement the proposed Healer exterior positional footsteps after one owner grant** (ready).

Roadmap item: - [ ] Add positional ambience and meaningful off-screen sound cues.

Missing deliverable: The completed Healer-only exterior contact/material/emitter/caption/history
proposal has no implementation grant or installed owner. Its exact new runtime grant and finite
implementation/verification are missing.

Use audio-direction/healer-footstep-owner-scope.md and its hash-bound JSON. Request only the one
pending exact Healer exterior runtime/material/emitter/caption/history grant; record it before
runtime edits. Once granted, implement and finish dependent tests, seven-mode parity, 43 actual
Master output groups, native views/controls/history, resource packaging, rejection probes and
retained full quality in the same session, then publish under standing authority. Do not reopen the
completed proposal or any unchanged accepted audio/action/animation scope.

Done when: One matching explicit owner grant is recorded; the exact Healer exterior positional-
footstep owner and all finite dependent checks/review are complete and published, with protected
truth and every prior owner unchanged.

Expected outputs:

- `audio-direction/healer-footstep-runtime-authority.json`
- `audio-direction/healer-footstep-runtime-verification.json`
- `audio-direction/healer-footstep-runtime-review/README.md`

Boundary: Implementation is gated on the one genuinely new owner grant. Limit it to the published
Healer-only exterior proposal and named files/checks. No Healer interior contacts/transmission,
other actor/emitter, gameplay hearing/perception/provider/save/time, new PCM/art/geometry/gait
promotion, closed-owner reopening, voice or release.
<!-- active-slice:end -->

## Verification and local exceptions

For proposal/queue maintenance use `python3 scripts/check-continuation.py --self-test`.
After queue maintenance use `python3 scripts/check-continuation.py --sync`, then rerun
the checker. Product/CI inputs are unchanged; reuse the passing linked hand-tool quality
evidence for this planning slice. After the new implementation grant, add the proposed
Healer checks and run root `./quality.sh`, retaining the unchanged hand-tool validator
and fixtures on their pinned source tree.

Root quality uses pinned source-built Pillow/numpy and isolated frozen contracts,
including water at 50f4a751 and cask at 5495202f; all retained product/interior/water/
cask checks remain and six hand-tool schedules/current boundary are included. Setup:
[quality maintenance](quality-review/README.md). Run Godot on the host where logs/editor
settings are writable. Retry only diagnosed native mutex shutdowns with the identical
command. Frozen validators and `scripts/quality.sh` remain unchanged; do not modify
historical current-boundary checks to accommodate successors.

Packaging has 36 streams and 23 individual notices (20 retained plus the new three-file
bundle; 17 notice groups). Full macOS export still lacks a mono template and retains its
C# solution lookup mismatch. Packaging is not an executable release or fresh device/
ordinary-play loudness claim. The controlled unsupported-segment output case disables
navigation; finite interior-sample rejection is independently unit-tested. Native Master
capture uses the graphical renderer with explicit fixture focus; headless Movie Maker
crashes in this Godot dummy renderer. Its failed attempt is retained, not product evidence.
Preserve/exclude `game/project.godot` (recorded editor SHA
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`) and `.DS_Store`.
