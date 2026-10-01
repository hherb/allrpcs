# Village Edge–Village Square connector feasibility proposal

Status: proposed. Obtain separate product-owner approval before building this proof. This proposal
does not authorize an ordinary-scene change.

## Exact bounded proof

Test only the existing manifest-bound `terrain.straight_path` export (`96x256`, SHA-256
`3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd`) as the
horizontal connector between the retained Village Edge east opening and the accepted Village
Square junction. The live route is centered at `y=900`. The Village Edge east boundary is
`x=1820`; the 512x512 Village Square junction centered at `(2200,900)` begins at `x=1944`.
The intervening corridor is therefore exactly `x=1820..1944`, `y=852..948`: `124x96` pixels.

In an external review composition, rotate the unchanged straight-path export 90 degrees to run
east–west, crop it to the exact 124-pixel span, and join its east end to the junction's west
96-pixel port. Retain the greybox Village Edge interior to the west of `x=1820`. Show the actual
Village Edge boundary and opening, the accepted runtime junction, nearby labels and markers, and
the current route approaches. Do not alter the export, its master, the junction, or the live scene.

## Acceptance checks

- Bind the manifest hash and original `96x256` dimensions. Verify the cropped and rotated texture
  stays within `x=1820..1944`, `y=852..948` with no scaling, off-route pixels, or gap.
- Compare the exact texture port pixels at `x=1944` with the accepted Village Square junction's
  west port. Inspect the `x=1820` edge where the texture meets the Village Edge greybox interior;
  report a visible seam as a failed feasibility result rather than silently extending the art.
- Verify the retained 96-pixel physical opening, 96-pixel route language, collision and navigation
  geometry, junction draw order, and nearby label/marker readability against the live scene.
- Produce deterministic 0.8x, 1.0x, and 2.0x views in 1280x720 and 1280x827 frames centered on
  the connector, then offer all six for product-owner review. If the exact module fails, document
  why and stop at a new correction proposal.

This is feasibility evidence only. Do not edit `Main.tscn`, copy another asset into runtime,
change route topology or physical/gameplay authority, or alter accepted Stream, junction, path,
clearing, garden, character, structure, UI, effects, audio, camera, scenario, dialogue, clue,
discovery, treatment, save, or provider work. Visual review alone cannot establish pacing,
usability, or human accessibility acceptance.
