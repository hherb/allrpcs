# Village Square cross-junction feasibility

This is a proof-only check of the approved Village Square anchor, its four retained route
connections, and the existing manifest-bound `terrain.path_cross_junction` export. Reproduce it
from the repository root with:

```sh
python3 art-direction/production-proof/corrected/tools/check_village_square_cross_junction_feasibility.py
```

The live and approved coordinates agree: Village Square `(2200, 900)`, Sickhouse `(2640, 1450)`,
Healer's Garden `(2300, 1300)`, Southern Road `(1500, 1400)`, and Village Edge `(1700, 900)`.
Their outward headings from Village Square are respectively `51.340192°`, `75.963757°`,
`144.462322°`, and `180°`.

The exact 256×256 cross-junction has four 96-pixel ports separated by 90°. The validator checks
its manifest digest, dimensions, pivot, source/import isolation, the live 96-pixel route width, and
exact parity between the runtime layout and approved proposal before assessing placement.

Rigid rotation preserves the four 90° port intervals. The minimax placement rotates the export by
`-19.329904°`, yielding port headings `-19.329904°`, `70.670096°`, `160.670096°`, and
`250.670096°`. Their residual mismatches against the four live routes are `70.670096°`,
`5.293661°`, `-16.207774°`, and `-70.670096°`. The smallest possible worst mismatch is therefore
`70.670096°`, or about `120.784` pixels of lateral displacement 128 pixels from the pivot.

The existing cross-junction is infeasible at Village Square under exact, unscaled, unsheared
placement. Moving it can shift the seams but cannot change the incompatible port intervals.

Six deterministic external review views show the best rigid placement at 0.8×, 1.0×, and 2.0× in
1280×720 wide and 1280×827 tall frames. Gold bands are the retained live routes, brown lines are
their centrelines, and cyan lines are the four rotated asset ports. These are planning views, not
runtime captures or acceptance evidence. The export remains isolated to
`DirectionAImportProof.tscn`; `Main.tscn`, collision, navigation, topology, camera behavior, and
gameplay authority are unchanged.

A tailored Village Square junction asset would require a separate proof proposal and explicit
product-owner approval.
