# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region layout, eleven anchors,
twelve topology edges, collision/navigation authority, closed north-west forest gate, camera tuning,
scenario, and gameplay authority remain unchanged. Direction A remains selected; its 66 exports and
seventeen OpenRaster masters remain proof-only unless explicitly promoted below.

Seven exact manifest assets now have separate runtime-owned copies. Quiet moss remains at
`z_index = -100`; leaf litter remains inside only the unchanged Woodland Clearing bounds
`(600, 200)` through `(1248, 450)` at `-90`; the straight leaf transition and two complementary
outer corners form the derived rounded perimeter at `-80`. The product owner accepted the supplied
0.8x, 1.0x, and 2.0x wide/tall rounded-perimeter correction as "much better." Collision, navigation,
routes, topology, labels, openings, and gameplay authority did not change.

The next two runtime assets are `terrain.straight_path`, SHA-256
`3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd`, and
`terrain.path_end_cap`, SHA-256
`9e600a5552dec140aafa0a6f09258cff7094f61fe3c360960936a95d993ef5f3`. The 96x256 straight module
repeats only from the exact Abandoned Laundry anchor `(1500, 220)` to the Woodland Clearing anchor
`(1000, 350)`. Its exact 256x256 rounded cap terminates at the clearing anchor with the same derived
rotation. Both draw at `z_index = -70`; the other eleven retained greybox route lines draw at `-75`,
above terrain and below all ordinary landmarks, openings, labels, and interaction markers.

## Review, evidence, and limits

- Failure-first static validation rejected the absent sixth and seventh bounded runtime assets.
  `scripts/validate-runtime-terrain.py` now proves all seven exact manifest/source/runtime hashes,
  expected 256x256 or 96x256 geometry, separate runtime UIDs/cache paths, lossless mipmap-free
  imports, the exact runtime asset set, ordinary-scene references, and proof-directory isolation.
- The focused engine check proves twelve unchanged topology presentations: eleven `Line2D` greybox
  routes and one exact repeated path plus its cap. It binds anchor-derived position, length, rotation,
  repeat mode, `-90 < -80 < -75 < -70` terrain/route draw order, and landmark-safe layering.
- The first engine run exposed that presentation creation preceded exported-resource binding; it now
  remains separate from navigation/collision creation and runs at `_Ready`. A second run exposed the
  scene's required PascalCase C# export name. Both corrections are covered by the passing smoke.
- Session visual review rejected a pointed underlap and then a hard straight-path termination. The
  corrected route layering plus the exact matching end cap remove both defects in six final
  Compatibility-rendered 0.8x, 1.0x, and 2.0x wide/tall views. This is not product-owner acceptance.
- Final host-local `./scripts/quality.sh` passed the art/proposal/import/runtime validators,
  zero-warning build, all 5,033 .NET tests (2,839 Domain, 925 Application, 1,261 Infrastructure,
  8 architecture), scenario validation, every credential-free Godot smoke, formatting, and the
  Compatibility launch.
- Hosted Credential-free CI run `36566160795` for implementation commit `f99bdd6` had `steps: []`
  for all five jobs. It ran no checkout or product command and remains Actions/account infrastructure
  state, not product-validation evidence or a product failure.
- Automated checks do not establish pacing, usability, path-art acceptance, or human accessibility
  acceptance. Record only observations the product owner actually supplies.

## Exact next bounded slice

Run one product-owner graphical review of the Abandoned Laundry-to-Woodland Clearing straight path
and rounded clearing endpoint at 0.8x, 1.0x, and 2.0x in the supplied 1280x720 and 1280x827 shapes.
Check repeat seams, width/scale, edge softness, the rounded termination, moss/leaf contrast, and
continued readability of the focus, stone marker, opening markers, labels, and accepted perimeter.
If correction is required, change only these two path imports, anchor-derived path/cap geometry, or
route-presentation draw order, then rerun `./scripts/quality.sh`.

Do not promote the path bend, T-junction, cross-junction, another end cap, either inset ground corner,
another route, banks, water, structures, foliage, props, characters, animation, UI systems, effects,
audio, or content-budget fills. Do not change collision, navigation, locations, topology, camera
tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay authority. Do
not claim path-art or human accessibility acceptance until the product owner explicitly supplies it.
