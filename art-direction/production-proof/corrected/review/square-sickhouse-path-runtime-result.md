# Square–Sickhouse runtime promotion

The owner accepted all six corrected external views with "accepted", then approved the separate
named promotion with "yes" on 2026-10-02. The bounded implementation is complete. Its separate
ordinary-scene visual checkpoint is pending.

One exact accepted `380x360` export now owns the presentation at `(2510,1320)`, `z=-71`, with
pivot `(190,180)` and a `760x720` layered OpenRaster master. Its SHA-256 is
`0d7510a58d9fed28b1560b137d6c7f324a69bf3a39e3ac4a535f224de77172b7`.
The two visible master layers retain exact junction overlap and clipped continuation; a hidden
reference retains the unclipped sampling recipe. The accepted overlay matches all 1,713 visible
junction-intersection pixels and ends at unchanged Sickhouse `(2640,1450)`.

Only the matching greybox presentation line is replaced. The original physical/navigation
connection from `(2200,900)` to `(2640,1450)` retains its 96-pixel width. The junction, other
routes, Sickhouse floor/entrance/patient markers, Garden objects, and world labels retain their
ownership and draw order. The sprite is unscaled, unrotated, and uses explicit repeat sampling
to match the accepted proof. Initial comparison caught one-pixel clamp/repeat differences at
0.8x and 2.0x; matching the proof's sampling removed them.

Proof and runtime copies have independent generated import UIDs, lossless compression, no
mipmaps, alpha-border processing, or premultiplied alpha. No previous asset or master changed.
The corrected proof now has 75 exports and 26 editable masters. The accessibility report's
manifest digest was refreshed; its samples, views, and measurements remain unchanged. The
archived external evidence's full-build hash was refreshed after verifying its sampling helper
was unchanged; all image and other input hashes remain intact.

All six ordinary-scene PNGs are byte-identical to the accepted external correction. The capture
injects no art and edits no routes. It temporarily hides the Seeker and screen overlays and
uses the accepted proof's camera settings. The Sickhouse covers the far path terminus. Existing
Garden-label overlap at high zoom and partial Sickhouse-label cropping at 2.0x wide remain;
the tall view shows the full Sickhouse label. These views establish no UI, pacing, usability,
or human accessibility acceptance.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](square-sickhouse-path-runtime-0.8x-wide.png) | [view](square-sickhouse-path-runtime-0.8x-tall.png) |
| 1.0x | [view](square-sickhouse-path-runtime-1.0x-wide.png) | [view](square-sickhouse-path-runtime-1.0x-tall.png) |
| 2.0x | [view](square-sickhouse-path-runtime-2.0x-wide.png) | [view](square-sickhouse-path-runtime-2.0x-tall.png) |

Reproduction and focused checks use Pillow-enabled Python and the configured Godot .NET wrapper:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit-after 120
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
godot --path game --script "$PWD/scripts/capture-square-sickhouse-path-runtime.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_square_sickhouse_path.py --check-captures --check-promotion-scope
```

The optional promotion-scope check freezes this slice's scene and physical-authority baseline;
regular quality omits it while checking exact asset/master/import and archived capture parity.
PNG bytes depend on the renderer; these use Godot 4.6.3 .NET Compatibility on this macOS host.

Session review covered the complete scope, generator/master layers, independently derived
import paths/UIDs, scene presentation and physical-authority baseline, and all six runtime views.
It fixed the clamp/repeat sampling difference and isolated-proof top-left alignment; no unresolved
finding remains. Repeat builds preserved all 102 exports/master/manifest files, including all 99
prior asset/master hashes. Lint, focused scope/capture checks, and negative master-layer and
alpha-import corruption checks passed. All quality validators, formatting, zero-warning build,
5,033 tests, scenario validation, and full host Godot smokes passed. Sandbox editor-settings/log
restrictions required identical host retries; no product workaround was introduced.
Compatibility launch passed.

Next: obtain owner review of the six ordinary-scene views. After acceptance, test the existing
straight-path texture on the retained Square–Southern Road continuation in an external-only
feasibility proof, preserving all runtime and physical/gameplay authority. No further production
asset or runtime promotion is authorized by the Sickhouse approval.
