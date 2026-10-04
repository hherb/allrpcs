# Direction A Godot import proof

These 91 PNG files are exact byte-for-byte copies of the corrected production exports in
`art-direction/production-proof/corrected`. They are intentionally used only by
`DirectionAImportProof.tscn`; `Main.tscn` and ordinary gameplay do not reference them. The isolated
set now also contains the three feasibility-only Stream modules: repeatable water, a mirrored bank
edge, and one reusable bank-opening cap. They have no runtime, collision, navigation, topology,
camera, or gameplay authority.

Godot imports them as lossless 2D textures with mipmaps and premultiplied alpha disabled.
Alpha-border correction remains enabled for original sprites and disabled for accepted
path/channel overlays and both Garden underlays, preserving their exact translucent pixels.
The two Garden samples retain native 512x384 geometry, linear filtering and no repeat;
their ordinary-scene counterparts are independently owned runtime copies. Validation
also binds every sidecar to its exact resource path,
deterministic cache path, and unique UID. The proof scene inherits linear filtering from its root,
includes the exact 256x256 repeatable woodland leaf-litter tile, its straight, two outer-corner, and
two inset-corner quiet-moss transitions, and the 90-degree path-bend, T-junction, cross-junction,
and end-cap modules, shows the same
1280x720 composition, and exposes 12
named Seeker animations: four six-frame `walk_*` loops, four two-frame `idle_*` loops, and four
four-frame non-looping `interact_*` gestures. They retain the
manifest's exact frame order, 120 ms walk, 600 ms idle, and 160 ms interaction timing,
direction-specific dimensions, at least 144 visible logical pixels in every walk and idle frame,
the fixed 156-pixel contact origin on a 168-pixel canvas, and 48 unique texture resources. The scene
contains no collision, navigation, input, gameplay state, or UI authority.

Run the source/import metadata check and isolated engine smoke from the repository root:

```sh
python3 scripts/validate-direction-a-import-proof.py
godot --headless --path game res://DirectionAImportProof.tscn --quit-after 30
```

This import proof does not authorize replacement of greybox presentation, runtime direction
switching, bulk art production, gameplay/UI changes, audio, release, or signing work.
