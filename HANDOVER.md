# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region layout, eleven anchors,
twelve topology edges, collision/navigation authority, closed north-west forest gate, camera tuning,
scenario, and gameplay authority remain unchanged. Direction A remains selected; Direction D is only
the contingency. The 66-export proof and seventeen OpenRaster masters remain proof-only.

Three exact manifest assets now have separate runtime-owned copies. `terrain.quiet_moss`, SHA-256
`3ef44b0912fec650bf4d40e1790e932e52083bdd3f197a5637aec75f5f1c98db`, repeats across the full
world at `z_index = -100`. `terrain.woodland_leaf_litter`, SHA-256
`f4caaf6f976f6f45cb4a0f22e9d978311422f95aa5d0d4e6233f122dafe1467a`, repeats across only the
Woodland Clearing bounds `(600, 200)` through `(1248, 450)` at `z_index = -90`. The straight
`terrain.woodland_leaf_litter_transition`, SHA-256
`8aea573d95b7fa8bc829c62e00bbaedf8fe2fef52c64019fed5054daf0b70334`, is rotated exactly 90
degrees and repeats across only that clearing's 648-pixel north edge at `y = 200` and
`z_index = -80`. All three remain behind retained routes, openings, labels, and interaction markers.

## Review, evidence, and limits

- The product owner accepted the supplied leaf-litter views at 0.8x, 1.0x, and 2.0x in 1280x720 and
  1280x827 shapes, including repetition, seams, scale/readability, contrast, the intentionally hard
  rectangular edge, and the corrected stone-marker description placement.
- The north-edge slice imports only the exact manifest straight transition. Its original repeatable
  top/bottom seam becomes the horizontal runtime repeat axis; no corner asset or other edge is used.
- The failure-first runtime validator rejected the missing third runtime asset. After implementation,
  `scripts/validate-runtime-terrain.py` proves all three exact manifest/source/runtime hashes,
  256x256 geometry, separate runtime UIDs/cache paths, lossless mipmap-free imports, the bounded
  runtime asset set, ordinary-scene references, and proof-directory isolation.
- `RuntimeTerrainGroundSmokeCheck` proves exact transition texture identity, quarter-turn, repeated
  648-pixel north-edge placement, and `-100 < -90 < -80` draw order. Session review removed fixed
  duplicate scene coordinates: the source now derives the center, north edge, quarter-turn, and
  repeat span from the same authoritative clearing binding used by the leaf-litter rectangle.
- Final host-local `./scripts/quality.sh` passed the art/proposal/import/runtime validators,
  zero-warning build, all 5,033 .NET tests (2,839 Domain, 925 Application, 1,261 Infrastructure,
  8 architecture), scenario validation, every credential-free Godot smoke, formatting, and the
  Compatibility launch.
- Automated checks prove provenance, geometry, import, placement, and mechanical isolation. No
  product-owner graphical observation has yet been supplied for the north-edge transition, and no
  human accessibility acceptance is claimed.

## Exact next bounded slice

Run one product-owner graphical review of the Woodland Clearing north-edge transition at 0.8, 1.0,
and 2.0 zoom in the supplied 1280x720 and 1280x827 shapes. Check horizontal repetition/seams,
transition softness and scale, route/opening/label contrast, its endpoints against the intentionally
hard west/east edges, and continued readability of the closed forest opening. Record only
observations the product owner actually supplies and correct only this transition's import,
placement, repeat span, rotation, or draw order, then rerun `./scripts/quality.sh`.

Do not promote either outer corner, either inset corner, another clearing edge, path art, banks,
water, structures, foliage, props, characters, animation, UI systems, effects, audio, or
content-budget fills before explicit acceptance. Do not change collision, navigation, locations,
topology, camera tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay
authority. After acceptance, choose one exact endpoint/corner slice rather than completing the whole
perimeter at once.
