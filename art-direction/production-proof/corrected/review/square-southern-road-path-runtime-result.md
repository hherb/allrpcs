# Square–Southern Road runtime promotion

The owner approved all six corrected external views, then approved the separate named
promotion with "approved" on 2026-10-02. The implementation is complete; the separate
ordinary-scene visual checkpoint remains pending.

One exact accepted `520x416` export presents the route at `(1700,1232)`, `z=-71`, pivot
`(260,208)`, with a `1040x832` layered OpenRaster master. Its SHA-256 is
`4225a6c49fd7a2c5fbcd2ec21b6190f537f6583df74fb80d39d885ca67b194d5`.
Visible layers retain the clipped continuation and exact 16 overlap columns; a hidden layer
retains the unclipped junction-relative reference. All 1,662 visible junction-intersection
pixels match; the route clips at unchanged Southern Road `(1500,1400)`.

Only the matching presentation line is replaced. The physical/navigation connection from
`(2200,900)` to `(1500,1400)` and its 96-pixel width remain unchanged. The sprite is unscaled,
unrotated, and explicitly disables repeat to match the accepted proof. The junction at `z=-70`,
other routes, road presentation/outline, and world markers/labels retain ownership and layering.
The flat endpoint across the unfinished road and partial covering by the road presentation
remain as accepted. High-zoom labels largely lie outside the frame; overview views provide
context. These views establish no UI, pacing, usability, or human accessibility acceptance.

Proof/runtime copies have independent generated UIDs, lossless compression, no mipmaps,
alpha-border processing, or premultiplied alpha. All 101 previous exports/masters remain
byte-identical. The proof now has 76 exports and 27 masters. Archived Sickhouse and Southern
Road full-builder baseline digests and enclosing pins were refreshed after verifying only
`main` changed in the builder; the sampling helper, generators, images, and other bindings
remain unchanged. The accessibility manifest digest was refreshed without changing evidence.

All six ordinary-scene PNGs are byte-identical to the accepted external correction. The capture
injects no art and edits no routes. It temporarily hides the Seeker and screen overlays and
uses the accepted proof's camera settings.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](square-southern-road-path-runtime-0.8x-wide.png) | [view](square-southern-road-path-runtime-0.8x-tall.png) |
| 1.0x | [view](square-southern-road-path-runtime-1.0x-wide.png) | [view](square-southern-road-path-runtime-1.0x-tall.png) |
| 2.0x | [view](square-southern-road-path-runtime-2.0x-wide.png) | [view](square-southern-road-path-runtime-2.0x-tall.png) |

Reproduction uses Pillow-enabled Python and the configured Godot .NET wrapper:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit-after 120
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
godot --path game --script "$PWD/scripts/capture-square-southern-road-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_square_southern_road_path.py --check-captures --check-promotion-scope
```

The optional promotion-scope check freezes this slice's scene and physical baseline. Regular
quality checks asset/master/import and archived capture parity. PNG bytes depend on the
renderer; these use Godot 4.6.3 .NET Compatibility on this macOS host.

Review covered the complete diff, master layers, import ownership/UIDs, presentation/physical
boundaries, and captures. It removed an obsolete alternate greybox boundary branch and corrected an overbroad endpoint
assertion that rejected the retained shrine/wagon connections. No unresolved finding remains. Repeat builds reproduced all 104 export/master/manifest files.
Focused scope/capture and negative alpha-import corruption checks passed. All quality validators,
formatting, zero-warning build, 5,033 tests, and scenario validation passed. The sandbox blocked Godot user-log writes; the identical host retry passed all 281 smoke
checks. Compatibility launch passed; no product workaround was introduced.
The owner runtime checkpoint remains pending; no following asset promotion is authorized.
