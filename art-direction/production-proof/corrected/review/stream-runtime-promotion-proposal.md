# Bounded Stream runtime promotion

Status: separately approved and implemented. The six external feasibility views were approved
first; the product owner then explicitly approved this exact runtime promotion.

## Exact change

Copy only the three existing manifest-bound exports into `game/art/runtime/terrain` with new,
runtime-owned Godot import identities. Keep their bytes, logical sizes, and lossless, mipmap-free
settings unchanged:

| Asset | Size | SHA-256 |
| --- | --- | --- |
| `terrain.stream_water` | 128x96 | `c9aaf4651f9e438fcfcae960641d8d66fbabba5db452cbbe53c39f7af17af07c` |
| `terrain.stream_bank` | 128x32 | `b63faba090a8c1d3427d9f9ad64f1d7a0da36600ca61d6419500590b8d629a8e` |
| `terrain.stream_bank_opening_cap` | 32x32 | `11bcc4d8113281c23adc4785d8134c2dd1dc6d9d8f00828ce19dac2686053ed1` |

In `Main.tscn`, replace only the `StreamWater` greybox fill with repeated water clipped to
`(1600,452)-(2000,548)`. Retain the existing centered `StreamCrossingArea` greybox at
`(1736,452)-(1864,548)` and draw it above the water. Add bank edges on the north and south sides
only over the retained collision spans. The north edge occupies `y=452..484`; the south edge
occupies `y=516..548` using vertically mirrored source pixels. On each edge, place the west bank
over `x=1600..1668`, its opening cap over `x=1668..1700`, the horizontally mirrored east cap over
`x=1900..1932`, and the east bank over `x=1932..2000`. Clip the repeated bank source to those
exact widths. The four caps must taper toward, but never draw inside, the clear
`x=1700..1900` opening. Match the approved proof composition and retain the existing route and
marker visibility.

## Acceptance checks

- Validate source, manifest, runtime PNG, import setting, and unique UID parity for all three
  assets; keep proof resources out of `Main.tscn`.
- Validate exact water and bank coverage, clipping, mirroring, draw order, crossing placement, and
  the unchanged west/east collision polygons. Check that the full 200-pixel opening remains clear.
- Run `./scripts/quality.sh` and inspect ordinary-scene Compatibility captures at 0.8x, 1.0x,
  and 2.0x in both 1280x720 and 1280x827 frames. Check seams, the Village Edge-Stream-Laundry
  route approaches, labels, markers, and crossing readability against the approved external views.
- Offer the six ordinary-scene views for a separate product-owner visual checkpoint. Automated
  evidence alone does not establish pacing, usability, or human accessibility acceptance.

Do not change accepted terrain, paths, junctions, clearing perimeter, foliage, table, beds,
Seeker art or animation, structures, props, other character art, UI systems, effects, audio,
collision, navigation, locations, topology, camera tuning, scenario, dialogue, clue, discovery,
treatment, save, provider, or gameplay authority.
