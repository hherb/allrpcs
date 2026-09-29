# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region layout, eleven anchors,
twelve topology edges, collision/navigation authority, closed north-west forest gate, camera tuning,
scenario, and gameplay authority remain unchanged. Direction A remains selected; Direction D is only
the contingency. The 66-export proof and seventeen OpenRaster masters remain proof-only.

Five exact manifest assets now have separate runtime-owned copies. `terrain.quiet_moss`, SHA-256
`3ef44b0912fec650bf4d40e1790e932e52083bdd3f197a5637aec75f5f1c98db`, repeats across the full
world at `z_index = -100`. `terrain.woodland_leaf_litter`, SHA-256
`f4caaf6f976f6f45cb4a0f22e9d978311422f95aa5d0d4e6233f122dafe1467a`, repeats across only the
unchanged Woodland Clearing bounds `(600, 200)` through `(1248, 450)` at `z_index = -90`.
`terrain.woodland_leaf_litter_transition`, SHA-256
`8aea573d95b7fa8bc829c62e00bbaedf8fe2fef52c64019fed5054daf0b70334`, supplies the north and
south straight spans. `terrain.woodland_leaf_litter_corner_transition`, SHA-256
`53f2dba1b4e8012aca525d6a7a8bc02624a991c64427cc6b858f7339d72440bf`, and its complementary
`_left` asset, SHA-256 `c88c43456d9cdc94a6bb1f09883dc6edc3b03f36eddc6521bb8e3acf143aec24`,
supply four outer corners. The source derives two 392-pixel spans, corner centers, and a three-pixel
vertical visual overhang from the authoritative 648x250 clearing bounds and 256-pixel textures. The
rounded perimeter remains at `z_index = -80`, behind every retained route, opening, label, and
interaction marker. Collision, navigation, routes, topology, and gameplay authority did not change.

## Review, evidence, and limits

- The product owner accepted the leaf tile's repetition, seams, scale/readability, contrast, initial
  rectangular presentation, and corrected stone-marker text placement. During the later north-edge
  review, the owner said the remaining hard edges looked unnatural; the north-only presentation was
  therefore not accepted.
- The correction promotes only the two existing manifest-bound outer corners and reuses the already
  promoted straight transition. No inset corner or unrelated production asset entered `Main.tscn`.
- The failure-first runtime validator rejected the missing five-asset runtime set. After implementation,
  `scripts/validate-runtime-terrain.py` proves all five exact manifest/source/runtime hashes,
  256x256 geometry, separate runtime UIDs/cache paths, lossless mipmap-free imports, the bounded
  runtime asset set, ordinary-scene references, and proof-directory isolation.
- `RuntimeTerrainGroundSmokeCheck` proves exact texture identities, two quarter-turned 392-pixel
  spans at `(924, 197)` and `(924, 453)`, the four exact corner positions and rotations, and
  `-100 < -90 < -80` draw order. The focused host-local smoke passed.
- Final host-local `./scripts/quality.sh` passed the art/proposal/import/runtime validators,
  zero-warning build, all 5,033 .NET tests (2,839 Domain, 925 Application, 1,261 Infrastructure,
  8 architecture), scenario validation, every credential-free Godot smoke, formatting, and the
  Compatibility launch.
- Six Compatibility-rendered repository-backed views exist at 0.8x, 1.0x, and 2.0x in both 1280x720
  and 1280x827 shapes. Automated and session review found a continuous rounded boundary with routes
  and labels still above it; this is not product-owner acceptance.
- Hosted Credential-free CI run `36560348765` for implementation commit `9920ef0` reported failure
  with `steps: []` for all five jobs; it ran no checkout, build, test, Godot, or export step. Treat
  that as Actions/account infrastructure state, not hosted product-validation evidence or a product
  failure.
- Automated checks prove provenance, geometry, import, placement, and mechanical isolation. No
  product-owner observation has yet accepted the rounded correction, and no human accessibility
  acceptance is claimed.

## Exact next bounded slice

Run one product-owner graphical review of the rounded Woodland Clearing perimeter at 0.8x, 1.0x,
and 2.0x in the supplied 1280x720 and 1280x827 shapes. Check all six joins for seams, boundary
naturalness and scale, route/opening/label contrast, and continued readability of the closed forest
opening. Record only observations the product owner actually supplies. If correction is required,
change only these two outer-corner imports or the perimeter's derived positions, spans, rotations,
or draw order, then rerun `./scripts/quality.sh`.

Do not promote either inset corner, path art, banks, water, structures, foliage, props, characters,
animation, UI systems, effects, audio, or content-budget fills. Do not change the exact leaf-tile
bounds, collision, navigation, locations, topology, camera tuning, scenario, dialogue, clue,
discovery, treatment, save, provider, or gameplay authority. Do not claim rounded-perimeter or human
accessibility acceptance until the product owner explicitly supplies it.
