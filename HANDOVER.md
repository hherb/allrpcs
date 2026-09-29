# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region layout, eleven anchors,
twelve topology edges, collision/navigation authority, closed north-west forest gate, and accepted
camera tuning remain unchanged. Direction A remains selected; Direction D is only the contingency.
The 66-export proof and seventeen OpenRaster masters remain proof-only.

Two exact manifest assets now have separate runtime-owned copies. `terrain.quiet_moss`, SHA-256
`3ef44b0912fec650bf4d40e1790e932e52083bdd3f197a5637aec75f5f1c98db`, repeats across the full
3200x1800 world at `z_index = -100`. `terrain.woodland_leaf_litter`, SHA-256
`f4caaf6f976f6f45cb4a0f22e9d978311422f95aa5d0d4e6233f122dafe1467a`, repeats only across the
existing Woodland Clearing bounds `(600, 200)` through `(1248, 450)` at `z_index = -90`. Both sit
behind every retained greybox route, opening, label, and interaction marker. No proof-only path is
referenced by `Main.tscn`.

## Review, evidence, and limits

- The product owner said the quiet moss looked all right and that the supplied 1.0x and 2.0x views
  otherwise worked as intended. No separate 0.8x or wider/taller observation was supplied.
- The 2.0x view exposed one pre-existing overlap between the identified stone-marker description and
  the yellow Village Edge opening marker. Only that world-space label moved, from x offsets
  `-72..72` to `104..248`; its exact `(1184, 402, 144, 69)` rectangle now has an 8-pixel gap from
  the opening. A focused smoke failed before the scene edit and passes afterward. Camera, routes,
  opening markers, collision, navigation, focus, discovery, and gameplay behavior did not change.
- `scripts/validate-runtime-terrain.py` proves the two exact manifest/source/runtime hashes, 256x256
  PNG geometry, separate runtime UIDs/cache paths, lossless mipmap-free imports, bounded runtime
  asset set, ordinary-scene references, and proof-directory isolation.
- `RuntimeTerrainGroundSmokeCheck` proves repeat-enabled exact world and clearing coverage plus draw
  order. It now waits one process frame so every scene source has completed `_Ready` before checking
  its runtime-authored polygon; this changes only the explicit smoke path.
- Final host-local `./scripts/quality.sh` passed the art/proposal/import/runtime validators,
  zero-warning build, all 5,033 .NET tests (2,839 Domain, 925 Application, 1,261 Infrastructure,
  8 architecture), scenario validation, every credential-free Godot smoke, formatting, and
  Compatibility launch.
- Automated checks prove provenance, geometry, import, and mechanical isolation. No product-owner
  graphical observation has yet been supplied for the leaf-litter clearing, and no human
  accessibility acceptance is claimed.

## Exact next bounded slice

Run one product-owner graphical review of the leaf-litter Woodland Clearing at 0.8, 1.0, and 2.0
zoom and representative wider/taller window shapes. Check repetition, seams, scale/readability,
route/opening/label contrast, the intentionally hard rectangular edge pending transition art, and
the corrected stone-marker description placement. Record only observations the product owner
actually supplies and correct only leaf-litter import/coverage/draw-order or the observed label
placement defect, then rerun `./scripts/quality.sh`.

Do not promote a moss-to-leaf transition or another asset before explicit product-owner acceptance
of this clearing. Do not add paths, banks, water, structures, foliage, props, characters, animation,
UI systems, effects, audio, or content-budget fills. Do not change collision, navigation, locations,
topology, camera tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay
authority. After acceptance, choose one coherent transition-boundary slice rather than bulk terrain
integration.
