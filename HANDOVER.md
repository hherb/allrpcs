# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 now has a deterministic
62-asset corrected pipeline with thirteen editable OpenRaster masters. The terrain proof includes
quiet moss, repeatable woodland leaf litter, one quiet-moss-to-leaf transition, and the complete
straight/bend/T/cross/end-cap packed-earth set. Exact copies of all 62 exports exist only in the
isolated Godot import proof; `Main.tscn` and ordinary gameplay reference none of this art.

## Evidence and boundaries to retain

- `asset-manifest.json` owns all 62 exports with source provenance, exact 2x geometry, pivots,
  baselines, digests, animation timing, and isolated-import status. `tools/build.py` reproducibly
  creates thirteen `.ora` masters and all review evidence from eighteen retained image-tool sources
  plus the earlier repository-owned walk sheet.
- `terrain.woodland_leaf_litter` is one opaque 256x256 export with an exact 512x512 two-layer master.
  Its opposing edges match exactly; focused validation proves useful tonal variation, measurable
  contrast from quiet moss and packed earth, and no 96-pixel full-span path-like corridor.
- The leaf source was made with Codex's built-in image tool using only the repository-owned quiet
  moss and packed-earth topology review. The exact prompt is retained; the tool exposed no
  model/version, and no third-party content or licence claim was introduced.
- `terrain.woodland_leaf_litter_transition` is derived deterministically from the exact moss and leaf
  masters. Its 256x256 export is fully opaque, retains exact 32-pixel source connector regions, and
  repeats exactly top-to-bottom. The 512x512 `.ora` retains both sources as hidden editable layers.
- `review/woodland-leaf-litter-review.png` proves the four 2x2 tile seams beside moss and packed
  earth. `review/woodland-leaf-litter-transition-review.png` proves two exact connected rows from
  moss through the irregular transition into leaf litter.
- `game/art/direction-a-import-proof` contains exact byte copies of all 62 exports with lossless
  imports, mipmaps off, alpha-border correction on, and premultiplied alpha off. The proof scene
  checks both new textures as uncentred 256x256 sprites with linear filtering and no collision,
  navigation, input, gameplay, or UI authority.
- Host-local `./scripts/quality.sh` passes both asset validators, all 62 exact import hashes,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, formatting, and Compatibility graphical
  launch. The restricted run first hit the known Godot settings/`user://logs` failure; only the
  identical full command was retried on the host.
- These remain pipeline proofs, not final production quality or evidence of runtime terrain
  assembly, animation selection, collision, navigation, gameplay/UI, accessibility, complete-region
  art, audio, release, or signing readiness.

## Exact next bounded slice

Extend only the external corrected pipeline with one 90-degree quiet-moss/woodland-leaf-litter
corner transition derived from the existing exact ground and straight-transition masters. Produce
one opaque 256x256 logical export and exact 512x512 editable master whose boundary enters at the
top midpoint and exits at the right midpoint, with leaf litter in the upper-right and quiet moss on
the left and lower edges. Retain exact connector-depth parity with the existing straight transition
and its deterministic rotation, exact unused outer-edge parity with the source ground tiles, a
repeatable corner review, manifest provenance, digest, and focused validator coverage. Keep it
outside `game/` until those external gates pass.

Do not add the opposite corner, concave/inset transitions, banks, water, structures, props,
character art, runtime terrain assembly or animation selection, ordinary Seeker replacement,
gameplay/UI/collision/navigation authority, audio, release, or signing work. After the corner passes
externally, the following slice may import only that exact manifest-bound asset into the isolated
proof.
