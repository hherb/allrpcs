# Square–Healer Garden path promotion proposal

On 2026-10-02, the owner accepted the six corrected external views with "ok, go ahead" after
their display. That completes external visual acceptance. This proposal names the separate
production and runtime scope; it has not yet been approved.

## Exact proposed scope

- Create only `terrain.square_garden_path`: the accepted `200x220` world-aligned overlay, SHA-256
  `04c9ea2ea7a609dadcfcff268dc445300104f3e466fc003b304252feb411c8de`, pivot `(100,110)`.
  Preserve its exact RGBA pixels, 1,400 matching junction-overlap pixels, clean zero-alpha RGB,
  existing texture phase, and unchanged Garden endpoint.
- Add one `400x440` layered OpenRaster master using nearest-neighbour 2x accepted pixels, with
  editable continuation and endpoint-clipping provenance. The flattened master must exactly
  reproduce those pixels; no existing master, source texture, or export may change.
- Bind the export/master/provenance in the deterministic build and manifest, then add independent
  isolated-proof and runtime-owned lossless imports with unique UIDs, no mipmaps or premultiplied
  alpha. Disable alpha-border processing only for these new imports to preserve accepted pixels.
- Add one ordinary-scene resource/binding and one unscaled, unrotated sprite at `(2300,1250)`,
  `z=-71`, matching the temporary corrected proof. Replace only the Square–Garden greybox
  presentation line with that sprite. Retain the Square junction at `z=-70` and all other route
  presentation, Garden table, beds, foliage, markers, and labels.
- Add focused manifest/master/import/geometry/scope checks and ordinary-scene smoke coverage.
  Capture six ordinary-scene views without injecting art or hiding a route line, compare them
  against the accepted external views, and request the owner's separate runtime checkpoint.

## Preserved boundaries and validation

Keep the original `(2200,900)`–`(2300,1300)` physical/navigation connection and its 96-pixel width.
The external sprite covers world origin `(2200,1140)` and uses a 16-pixel overlap beneath the
Square junction; this is presentation only. Preserve collision, topology, navigation, camera,
scenario, dialogue, clues, discovery, treatment, save, provider, UI, audio, release, and signing.
No other path or asset promotion is included. Do not infer pacing, usability, or accessibility
acceptance from these views.

Review the cumulative change, validate rebuild determinism and all prior asset hashes, run
`./scripts/quality.sh` with Pillow-enabled Python (identical host retry only for sandbox-blocked
Godot steps), prune `HANDOVER.md`, and commit/push reviewed work to `main` under the existing
publication instruction. Any manifest-digest reference refresh must preserve its existing evidence.

The [external proof result](square-garden-path-feasibility.md) contains all accepted views,
geometry, reproducible capture commands, pinned evidence, and archived failed candidates.

Next: obtain explicit approval of this named promotion scope before creating the production
asset/master/imports or changing runtime presentation.
