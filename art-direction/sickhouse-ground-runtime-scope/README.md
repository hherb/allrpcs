# Sickhouse pale-gravel runtime scope proposal

**Named runtime approval pending.** The owner approved the displayed 16px-feather
[external series](../sickhouse-ground-feasibility/README.md) on 2026-10-05.
[Bindings](source-bindings.json) freeze the exact proposal and selected inputs.
No production asset, import, ordinary scene or product code is installed by this packet.

Promote exactly one underlay, `terrain.sickhouse_entrance_ground`, from
`sickhouse-ground-feasibility/proof-inputs/feather-16-layer.png`, unchanged SHA-256
`216c49fbc5c3d640345b89db458414e7daec79f6204d0c7a7b63827ab2051a86`.
Do not import an opaque reconstruction, the hard support, the 24px alternative or
the unmasked gravel field as broad ground.

| Property | Proposed value |
| --- | --- |
| Owner | Independent root `Node2D`, `SickhouseGroundPresentation`, no script |
| Owner transform | Position `(2408,1248)`, z=0, relative depth, scale1, rotation0 |
| Child | `PaleGravel`, one 208x86 `Sprite2D`, local position/offset `(0,0)` |
| Child transform | centred=false, scale1, rotation0, white modulation, no region |
| Effective depth | -90, above quiet moss, below retained route and room floor |
| Sampling | Linear filtering, repeat disabled, world phase `(0,0)`, period128 |
| Envelope | `(2408,1248)`–`(2616,1334)`, right/bottom exclusive |
| Coverage | Exact selected route segment, radius24 support, inward16px feather |

Preserve the inherited floor occlusion, entrance marker and labels. No cropping,
repadding, recolouring, alpha editing, rotation, resampling or second layer.

## Allowed implementation after named approval

Use `sickhouse-entrance-ground.png` for exact-byte production, isolated-proof and
runtime copies under their existing terrain directories. Copy the selected
416x172 master unchanged to
`production-proof/corrected/masters/sickhouse-entrance-ground.ora`. Append one
manifest asset and provenance block through a new scoped
`production-proof/corrected/tools/build_sickhouse_ground.py`, preserving every
existing entry and master: **92 assets / 43 masters become 93 / 44**.

Add independent proof/runtime lossless `.import` sidecars with distinct new
UIDs/cache paths: compression0, no mipmaps, alpha-border fixing, premultiplication,
channel remapping or size limit. Prove exact decoded runtime RGBA from an empty
disposable cache and proof UID independence. Keep old imports byte-exact.

Allowed scene changes are one texture resource and the independent owner/child
in `game/Main.tscn`, plus one inactive environment-gated
`RuntimeSickhouseGroundSmokeCheck` node/resource, its C# script and `.uid`.
Preserve every old resource/node block and sibling order; insert new roots before
`WholeChannelPresentation`, which remains final. Add one isolated sample/resource
in `DirectionAImportProof.tscn` and bounded live-copy parity in its import validator.

New tooling is limited to `scripts/validate-sickhouse-ground-runtime.py`, explicit
`scripts/capture-sickhouse-ground-runtime.gd` and `.uid`, plus a separate
`sickhouse-ground-runtime-promotion/` authorization/baseline/evidence package.
Allow bounded `scripts/quality.sh` orchestration and continuation documentation.
No other production file is selected; extensions need an updated named scope.

Freeze the complete actual implementation starting head after approval. Run
frozen artwork checks unchanged at `2fcc849`, Garden checks at `7942e7d`, and
Square checks at `06f0dab4`. The new successor must prove their preserved live
inputs plus only these additions; never rewrite frozen inventories or validators.

## Preservation and acceptance

Preserve quiet moss, all route paint/alpha/transforms and topology, Sickhouse
floor/walls/entrance/patient bindings, Garden soil/meadow/station, Square earth/cask,
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
baselines: target `(2528,1290)`, 0.8x/1x/2x, 1280x720 and 1280x827, retained
camera clamps and identical disposable actor/HUD settings. Require exact paired
external/runtime pixel parity and changed pixels confined to guarded support.
Inspect entrance alpha edges, floor occlusion and protected-ground separation.

Present all six actual runtime views for **separate owner runtime confirmation
before publishing the runtime promotion**. Approval of this scope supplies no
later visual confirmation, accessibility, pacing or terrain-completeness acceptance.

## Next slice

Obtain approval of this README and its binding digest, then freeze the live
implementation baseline and install only the selected asset under this scope.
[Session review](session-review.json) records source/scope checks.

```sh
/opt/homebrew/bin/python3 art-direction/sickhouse-ground-runtime-scope/check.py
```
