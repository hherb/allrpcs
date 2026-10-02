# Southern Road–Wagon runtime result

The owner accepted the external incoming-tip trim and separately approved the named
`terrain.southern_road_wagon_path` promotion on 2026-10-02. These captures show ordinary
`Main.tscn` presentation, without injected textures or route edits. Only overlays/Seeker
visibility and camera framing change for capture, as in the accepted external proof.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](southern-road-wagon-path-runtime-0.8x-wide.png) | [view](southern-road-wagon-path-runtime-0.8x-tall.png) |
| 1.0x | [view](southern-road-wagon-path-runtime-1.0x-wide.png) | [view](southern-road-wagon-path-runtime-1.0x-tall.png) |
| 2.0x | [view](southern-road-wagon-path-runtime-2.0x-wide.png) | [view](southern-road-wagon-path-runtime-2.0x-tall.png) |

All six PNG hashes and decoded pixels match the [accepted trim](southern-road-wagon-path-trim.md).
The 560x432 export retains SHA-256
`34f1c1c28c5ee9f654953ba4d07ef2ac9d1fe6fe3f206fd04e68022395741799`.
Its 1120x864 OpenRaster master partitions the exact 2x pixels at column 360 into disjoint
editable junction and road layers, with the untrimmed correction as a hidden reference.
Proof/runtime imports own separate UIDs and caches, lossless compression, no mipmaps,
no premultiplication, and no alpha-border processing.

Ordinary startup replaces only the Southern Road–Wagon greybox route line with an unscaled
sprite centered at `(1680,1496)`, `z=-70`. The presentation road fill retains only
`(572,460)`–`(1500,1400)`. Upstream brown fill, straight boundary outlines, wagon occlusion,
physical route geometry, collisions, navigation, and all gameplay authority remain intact.

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit
godot --path game --script "$PWD/scripts/capture-southern-road-wagon-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_southern_road_wagon_path.py --check-captures --check-promotion-scope
./scripts/quality.sh
```

The scope validator restores the pre-promotion scene/manifest hashes and pins retained physical
and navigation authority. Regular quality validates export/master/import identity and all six
capture hashes. Earlier evidence records and images remain intact; their original full builder
is archived as `path-proof-build-source.py.gz`, with its digest and the live renderer checked.
The accessibility report's manifest digest was regenerated; its eight samples, 40 views,
measurements, and review pixels remain unchanged. No new accessibility acceptance is inferred.

Session cumulative review found no unresolved issue. Quality validators, formatting,
zero-warning build, all 5,033 .NET tests, scenario validation, the full host Godot smoke suite,
and Compatibility launch passed. The sandbox smoke failed in `RotatedFileLogger`; identical
host smoke/capture/launch retries succeeded. Negative promotion checks rejected altered pivot
and alpha-border processing. The sandbox editor imported the textures but could not save its
external editor settings; no product workaround was introduced.

The owner visually accepted these six ordinary runtime views on 2026-10-02 ("yes").
This is bounded visual acceptance only; no pacing, usability, accessibility, gameplay, or
next-asset promotion acceptance is inferred.
