# Laundry east-entrance packed-earth study

Complete external comparison, 2026-10-05 (Australia/Sydney). Owner preference and
revisions are pending. No runtime promotion is selected or authorized.

Review the entire series together:

- [Actual wide scene series](review/gpu-series-wide.png)
- [Actual tall scene series](review/gpu-series-tall.png)
- [Complete actual-scene edge crops](review/complete-edge-series-2x.png)
- [Moss/route substrate comparison](review/comparison-1x.png)

Columns are baseline, hard support, feather-16 and feather-24. Each GPU board has
0.8x, 1x and 2x camera rows; all 24 full-size captures are retained. Edge crops are
exact GPU pixels at camera 2x. Substrate boards omit scene shells/woodland/channel;
use actual-scene boards for the visual decision. Feather-24 is the agent's visual
preference for the softer woodland blend; this is not an owner selection.

## Recipe and provenance

The retained Laundry east opening is centered at (1580, 294.6667), width 96.
The accepted Stream–Laundry route is 432x416 at world origin (1424,152), z=-75.
Use only its nonzero alpha in world columns [1588,1632), dilated by a square
radius 24. The complete support is bounded to 92x210 at (1564,219). Two inward,
zero-padded smoothstep feathers are 16 and 24 source pixels. Unchanged approved
128px packed-earth paint is sampled at absolute world phase (0,0); no scaling,
rotation or repainting. Proposal z=-78 lies above woodland -80/-79 and below
route -75. The retained Laundry shell/opening occludes the underlay normally.
The support and its filtering guard remain clear of accepted channel paint.

Three independent editable 184x420 OpenRaster masters retain visible masked
paint, an empty Corrections layer, hidden coverage and hidden raw world paint.
Zero-alpha export texels have zero RGB. Baseline, source and output digests are
in `baseline.json`, `selection.json`, `inventory.json`, `capture-evidence.json`
and `review-board-manifest.json`.

`capture.gd` instantiates the retained Main scene only in a disposable viewport,
adds external ImageTextures and never saves/packs the scene or imports proof
textures. Root UI and characters are hidden; all physical/presentation owners,
labels, ordinary camera limits and accepted ground remain. The sandboxed Godot
attempt exited 134 without output; the identical host capture completed all views.

## Review and continuation

Code and visual review are recorded in `session-review.json`. Initial generation,
image opening and capture/board production are complete. Consolidated validation
and full quality are deliberately deferred until the collective comparison and
any revisions are settled, as required by the live handover. This packet records
no owner approval or completed validation claim.

After agreement, run:

```sh
python3 art-direction/laundry-ground-feasibility/build.py --check
python3 art-direction/laundry-ground-feasibility/boards.py --check
python3 art-direction/laundry-ground-feasibility/inspect_package.py
./scripts/quality.sh
```

Keep this packet immutable; retain revisions in a separate packet. Record the
owner's selected variant and exact file hashes separately. Only then prepare a
named runtime scope; source selection alone does not authorize runtime changes.
Preserve accepted paint, masters, masks, route geometry, sibling order, collision,
navigation, layout, gameplay, providers, functional UI, audio and release/signing.
