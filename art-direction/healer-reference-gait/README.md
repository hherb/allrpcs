# Healer side walk on the reference gait model (#480)

The owner chose the Healer half of [#480](https://github.com/hherb/allrpcs/issues/480) on
2026-11-06 ([authority](authority.json)): re-author the Healer's walk to the
[reference gait model](../reference-gait-model.md) by code-rigging her approved paint. There is no
image generation and no new painted detail. The Sage never walks in Main, so his walk is deferred.

## What changes

- **West and east** get sixteen 45 ms poses each (720 ms cycle), replacing the 8 x 90 ms cutout
  gait from [walk-rig-polish](../characters-batch/walk-rig-polish/README.md).
- **South and north** keep their approved 8 x 90 ms frames byte for byte (`carried_forward` in
  [candidate.json](candidate.json)).

Each side pose follows the model. The heel lands with the toe raised, then the sole is planted flat
and slides back under the body. The foot rolls onto a trailing toe-off and lifts into the swing,
where the hidden knee leads the foot. Opposite legs are half a cycle apart, with brief double support at
each handover. Both thighs pivot at one hidden pelvis point on fixed-length bones; knees bend only
backward. The body height follows the supporting legs, so it dips at most 3.5 px (export scale)
when the feet are spread. The skirt hides the thighs and knees, so the visible gait is the boots
and the short leather shin below the hem.

## Paint used

[`build.py`](build.py) reuses the approved rig's extraction (`polish_walk.prepare_polished`): the
neutral idle body with the legs erased below the hem, the boot cut from the first walk study and
the leather connector patch. The only alpha edits are clearing the strip of skirt and stocking
above the boot shaft and the faint haze around the boot, which showed once the boot rotated.
The straight cut along the skirt bottom is inherited unchanged from the approved walk.

## Files

- `exports/healer-{west,east}-walk-NN.png`: 32 native 128 x 192 candidates.
- `masters/*.ora`: matching 2x OpenRaster masters.
- `review/healer-sides-old-vs-new.gif`: approved (left) and candidate (right) side walks at 2x.
  GIF frame delays are whole hundredths of a second, so the 45 ms poses play at 40 ms here.
- `review/healer-leg-identity-half-speed.gif`: leg A (anatomical left) red, leg B blue.
- `review/all-side-poses.png`: all 32 poses.
- [`candidate.json`](candidate.json): rig parameters, per-pose geometry, export hashes.

## Reproduce and check

```sh
python3 art-direction/healer-reference-gait/build.py      # refuses once owner-acceptance.json exists
python3 art-direction/healer-reference-gait/check_gait.py
```

`check_gait.py` re-renders the rig and checks the model rules on the actual geometry and paint:
fixed leg identity and bone lengths, no hyperextension, grounded stance soles, lifted swing,
heel-first contact and toe-off, the knee leading the foot, and brief double support. It also
checks that every export matches the builder and that the carried-forward front/back frames are
unchanged.

Limitations: cutout paint, so the upper body, arms and skirt are rigid and only the body height
moves.
