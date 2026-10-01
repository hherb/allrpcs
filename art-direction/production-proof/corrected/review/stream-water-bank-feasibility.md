# Stream water and bank feasibility proof

## Bound geometry

This external proof reproduces only the retained Stream presentation geometry:

- water band: `(1600, 452)` to `(2000, 548)`, exactly `400x96` pixels;
- crossing: `(1736, 452)` to `(1864, 548)`, exactly `128x96` pixels;
- west collision polygon: `(1600,452)`, `(1700,452)`, `(1700,548)`, `(1600,548)`;
- east collision polygon: `(1900,452)`, `(2000,452)`, `(2000,548)`, `(1900,548)`; and
- clear collision opening: `(1700,452)` to `(1900,548)`, exactly `200x96` pixels, containing the
  centered `128x96` crossing; and
- route chain: Village Edge to Stream to Abandoned Laundry.

## Minimum module set

The proof uses three assets: one opaque `128x96` water fill that repeats on both axes, one
horizontally repeatable `128x32` bank edge that is mirrored for the opposite bank, and one `32x32`
bank-opening cap that retains an exact bank connector and tapers to transparent. The mirrored bank
coverage ends exactly with the west/east collision polygons rather than narrowing the clear
200-pixel physical opening. The crossing stays
the existing `128x96` greybox presentation; no bridge or crossing asset is proposed by this slice.

Each export has an exact 2x layered OpenRaster master, manifest hash, and lossless mipmap-free copy
in `DirectionAImportProof.tscn`. Six deterministic views show the exact composition at 0.8x, 1.0x,
and 2.0x in 1280x720 and 1280x827 frames.

## Authority boundary

This proves only that the three-module set can cover the current geometry without closing the
crossing or obscuring the retained route approaches. The assets remain external and isolated.
`Main.tscn`, runtime art, collision, navigation, topology, camera behavior, scenario, and gameplay
are unchanged. Visual review does not authorize runtime promotion and does not establish pacing,
usability, or human accessibility acceptance.
