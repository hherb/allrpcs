# Southern Road–Wagon junction tip trim

On 2026-10-02 the owner said: "Seems to overshoot a bit to the left (east) side,
otherwise visually approved." This accepts the preceding correction except for the overshoot;
it does not authorize runtime promotion. This revision interprets "left" as the textured
junction tip below the upstream brown road endpoint. The screen-left/east ambiguity has been
raised with the owner. On 2026-10-02 the owner accepted the displayed trim ("Looks good");
the intended-side and adjusted-tip visual gate is complete.

Only the incoming Square extension changes, from 48 to 16 pixels. Road sampling, both
48-pixel backward overlaps, canvas origin `(1400,1280)`, size 560x432, center `(1680,1496)`,
and all disposable substitutions remain as documented in the [original correction](southern-road-wagon-path-correction.md).
The failed strip and all original correction recipes, images, and evidence are retained.
No production/runtime, physical/gameplay, manifest, or import file changes.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](southern-road-wagon-path-trim-0.8x-wide.png) | [view](southern-road-wagon-path-trim-0.8x-tall.png) |
| 1.0x | [view](southern-road-wagon-path-trim-1.0x-wide.png) | [view](southern-road-wagon-path-trim-1.0x-tall.png) |
| 2.0x | [view](southern-road-wagon-path-trim-2.0x-wide.png) | [view](southern-road-wagon-path-trim-2.0x-tall.png) |

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_southern_road_wagon_path_trim.py
godot --path game --script "$PWD/scripts/capture-southern-road-wagon-path-trim.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_southern_road_wagon_path_trim.py --check-live-baseline
```

The focused validator validates both previous evidence sets, pins this recipe and six views,
reproduces the overlay, and independently bounds all 2,789 changed pixels to the incoming tip.
No pixel gains alpha; no surrounding texture is changed. Image dimensions, transparent RGB,
canvas-edge containment, and the live runtime/manifest/geometry baseline pass. Repeated capture
reproduces all six PNG hashes. Code and six-view review found no unresolved implementation
issue. Full product tests/smokes passed in the preceding slice; this external-only adjustment
was checked with the focused evidence validator and actual Compatibility captures.

Next: obtain separate approval of the exact named production/runtime promotion proposed in
`HANDOVER.md`. The retained
upstream brown fill, straight outlines, wagon occlusion, and other views keep their bounded
visual acceptance; no pacing, usability, accessibility, gameplay, or promotion acceptance is inferred.
