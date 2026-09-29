# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region greybox retains all
eleven anchors, twelve topology edges, collision/navigation authority, the closed north-west forest
gate, and accepted 0.8/1.0/2.0 camera tuning. Direction A remains selected; Direction D is only the
contingency. The isolated 66-export proof and its seventeen OpenRaster masters remain proof-only.

The first bounded production promotion is complete. Only manifest asset `terrain.quiet_moss`, exact
SHA-256 `3ef44b0912fec650bf4d40e1790e932e52083bdd3f197a5637aec75f5f1c98db`, now has a separate
runtime-owned copy at `game/art/runtime/terrain/quiet-moss.png`. Its own lossless, mipmap-free Godot
import is repeated from the exact 256x256 source across the full 3200x1800 `Ground` region at
`z_index = -100`. `Main.tscn` references no proof-only path. All existing greybox presentation,
collision, navigation, locations, topology, scenario, dialogue, discovery, save, provider, and
gameplay authority remain unchanged.

## Review, evidence, and limits

- `scripts/validate-runtime-terrain.py` proves the exact manifest/source/runtime hash, 256x256 PNG
  geometry, independent runtime UID/cache path, lossless mipmap-free import, bounded runtime asset
  set, ordinary-scene reference, and proof-directory isolation.
- `RuntimeTerrainGroundSmokeCheck` proves the exact runtime texture resource, repeat-enabled full
  world region, origin, and draw order. It is part of `scripts/smoke-godot.sh` and the quality spine.
- Final host-local `./scripts/quality.sh` passed the art/proposal/import validators, zero-warning
  build, all 5,033 .NET tests (2,839 Domain, 925 Application, 1,261 Infrastructure, 8 architecture),
  scenario validation, every credential-free Godot smoke, formatting, and Compatibility launch.
- Cumulative review found the fixed-frame closed-forest smoke could outrun navigation readiness
  after the new texture import. Normal timing passed; increasing only its bounded synchronization
  allowance from 60 to 240 frames made the exact fixed-frame retry and final complete suite pass,
  matching the existing southern-road allowance without changing assertions or product behavior.
- No product-owner graphical observation was supplied for the runtime ground in this session.
  Automated checks prove mechanics and import invariants, not visual acceptance or accessibility.

## Exact next bounded slice

Run one product-owner graphical review of the repeated quiet-moss ground throughout the opening
region at 0.8, 1.0, and 2.0 zoom and representative wider/taller window shapes. Check for obvious
repetition, seams, scale/readability problems, greybox contrast, and framing, and record only the
observations the product owner actually supplies. Correct only ground tiling, import, or draw-order
defects exposed by that review, then rerun `./scripts/quality.sh`.

Do not promote another asset before explicit product-owner acceptance of this ground. Do not add
leaf litter, transitions, paths, banks, water, structures, foliage, props, characters, animation,
UI, effects, audio, or content-budget fills. Do not change collision, navigation, locations,
topology, camera tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay
authority. After acceptance, leave one new bounded production-asset slice rather than broadening the
review slice.
