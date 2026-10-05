# Laundry packed-earth runtime scope proposal

**Named runtime approval pending.** The owner agreed to continue with the
recommended displayed 24px-feather
[external selection](../laundry-ground-feasibility/owner-selection.json) on 2026-10-05.
[Bindings](source-bindings.json) freeze the exact proposal and selected inputs.
No production asset, import, ordinary scene or product code is installed by this packet.

Promote exactly one underlay, `terrain.laundry_entrance_ground`, from
`laundry-ground-feasibility/proof-inputs/feather-24-layer.png`, unchanged SHA-256
`65358c5ef294f093c993a98064e25dc5325aab877b3fec1ef76a4ee69954ed20`.
Keep the selected layer byte-exact. The 16px alternative, hard support and
unmasked field remain external diagnostics.

| Property | Proposed value |
| --- | --- |
| Owner | Independent root `Node2D`, `LaundryGroundPresentation`, no script |
| Owner transform | Position `(1564,219)`, z=0, relative depth, scale1, rotation0 |
| Child | `PackedEarth`, one 92x210 `Sprite2D`, local position/offset `(0,0)` |
| Child transform | centred=false, scale1, rotation0, white modulation, no region |
| Effective depth | -78, above woodland transitions (-80/-79), below route (-75) and Laundry shell |
| Sampling | Linear filtering, repeat disabled, world phase `(0,0)`, period128 |
| Envelope | `(1564,219)`–`(1656,429)`, right/bottom exclusive |
| Coverage | Exact selected route segment, radius24 support, inward24px feather |

Preserve woodland litter/transition owners, the retained Laundry shell, physical
boundaries, opening markers and labels. No cropping,
repadding, recolouring, alpha editing, rotation, resampling or second layer.

## Allowed implementation after named approval

Use `laundry-entrance-ground.png` for exact-byte production, isolated-proof and
runtime copies under their existing terrain directories. Copy the selected
184x420 master unchanged to
`production-proof/corrected/masters/laundry-entrance-ground.ora`. Append one
manifest asset and provenance block through a new scoped
`production-proof/corrected/tools/build_laundry_ground.py`, preserving every
existing entry and master: **95 assets / 46 masters become 96 / 47**.

Add independent proof/runtime lossless `.import` sidecars with distinct new
UIDs/cache paths: compression0, no mipmaps, alpha-border fixing, premultiplication,
channel remapping or size limit. Prove exact decoded runtime RGBA from an empty
disposable cache and proof UID independence. Keep old imports byte-exact.

Allowed scene changes are one texture resource and the independent owner/child
in `game/Main.tscn`, plus one inactive environment-gated
`RuntimeLaundryGroundSmokeCheck` node/resource, its C# script and `.uid`.
Preserve every old resource/node block and sibling order; insert new roots before
`WholeChannelPresentation`, which remains final. Add one isolated sample/resource
in `DirectionAImportProof.tscn` and bounded live-copy parity in its import validator.

New tooling is limited to `scripts/validate-laundry-ground-runtime.py`, explicit
`scripts/capture-laundry-ground-runtime.gd` and `.uid`, plus a separate
`laundry-ground-runtime-promotion/` authorization/baseline/evidence package.
Allow bounded `scripts/quality.sh` orchestration and continuation documentation.
No other production file is selected; extensions need an updated named scope.

Freeze the complete actual implementation starting head after approval. Run
frozen artwork checks unchanged at `2fcc849`, Garden checks at `7942e7d`, and
Square checks at `06f0dab4`, Sickhouse checks at `bbcc14b1`, and Wagon checks at
`90dd8ccf`, plus confirmed Shrine checks at `5e5e3cab`. The successor must prove
their preserved live inputs plus only
these additions; never rewrite frozen inventories or validators.

## Preservation and acceptance

Preserve quiet moss, all route paint/alpha/transforms and topology, Laundry
shell/openings/physical bindings, accepted Shrine underlay/inscription/discovery
bindings, Sickhouse gravel/floor/walls/patient,
Garden soil/meadow/station, Square earth/cask, Wagon earth/shell/opening/clue,
and all physical/navigation/discovery authority. Preserve the channel's five
sprites, native three-node shader scope, UVs/caps/crossing, Shrine triangle,
Clearing near side, gate/Forester and x=1664/x=2000 limits. No gameplay/dialogue/
clue/discovery/treatment/save/provider/functional-UI/audio/release/signing change.

Require append-only manifest/master parity, exact normalized old scene blocks,
all predecessor bytes, independent imports and unchanged selected paint/mask.
Use disposable rejection probes for wrong phase/depth/transform, duplicate/dropped
owner, altered paint/coverage, import processing and physical/route changes.
Run full `PATH="/opt/homebrew/bin:$PATH" ./scripts/quality.sh`, scoped successor
validation and cold-import checks; distinguish sandbox failures from host retries.

Capture ordinary `Main.tscn` using the **actual imported runtime texture**, without
an injected external proof. Match six selected external views and six paired
baselines: target `(1620,320)`, 0.8x/1x/2x, 1280x720 and 1280x827, retained
camera clamps and identical disposable actor/HUD settings. Require exact paired
external/runtime pixel parity and changed pixels confined to guarded support.
Inspect entrance alpha edges, shell transparency and protected-ground separation.

Present all six actual runtime views for **separate owner runtime confirmation
before publishing the runtime promotion**. Approval of this scope supplies no
later visual confirmation, accessibility, pacing or terrain-completeness acceptance.

## Current external verification

The initial comparison packet's README/inventory retain their historical pending
status; the new [owner selection](../laundry-ground-feasibility/owner-selection.json)
and [validation](../laundry-ground-feasibility/evidence/validation.json) supersede
that status without rewriting the reviewed packet. Independent inspection passes
all masks/source RGB, 12 editable layers, 11 protected paint planes and 18 bounded
GPU comparisons. Source/master/board byte reproduction passes. Scope checks bind
43 inputs; ten altered-metadata probes are rejected. Successor preservation covers
7,801 published files, including every byte of the initial Laundry packet.

The direct live quality invocation stops at the frozen Shrine validator's new-file
membership gate. Keep it unchanged. Full product quality runs against accepted
`5e5e3cab` in a disposable checkout; the successor checker proves every live
product source and inherited evidence file unchanged. Exact command/outcomes
and logs are recorded in `verification.json` and `evidence/`. This validation
arrangement does not authorize runtime promotion.

## Next slice

Obtain approval of this README and its binding digest, then freeze the live
implementation baseline and install only the selected asset under this scope.
[Session review](session-review.json) records source/scope checks.

```sh
python3 art-direction/laundry-ground-runtime-scope/check.py
python3 art-direction/laundry-ground-runtime-scope/check_predecessor.py
```
