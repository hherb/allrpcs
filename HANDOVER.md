# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The explicitly approved 3200x1800 opening-region proposal is
now the live greybox layout. Runtime provenance retains proposal SHA-256
`a5ebd5e23b5f41a8958477c953ba2763afffcb636f3bb6f69a969c0c1d57f8f1`; all eleven exact anchors,
twelve topology edges, 96-pixel route corridors, 24-pixel navigation clearance, camera bounds and
0.8/1.0/2.0 tuning are implemented together. The Closed Forest Path is the retained north-west
terminal branch and remains physically gated until its existing Forester access removes the gate.

Direction A remains selected and Direction D remains only the contingency. The 66 proof exports,
seventeen editable OpenRaster masters, and isolated `DirectionAImportProof.tscn` remain proof-only;
`Main.tscn` references none of that production-proof art. The proposal's five empty-area budgets
remain non-authoritative.

## Completed slice

- Reflowed the world, camera, party, interactables, eleven location anchors, and all existing
  landmark greyboxes without changing location identity, topology, scenario, dialogue, clue, or
  gameplay rules.
- Replaced the old broad navigation surface with the approved twelve-corridor raster, exact
  endpoint clearance, visible route presentation, and collision-backed outer boundaries.
- Derived village, stream/laundry, clearing, shrine, sickhouse, road/wagon, and closed-path openings
  from the approved diagonal routes. Cumulative review added a physical traversal check for every
  open topology edge and retained the intentional closed-gate exception.
- Corrected the sickhouse entrance, village four-way shell, shrine-road opening, southern-road hub,
  and wagon opening after the stronger physical-route review exposed their old-map assumptions.

## Verified evidence and limits

- `python3 art-direction/world-layout-proposal/tools/validate.py` passes the exact 11 anchors,
  12 edges, route-band separation, 5 bounded content zones, 2 camera frames, SVG structure,
  geometry regressions, and deterministic rebuilding.
- Host-local `./scripts/quality.sh` passes the zero-warning build, 2,839 Domain, 925 Application,
  1,261 Infrastructure, and 8 architecture tests, every credential-free Godot smoke, formatting,
  and the Compatibility graphical launch.
- Automated graphical launch and smoke coverage are not human pacing, route-usability, or
  accessibility acceptance. No independent human playtest evidence was supplied in this slice.

## Exact next bounded slice

Run one explicit product-owner graphical greybox checkpoint over the approved layout: traverse all
eleven anchors and every open connection, confirm the north-west closed gate reads correctly, assess
the 6.5-second shrine-road pacing hypothesis, and inspect camera framing at 0.8/1.0/2.0 zoom in both
representative viewport shapes. Record only observations the human reviewer actually supplies.

If corrections are requested, change only the accepted greybox geometry and its focused tests, then
rerun the proposal validator and `./scripts/quality.sh`. Do not wire proof art into `Main.tscn`, add
locations or topology, change scenario/dialogue/clue/gameplay authority, fill the five content
budgets, or begin bulk art, audio, release, signing, or distribution work in that slice.
