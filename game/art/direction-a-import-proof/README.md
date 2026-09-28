# Direction A Godot import proof

These 32 PNG files are exact byte-for-byte copies of the corrected external proof exports in
`art-direction/production-proof/corrected`. They are intentionally used only by
`DirectionAImportProof.tscn`; `Main.tscn` and ordinary gameplay do not reference them.

Godot imports them as lossless 2D textures with mipmaps disabled, alpha-border correction enabled,
and premultiplied alpha disabled. The proof scene inherits linear filtering from its root, shows the
same 960x540 composition, and exposes four named six-frame Seeker animations: `walk_south`,
`walk_west`, `walk_east`, and `walk_north`. Each loop retains the manifest's exact frame order and
120 ms timing, direction-specific dimensions, fixed contact origin, and 24 unique texture resources.
The scene contains no collision, navigation, input, gameplay state, or UI authority.

Run the source/import metadata check and isolated engine smoke from the repository root:

```sh
python3 scripts/validate-direction-a-import-proof.py
godot --headless --path game res://DirectionAImportProof.tscn --quit-after 30
```

This import proof does not authorize replacement of greybox presentation, runtime direction
switching, bulk art production, gameplay/UI changes, audio, release, or signing work. The separate
external idle/breathe proof is intentionally not copied into this import set yet.
