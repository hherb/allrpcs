# Stream–Laundry–Woodland junction feasibility

This is a proof-only check of the approved anchors and existing manifest exports. Reproduce it from
the repository root with:

```sh
python3 art-direction/production-proof/corrected/tools/check_opening_junction_feasibility.py
python3 art-direction/production-proof/corrected/tools/validate.py
```

The live and approved anchor coordinates agree: Stream `(1800, 500)`, Abandoned Laundry
`(1500, 220)`, and Woodland Clearing `(1000, 350)`. The Stream-to-Laundry heading is
`-136.974934°`, and the Laundry-to-Clearing heading is `165.425784°`. Their shortest signed turn
is `-57.599282°`. The exact 256×256 path bend has two 96-pixel ports separated by 90°. Its
manifest-bound export is validated independently for byte identity, alpha, and connector parity.

Rotation preserves the angle between the bend's ports. Aligning either port with one route leaves
the other `32.400718°` away from the second route. At 128 pixels from an aligned port, that angular
difference represents `68.587` pixels of lateral displacement from the target heading. Moving the
bend may shift the seam but cannot remove the angular mismatch; scaling or shearing it would change
its exact 96-pixel connectors and texture.

The existing straight module likewise preserves one heading and cannot resolve the turn.

The existing 90° bend is therefore infeasible at this junction under exact, unscaled placement.
The product owner subsequently approved one junction-specific isolated proof. The deterministic
`terrain.opening_junction` export is a 512×512 canvas with a 1024×1024 editable OpenRaster master.
Its canonical incoming port follows `90°`; its outgoing port follows `32.400718°`. Rotating the
module by `133.025066°` aligns those ports to the live `-136.974934°` incoming and `165.425784°`
outgoing headings without scaling or shearing.

The manifest records a 336×458 visible footprint inside that canvas. The corrected proof validator
checks the exact 96×32 incoming connector pixels, reconstructs and compares the transformed
outgoing connector pixels, rejects any visible pixel beyond the unchanged 96-pixel physical-route
bands, and binds the side-by-side seam/containment review. The asset is imported only by
`DirectionAImportProof.tscn`; `Main.tscn` and ordinary gameplay remain unchanged. Runtime use still
requires separate product-owner approval.
