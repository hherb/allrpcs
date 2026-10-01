# Square–Sickhouse path promotion proposal

On 2026-10-02, the owner replied "accepted" after display of all six corrected external views.
That completes external visual acceptance. This proposal names the separate production/runtime
scope; its approval is pending.

## Exact proposed scope

- Create only `terrain.square_sickhouse_path`: the accepted `380x360` world-aligned overlay,
  SHA-256 `0d7510a58d9fed28b1560b137d6c7f324a69bf3a39e3ac4a535f224de77172b7`,
  pivot `(190,180)`. Preserve exact accepted RGBA, 1,713 matching junction-overlap pixels,
  clean zero-alpha RGB, texture phase, and unchanged Sickhouse endpoint.
- Add one `760x720` layered OpenRaster master using nearest-neighbour 2x accepted pixels,
  with editable continuation and endpoint-clipping provenance. Its flattened pixels must
  reproduce the accepted overlay exactly. Preserve all existing masters, textures, and exports.
- Bind export/master/provenance in the deterministic build and manifest. Add independent
  isolated-proof and runtime-owned lossless imports with unique UIDs, no mipmaps or premultiplied
  alpha. Disable alpha-border processing only for these new imports to preserve accepted pixels.
- Add one ordinary-scene resource/binding and one unscaled, unrotated sprite at `(2510,1320)`,
  `z=-71`, exactly matching the disposable corrected proof. Replace only the Square–Sickhouse
  greybox presentation line. Retain the junction at `z=-70`, all other routes, the Sickhouse
  structure/entrance/patient markers, Garden table/bed, foliage, and world labels.
- Add focused manifest/master/import/geometry/scope checks and ordinary-scene smoke coverage.
  Capture six ordinary-scene views without injecting art or hiding a route line; use the same
  temporary camera, Seeker, and screen-overlay settings as the accepted proof. Compare to the
  accepted external views and request the separate owner runtime checkpoint.

## Preserved boundaries and validation

Retain the physical/navigation connection `(2200,900)`–`(2640,1450)` and its 96-pixel width.
The sprite's world origin is `(2320,1140)` with 16 overlap rows beneath the junction. This is
presentation only. Preserve collision, topology, navigation, camera, scenario, dialogue, clues,
discovery, treatment, save, provider, UI, audio, release, and signing. Include no other path or
asset promotion. Leave the recorded existing high-zoom label overlap/cropping unchanged.
Do not infer pacing, usability, or human accessibility acceptance.

Review the complete change, validate deterministic rebuilds and all prior asset/master hashes,
and run `./scripts/quality.sh` with Pillow-enabled Python. Retry only sandbox-blocked Godot
steps on the host. The archived proof pins the full `build.py` file: update only that baseline
digest and its enclosing evidence pin when adding the named asset, after verifying that its
sampling helper, accepted overlay, and all archived views remain unchanged. Refresh required
manifest-digest references while retaining their existing evidence.

Prune `HANDOVER.md` and commit/push reviewed work directly to `main` under the existing
publication instruction. The [accepted proof result](square-sickhouse-path-feasibility.md)
contains all six views, geometry, capture commands, hashes, and retained failed candidates.

Approval of this proposal authorizes only this named implementation. Runtime visual acceptance
requires the later ordinary-scene checkpoint.
