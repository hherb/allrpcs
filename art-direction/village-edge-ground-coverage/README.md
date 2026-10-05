# Village Edge entrance ground-coverage audit

External-only audit at main `0b4d5884e0f90a341688a52ba0922c6ff2df3476`,
2026-10-05 (Australia/Sydney). No production asset, ordinary scene, physical owner
or existing approval changed.

Review the [complete six-view baseline](review/baseline-series.png),
[native entrance crops](review/entrances-1x.png),
[exact camera-2x crops](review/entrances-2x.png) and
[candidate support diagram](review/support-diagram.png).

The opaque quiet-moss field covers all three entrances. There are no unpainted
alpha holes. The Woodland, Stream and Square routes have exposed moss shoulders
and no independent packed-earth apron. This finding does not settle aesthetic
terrain completeness. The retained opaque Village Edge greybox shell hides
interior paint; this audit does not propose replacing it.

`baseline.json` binds all 7,886 starting tracked files. `inputs.json` and
`capture-evidence.json` bind every tracked game input, relevant acceptance records,
material and runtime owner transforms, native route alpha and physical boundary
segments. The actual opening centers are Woodland `(1580,805.7143)`, Stream
`(1750,700)` and Square `(1820,900)`; use captured geometry rather than historical
serialized greybox coordinates. Six unmodified-scene captures retain all ground,
channel, shell/openings, routes and labels. Only root HUD and characters are hidden.

`audit.json` records one optional external apron recipe: select each of three
exterior native route-alpha segments, dilate their union by a square radius 24,
then compare hard support and strictly inward 16/24-pixel smoothstep feathers.
Sample the approved 128px packed-earth field at absolute world phase `(0,0)`.
The envelope is 500x390 at `(1480,620)`; proposed z=-78 lies beneath all three
retained routes. A 2px support guard has zero alpha overlap with all five channel
sprites and seven accepted ground sprites. Full measurements, depth, exclusions
and hashes are in the audit. This is a feasibility recipe, not a selection or
runtime scope approval.

The subsequent [complete external comparison](../village-edge-ground-feasibility/README.md)
implements only that recipe in disposable viewports. The audit stays immutable.

Godot sandbox capture exited 134 without output; the identical host retry captured
all six views with Compatibility rendering. The initial clipped final board label
and its manifest are retained under `evidence/initial-board-review`; corrected
review framing is recorded in `session-review.json`.

Reproduce without writes:

```sh
python3 art-direction/village-edge-ground-coverage/build.py --check
python3 art-direction/village-edge-ground-coverage/verify.py
```

Fresh scoped checks pass. All 7,883 predecessor files outside the three maintained
continuation documents are unchanged, including every product and CI input.
The terminal Laundry acceptance and its passing zero-warning/5,033-test evidence
are verified and carried forward; no new full-suite run is claimed.
