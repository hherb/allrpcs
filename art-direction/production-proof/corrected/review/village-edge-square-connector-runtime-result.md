# Village Edge–Village Square connector runtime result

The owner accepted the six supplied alpha comparison previews and approved the scoped promotion
after the visual checkpoint, replying "ok". That acceptance applies to the connector appearance;
it establishes no pacing, usability, accessibility, or additional asset/gameplay authority.

One new `terrain.village_edge_square_connector` asset has a `124x96` export, `(62,48)` pivot,
and a `248x192` layered OpenRaster master. Export SHA-256 is
`990b5cfa76c5e75e97adeef49a84a89a7dd15ea177a40804c414742d7f66a854`.
Its decoded pixels exactly match the accepted alpha strip, including original west alpha,
original columns `8..91`, and the exact corrected east-port blend. The master retains the
approved straight crop as a hidden reference and separate editable original, west, and east bands.
Rebuilding the connector export, master, and manifest is byte-identical.

The connector's isolated and runtime-owned copies have independent lossless import identities.
Only these two imports disable alpha-border processing: the default processing changed fringe
pixels in the first runtime comparison. Preserving the accepted 84 invisible RGB pixels and
disabling that processing makes every ordinary-scene capture byte-identical to its accepted
external preview. All other exports/imports retain their previous alpha rules and bytes.

The ordinary scene presents one centered sprite at `(1882,900)`, at `z=-70`, without scale or
rotation. Only the Square–Edge greybox presentation line starts at `(1820,900)` instead of
`(1944,900)`; its `(1700,900)` endpoint remains unchanged. Source guards retain the exact
collision/navigation methods and all scene content outside the new resource/binding lines.
Focused runtime checks retain the other three Square route continuations, the original route
count, five bounded art sprites, and marker/label layering. Accepted junction and Stream textures,
physical topology, camera, scenario, gameplay, dialogue, clues, discovery, treatment, save,
providers, UI, audio, release, and signing remain unchanged.

All six ordinary-scene views were inspected and match the accepted previews pixel-for-pixel and
PNG-byte-for-PNG-byte. The capture script changes only temporary review framing and Seeker
visibility; it injects no connector and edits no presentation line.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](village-edge-square-connector-runtime-0.8x-wide.png) | [view](village-edge-square-connector-runtime-0.8x-tall.png) |
| 1.0x | [view](village-edge-square-connector-runtime-1.0x-wide.png) | [view](village-edge-square-connector-runtime-1.0x-tall.png) |
| 2.0x | [view](village-edge-square-connector-runtime-2.0x-wide.png) | [view](village-edge-square-connector-runtime-2.0x-tall.png) |

```sh
python3 art-direction/production-proof/corrected/tools/build.py
godot --headless --editor --path game --quit
godot --path game --script "$PWD/scripts/capture-village-edge-square-connector-runtime.gd"
python3 art-direction/production-proof/corrected/tools/validate_village_edge_square_connector.py --check-captures --check-promotion-scope
```

`--check-captures` verifies the archived ordinary-scene pixels against the accepted six previews.
`--check-promotion-scope` additionally checks this slice's original Main scene and physical/code
baseline; later authorized slices may supersede that optional baseline. The regular quality suite
retains asset/master/import/capture checks without freezing unrelated future scene work.

Negative export-pixel, editable-band, and alpha-border-setting corruption checks fail closed and
restore their originals. Session review tightened the merged-master override so it can change
only RGB under zero alpha, retained default clean-alpha/import behavior for every other asset,
and refreshed only the accessibility report's manifest digest; its images and measurements are
unchanged. No unresolved implementation finding remains.
Full host-local `./scripts/quality.sh` passed all validators, formatting, zero-warning build,
5,033 tests, scenario validation, the complete credential-free Godot smokes, and Compatibility launch.

The next bounded step is the owner's ordinary-scene checkpoint. Automated identity with the
accepted external previews does not manufacture a separate human runtime response.
