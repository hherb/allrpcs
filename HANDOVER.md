# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete. Direction A remains selected. The approved 3200x1800 opening
region, eleven anchors, twelve topology edges, closed forest gate, collision, navigation, camera,
scenario, and gameplay authority remain unchanged.

The corrected Phase 9.2 proof now contains 68 manifest-bound exports and nineteen editable
OpenRaster masters. The newest asset is `terrain.woodland_village_edge_transition`, SHA-256
`283f7231bfaa526c1dde34eb6bb709da67f998741841e53a06d17068ec1d2b21`: a deterministic 1024x512
world-aligned leaf overlay with a 2048x1024 master, exact clearing/opening geometry, and an irregular
64-pixel alpha feather. It has exact isolated-import and separate runtime-owned copies.

`Main.tscn` places the runtime copy at `(536, 136)`, unscaled and unrotated, at `z_index = -79`.
It covers only the existing 648x250 Woodland Clearing footprint plus the existing 96-pixel-wide,
180-pixel Village route continuation. The existing leaf tile, rounded perimeter, path art, greybox,
four route openings, collision, navigation, layout, and gameplay rules retain their prior authority.
The yellow `VillageEdgeOpening` presentation is hidden beneath the approved terrain/path art; the
other three clearing-opening markers remain visible.

## Evidence and limits

- Deterministic build and proof validation bind all 68 hashes, the overlay geometry and pivot,
  transparent corners, broad partial-alpha range, review image, and all nineteen valid OpenRaster
  archives.
- Static import/runtime validators bind exact source parity, distinct Godot UIDs/cache paths,
  lossless mipmap-free imports, the 1024x512 runtime geometry, and proof-path isolation.
- Final host-local `./scripts/quality.sh` passed all art/layout/import/runtime validators, formatting,
  a zero-warning build, 2,839 Domain, 925 Application, 1,261 Infrastructure, and 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the Compatibility launch.
  Godot's sandboxed `user://logs` failure required only the identical authorized host retry.
- Six Compatibility-rendered runtime views cover 0.8x, 1.0x, and 2.0x at 1280x720 and 1280x827.
  The product owner rejected the first paired-corner attempt, then rejected the tailored overlay's
  remaining north/northeast straight intersection. After the full-perimeter feather correction,
  the owner accepted the unobstructed and ordinary-scene views.
- That approval establishes only this visible moss/leaf blend. It does not establish pacing,
  usability, human accessibility, or any unreviewed asset acceptance.

## Exact next bounded slice

Stop at the next asset-selection authorization gate. Before editing, propose one exact bounded Phase
9.2 art slice, name its proof or runtime destination, intended live placement if any, acceptance
views, and exclusions, then obtain explicit product-owner approval. Do not treat the remaining broad
ROADMAP production checkboxes or this feather acceptance as approval for another asset.

Do not promote another route or asset. Do not change banks, water, structures, foliage, props,
characters, animation, UI systems, effects, audio, content-budget fills, collision, navigation,
locations, topology, camera tuning, scenario, dialogue, clue, discovery, treatment, save, provider,
or gameplay authority. Do not claim pacing, usability, or human accessibility acceptance.
