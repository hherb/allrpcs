# Square–Southern Road path promotion proposal

The owner replied "approved" after display of all six corrected external views on 2026-10-02.
That completes external visual acceptance, including the recorded flat endpoint across the
unfinished road junction. This proposal names the separate production/runtime scope; approval
of that scope was granted with "approved" on 2026-10-02. The named implementation is now
complete; the ordinary-scene visual checkpoint remains pending.

## Exact proposed scope

- Create only `terrain.square_southern_road_path`: the accepted `520x416` world-aligned overlay,
  SHA-256 `4225a6c49fd7a2c5fbcd2ec21b6190f537f6583df74fb80d39d885ca67b194d5`,
  pivot `(260,208)`. Preserve exact accepted RGBA, 1,662 matching junction-intersection pixels,
  clean zero-alpha RGB, source phase, and endpoint `(1500,1400)`.
- Add one `1040x832` layered OpenRaster master using nearest-neighbour 2x accepted pixels.
  Keep editable clipped continuation, exact junction-overlap columns, and hidden unclipped
  sampling provenance. Its visible stack must reproduce the accepted pixels exactly.
  Preserve every existing source, export, and master.
- Bind export/master/provenance in the deterministic build and manifest. Add independent
  isolated-proof and runtime-owned lossless imports with unique UIDs, no mipmaps or premultiplied
  alpha. Disable alpha-border processing only for these new imports to preserve accepted pixels.
- Add one ordinary-scene resource/binding and one unscaled, unrotated sprite at `(1700,1232)`,
  `z=-71`, with texture repeat explicitly disabled as in the accepted proof. Replace only the
  matching Square–Southern Road greybox presentation line. Retain the Square junction at
  `z=-70`, road presentation/outline, all other routes and assets, and every marker/label.
  Preserve the accepted flat endpoint and current road layering; this scope includes no road
  junction redesign or additional terrain promotion.
- Add focused manifest/master/import/geometry/scope checks and ordinary-scene smoke coverage.
  Capture six ordinary-scene views without injecting art or hiding a route; match the accepted
  proof's temporary camera, Seeker, and screen-overlay settings. Compare exact PNG bytes and
  request the separate owner runtime checkpoint.

## Preserved boundaries and validation

Retain the physical/navigation `(2200,900)`–`(1500,1400)` connection and its 96-pixel width.
The sprite covers origin `(1440,1024)` and overlaps 16 columns beneath the junction's west edge.
This is presentation only. Preserve collision, topology, navigation, camera, scenario, dialogue,
clues, discovery, treatment, save, provider, UI, audio, release, and signing. Do not infer pacing,
usability, UI, or human accessibility acceptance. No other asset or route promotion is included.

Review the complete change, verify deterministic rebuilds and all prior asset/master hashes,
then run `./scripts/quality.sh` with Pillow-enabled Python. Retry only sandbox-blocked Godot
steps on the host. Adding this asset changes the full `build.py` digest pinned by the archived
Sickhouse and Southern Road proof records: refresh only those baseline digests and enclosing
evidence pins after verifying their sampling helper, generators, images, and other bindings
remain unchanged. Refresh required manifest-digest references while preserving existing evidence.

Prune `HANDOVER.md` and commit/push reviewed work directly to `main` under the existing
publication instruction. The [accepted external proof](square-southern-road-path-feasibility.md)
contains all six views, geometry, commands, hashes, limitations, and retained failed candidates.

Approval authorizes only this named implementation. Runtime visual acceptance requires the
later ordinary-scene checkpoint.
