# Village Square–Healer Garden external feasibility

The 2026-10-01 continuation tests only the existing straight-path texture on the retained
Square–Garden route. No production asset, editable master, manifest entry, import, ordinary scene,
physical/navigation geometry, or gameplay file is changed. The connector's earlier ordinary-scene
checkpoint is separately accepted. On 2026-10-02, the owner accepted this proof's six corrected
external views with "ok, go ahead" after their display. This is external visual acceptance only.

The Square anchor is `(2200,900)`, Garden anchor `(2300,1300)`, and Square footprint exit
`(2264,1156)`. The remaining route is `148.431803` pixels along heading `75.963757°`, retaining
96-pixel width. A centered, unscaled straight-texture strip at `(2282,1228)` rotated `-14.036243°`
leaves a triangular gap at the horizontal junction boundary and greybox fringes along its alpha
edges. All six failed views remain archived under `square-garden-path-proof-*`.

The correction samples the existing module from the Square pivot using the accepted junction's
exact heading and Pillow bicubic recipe. Its world-aligned `200x220` external overlay begins at
`(2200,1140)`, overlaps 16 pixels beneath the junction, and clips pixel centres at the Garden's
unchanged perpendicular endpoint. Every one of the overlap's 1,400 visible pixels exactly matches
the accepted junction RGBA. Zero-alpha pixels have clean RGB. Its temporary sprite at `(2300,1250)`
has no rotation or scale, draws at `z=-71` beneath the junction at `-70`, and hides only the matched
Square–Garden greybox presentation line in the disposable proof instance. Physical/navigation
authority is untouched. The preparation table, herb bed, markers, labels, other routes and assets
retain their ordinary-scene ownership and draw order.

| Zoom | Failed wide | Failed tall | Corrected wide | Corrected tall |
| --- | --- | --- | --- | --- |
| 0.8x | [view](square-garden-path-proof-0.8x-wide.png) | [view](square-garden-path-proof-0.8x-tall.png) | [view](square-garden-path-correction-0.8x-wide.png) | [view](square-garden-path-correction-0.8x-tall.png) |
| 1.0x | [view](square-garden-path-proof-1.0x-wide.png) | [view](square-garden-path-proof-1.0x-tall.png) | [view](square-garden-path-correction-1.0x-wide.png) | [view](square-garden-path-correction-1.0x-tall.png) |
| 2.0x | [view](square-garden-path-proof-2.0x-wide.png) | [view](square-garden-path-proof-2.0x-tall.png) | [view](square-garden-path-correction-2.0x-wide.png) | [view](square-garden-path-correction-2.0x-tall.png) |

Reproduce with Pillow-enabled Python and the configured Godot .NET Compatibility wrapper:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_square_garden_path_correction.py
godot --path game --script "$PWD/scripts/capture-square-garden-path-proof.gd"
godot --path game --script "$PWD/scripts/capture-square-garden-path-proof.gd" -- --corrected
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_square_garden_path_feasibility.py --check-live-baseline
```

The pinned evidence record binds recipes, source textures, all twelve captures, overlay, and the
optional frozen ordinary scene/code baseline. The validator rebuilds pixels, checks exact overlap
and endpoint containment, and rejects evidence drift. Archived PNG bytes are renderer-specific;
these captures use Godot 4.6.3 .NET Compatibility on this macOS host. Captures hide the Seeker and
temporarily reframe the camera; they provide no pacing, usability, or human accessibility evidence.

Session review inspected all six corrected views and the capture/generator/validator scope; an
outdated capture comment was corrected, and no unresolved implementation finding remains.
On 2026-10-02, focused baseline validation, Python lint, all quality validators, formatting,
zero-warning build, 5,033 tests, and scenario checks passed. The full Godot smoke suite and
Compatibility launch passed on the host after sandbox editor-settings/log restrictions prevented
the initial Godot smoke run. No product workaround was introduced.

The owner checkpoint covers the junction handoff, path edges, Garden endpoint, and
preparation-table/herb-bed/label layering. It establishes no runtime, pacing, usability, or
accessibility acceptance. Next: approve the separate
[named production and runtime scope](square-garden-path-promotion-proposal.md).
