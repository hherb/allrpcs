# Square–Healer Garden path runtime result

The owner accepted the six corrected external views, then separately replied "approved" to the
[named promotion scope](square-garden-path-promotion-proposal.md) on 2026-10-02.

One `terrain.square_garden_path` export preserves the exact accepted `200x220` PNG bytes,
SHA-256 `04c9ea2ea7a609dadcfcff268dc445300104f3e466fc003b304252feb411c8de`.
Its `(100,110)` pivot and world origin `(2200,1140)` place the unscaled, unrotated sprite at
`(2300,1250)`, `z=-71`, beneath the Square junction. Only the Square–Garden greybox presentation
line is replaced. The original anchors, 96-pixel physical/navigation route, all other routes,
Garden table/beds/foliage, markers, labels, and gameplay authority remain unchanged.

The `400x440` layered OpenRaster master contains exact editable junction-overlap and clipped
continuation bands plus a hidden unclipped reference. The flattened master exactly matches
nearest-neighbour 2x accepted pixels. All 1,400 visible overlap pixels retain exact junction RGBA.
Manifest provenance binds the source texture, accepted proof, phase, endpoint and geometry.
Export/master/manifest rebuilds are byte-identical; all 100 pre-existing export/master files
retain their bytes. The accessibility report changes only its manifest digest, preserving evidence.

Independent isolated and runtime-owned lossless imports have unique UIDs, no mipmaps, no
premultiplied alpha, and disabled alpha-border processing only for these new copies. The existing
connector retains its previous exception; every other import retains its settings.

All six ordinary-scene PNGs exactly match the accepted external correction, both decoded RGBA
and file bytes. The runtime capture recipe changes only temporary camera framing and Seeker
visibility; it injects no path and hides no presentation line.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](square-garden-path-runtime-0.8x-wide.png) | [view](square-garden-path-runtime-0.8x-tall.png) |
| 1.0x | [view](square-garden-path-runtime-1.0x-wide.png) | [view](square-garden-path-runtime-1.0x-tall.png) |
| 2.0x | [view](square-garden-path-runtime-2.0x-wide.png) | [view](square-garden-path-runtime-2.0x-tall.png) |

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit
godot --path game --script "$PWD/scripts/capture-square-garden-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_square_garden_path.py --check-captures --check-promotion-scope
```

The regular quality suite checks the new master/manifest/imports and archived captures without
freezing future scene/code changes. The optional promotion-scope check binds this slice's Main
scene and physical/navigation baseline. Older proofs' original scene hashes are intentionally
superseded; reproduce their captures from their documented pre-promotion revisions.

Session review tightened manifest/master geometry and import guards. Deliberate editable-layer
pixel corruption and alpha-border setting corruption failed closed and restored their originals.
The isolated proof sprite's initial centering error was corrected to match its scene convention.
No unresolved implementation finding remains. Quality passed all validators, formatting,
zero-warning build, 5,033 tests, and scenario validation. After the sandbox Godot log restriction
and isolated-proof centering fix, the full host Godot smoke suite passed, including exact path
ownership, replacement-line absence, retained route continuations, and Garden layering.
The ordinary Compatibility launch passed as well.

The six ordinary-scene views were displayed for the junction handoff, path edges, Garden endpoint,
and table/herb-bed/label layering checkpoint. The owner replied "ok" on 2026-10-02. This completes
bounded runtime visual acceptance only; no pacing, usability, accessibility, further asset, or
gameplay acceptance is inferred.

Next: an external-only feasibility proof of the existing straight-path texture on the retained
Square–Sickhouse continuation, preserving physical/navigation authority and all runtime files.
