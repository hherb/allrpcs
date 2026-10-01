# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete, and Direction A remains selected. The latest bounded Phase 9.2
slice creates only an external feasibility proof for the retained Stream water band and banks. It
adds three manifest-bound assets outside ordinary gameplay: `terrain.stream_water` (`128x96`,
SHA-256 `c9aaf4651f9e438fcfcae960641d8d66fbabba5db452cbbe53c39f7af17af07c`),
`terrain.stream_bank` (`128x32`, SHA-256
`b63faba090a8c1d3427d9f9ad64f1d7a0da36600ca61d6419500590b8d629a8e`), and
`terrain.stream_bank_opening_cap` (`32x32`, SHA-256
`11bcc4d8113281c23adc4785d8134c2dd1dc6d9d8f00828ce19dac2686053ed1`).

The proof binds the exact `(1600,452)-(2000,548)` water band, centered
`(1736,452)-(1864,548)` crossing, two retained collision polygons, and the 200-pixel clear collision
opening. Bank coverage ends at those collision polygons instead of visually narrowing the physical
opening. Six deterministic 0.8x, 1.0x, and 2.0x wide/tall views retain the Village Edge-Stream-
Laundry route approaches.

## Evidence and limits

- Failure-first validation rejected the former 69-asset manifest. The corrected pipeline now binds
  72 exact exports, 23 valid layered OpenRaster masters, water/bank seams, exact cap connectors,
  proof geometry, all six review hashes, and unchanged proof-only authority.
- All 72 exports have exact lossless, mipmap-free, unique-UID copies in the isolated
  `DirectionAImportProof.tscn`; its host-local smoke passed after the sandbox hit the known
  `user://logs` permission crash.
- The sandbox quality run passed every validator, the zero-warning build, all 5,033 .NET tests,
  and scenario validation before that known Godot crash. The identical host-local quality run then
  passed every validator, build/test/scenario gate, credential-free Godot smoke, formatting check,
  and Compatibility launch.
- `Main.tscn` has no proof-directory reference. Runtime art, collision, navigation, topology,
  camera behavior, scenario, and gameplay are unchanged.
- The product owner approved all six supplied feasibility views without requesting changes. This is
  external visual acceptance only; it does not establish pacing, usability, human accessibility,
  or runtime-promotion authority.

## Exact next bounded slice

Stop at the runtime-promotion authorization gate. Before editing, propose one exact bounded slice
that copies only the three approved manifest-bound Stream assets into runtime ownership, repeats
water across the existing `400x96` band, mirrors bank edges and caps only across the two retained
collision polygons, keeps the existing centered `128x96` crossing above them, and validates exact
hash/import parity, draw order, coverage, collision-opening alignment, route approaches, and six
ordinary-scene 0.8x/1.0x/2.0 wide/tall Compatibility views. Obtain separate explicit product-owner
approval before making that runtime change.

Do not copy these assets into runtime ownership or change `Main.tscn` without that approval. Do not
change accepted terrain, paths, junctions, clearing perimeter,
foliage, table, beds, Seeker art or animation, structures, props, other character art, UI systems,
effects, audio, collision, navigation, locations, topology, camera tuning, scenario, dialogue,
clue, discovery, treatment, save, provider, or gameplay authority.
