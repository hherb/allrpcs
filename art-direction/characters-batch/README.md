# Characters artwork batch 01

Initial Phase 9.2 character artwork series for collective review, produced on
2026-10-04 (Australia/Brisbane) with the built-in image_gen tool, followed by
owner-authorized code-based cutout rigging. **Artwork acceptance remains pending.** The initial similar faces were revised following owner feedback; the
first pass remains in `rejected/`.

The eight authored roles are Seeker, Healer, Elian, Sera, Orin, Vale, Rowan and Mira.
New art includes eight full-body designs, eight portraits, 32 directional frames each
for seven companion/encountered roles and the Seeker plain-tunic/work-apron choices,
plus eight sleeping/breathing/waking/recovery poses for Rowan and Mira: **312 new
transparent exports and 312 editable 2x OpenRaster masters**. The approved Seeker
travelling-cloak set's 48 existing frames are referenced by hash, not regenerated.

## Designs and portraits

![Cast designs](review/cast.png)
![Revised individual portraits](review/portraits.png)
![Seeker appearance choices](review/seeker-appearances.png)

Face silhouettes, noses, hairlines, haircuts and beard shapes distinguish the cast.
Costumes retain Direction A's cream, ochre, moss, teal, indigo and russet gouache detail.
These are visual proposals; they do not change domain identity or customization rules.
Vale retains an ordinary courier appearance, without exposing the private enemy role.

## Initial directional source series

Each board has south, west, east and north rows. Each row contains two idle, four
incomplete walking studies and two inspection frames. The owner correctly identified that
the walking previews keep the same foot leading: these are **not full alternating gait cycles**.
The walk GIFs are labelled incomplete; filenames retain `walk` for provenance only.
Do not approve or promote them as complete animations. Three replacement generation attempts,
including an explicit pose guide and colored-boot diagnostic, also failed and are retained.
The owner then explicitly authorized mechanical rigging and rendering.
[Replacement walk series](walk-rig/README.md) contains 288 eight-phase cutout frames
for all nine families in four directions. The 312 initial assets remain unchanged as
lineage; substituting the replacement walks gives 456 active new candidates. The new
loops alternate anatomical left/right contact and swing, with exact loop endpoints.
Their simple cutout style, restrained upper-body motion and timing still need visual review.

![Healer](review/healer.png)
![Healer walk](walk-rig/review/healer-walk.gif)
![Elian](review/elian.png)
![Elian walk](walk-rig/review/elian-walk.gif)
![Sera](review/sera.png)
![Sera walk](walk-rig/review/sera-walk.gif)
![Orin](review/orin.png)
![Orin walk](walk-rig/review/orin-walk.gif)
![Vale](review/vale.png)
![Vale walk](walk-rig/review/vale-walk.gif)
![Rowan](review/rowan.png)
![Rowan walk](walk-rig/review/rowan-walk.gif)
![Mira](review/mira.png)
![Mira walk](walk-rig/review/mira-walk.gif)
![Seeker plain tunic](review/seeker-plain.png)
![Seeker plain tunic walk](walk-rig/review/seeker-plain-walk.gif)
![Seeker work apron](review/seeker-apron.png)
![Seeker work apron walk](walk-rig/review/seeker-apron-walk.gif)

Each role also has `<role>-idle.gif` and `<role>-inspect.gif` in `review/`.
The unchanged approved cloak frames remain in
`../production-proof/corrected/exports/characters/seeker/`; their three labelled
boards remain in `../production-proof/corrected/review/seeker-four-direction-*.png`.

## Dormancy and proposed scale

![Sleeping and recovery poses](review/dormant.png)
![Cast at proposed export scale](review/cast-scale.png)

The scale arrangement is external artwork review, not a runtime capture or layout proposal.
New directional frames have 128x192 canvases and a shared scale within each role, with
figures at most 144 pixels high. Final common foot registration, frame cadence, loop smoothness, camera consistency,
directional accessory continuity, reach semantics and occlusion require review before
any runtime proposal. Waking poses are artwork, not an implemented controller transition.

## Editable package and reproduction

- `sources/`: twelve generated RGBA sheets, unchanged after their recorded generation/edit calls.
- `prompts.json`: exact prompt set, transparency request, references and revision provenance.
- `exports/`: 312 extracted RGBA candidates.
- `masters/`: exact 2x raster source-paint layers with a separate blank correction layer.
  They are editable raster masters, not semantic head/body/clothing layers; source sheets
  retain the original generated resolution. Resizing does not add original paint detail.
- `inventory.json`: source crop rectangles, asset identities, dimensions, hashes and provisional pivots.
- `review/`: fourteen labelled boards/compositions and 27 animated cycle previews.
- `rejected/`: superseded face and layout candidates retained as evidence.

Reproduce using `/opt/homebrew/bin/python3 art-direction/characters-batch/build_review.py`.
Extraction locates complete animation figures from substantial connected alpha, retains
the original alpha within padded crop bounds and proposes bottom-center registration.
The detection threshold is not applied to exported alpha. No source paint is repainted or
mirrored. Rowan's generated side rows are reordered to label their actual facing correctly.
Sleeping source gutters are measured individually to retain complete reclining figures.
The Seeker work-apron and Healer inspection side views received targeted generated corrections.
The builder protects a subsequently approved package from accidental overwrites;
create a separate candidate package for further approved-artwork revisions.

`inspect_package.py` runs only authoring checks: completeness, alpha integrity, crop-edge
warnings, image integrity, hashes, exact 2x parity and unchanged inherited Seeker bytes.
`review-manifest.json` records the candidate snapshot; it is **not owner acceptance**.
The next slice is collective review and requested polishing of the mechanically alternating
walk series. `rig_walk.py` reproduces it; `inspect_walk_rig.py` checks its 36 complete loops.
Consolidated validation follows artwork review and requested revisions. When product/runtime/CI
inputs remain unchanged, retain and verify the preceding full-suite evidence instead of
replaying unchanged Godot smokes. Runtime promotion requires named scope approval and visual
confirmation. No product code, runtime manifests/imports, scenes, collision, navigation,
dialogue, clues, provider, save, UI or audio changes are included.
