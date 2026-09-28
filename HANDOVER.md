# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. Direction A, painterly storybook gouache, remains selected;
Direction D is only the documented contingency. The deterministic 66-asset corrected proof,
seventeen editable OpenRaster masters, isolated `DirectionAImportProof.tscn`, 1280x720 base design
viewport, accepted Seeker scale, unchanged terrain pixels, and 0.8/1.0/2.0 camera tuning remain as
previously recorded. `Main.tscn` still references none of the proof art.

The external [`art-direction/world-layout-proposal`](art-direction/world-layout-proposal/README.md)
is the current candidate for the opening-region reflow. It proposes a 3200x1800 authored world,
all eleven retained locations, all twelve existing topology edges, the unchanged 240-pixel-per-second
movement speed and 96-pixel path language, exact route budgets, 1280x720 and 1280x827 camera frames,
and five non-authoritative empty-area content budgets. `layout-proposal.json` remains the single
source for the three deterministic SVG review diagrams.

## This session

- Technical review recommends the proposal as the candidate for one coordinated greybox
  implementation. The north-west Closed Forest Path removes the prior false crossing; the
  6.5-second shrine-to-road connection remains an explicit pacing hypothesis; both representative
  camera frames are contained at the retained zoom range; and the five content budgets add no
  gameplay authority.
- The proposal validator now checks complete 96-pixel route-band separation, not only centreline
  intersection. The closest independent centrelines are about 290.6 pixels apart.
- Focused geometry regressions cover ordinary crossing, collinear overlap, a non-intersecting
  95-pixel route-band overlap, and separated 97-pixel parallel routes through the same predicate
  used by proposal validation.
- No `game/` file, runtime node, navigation polygon, collision shape, location, clue, dialogue,
  gameplay rule, or ordinary art reference changed.

## Verified evidence and limits

- `python3 art-direction/world-layout-proposal/tools/validate.py` passes 11 anchors, 12 exact edges,
  96-pixel independent-route separation, 5 bounded content zones, 2 exact camera frames,
  accessible SVG structure, deterministic geometry regressions, and byte-deterministic output.
- Host-local `./scripts/quality.sh` passes all art/import/layout validators, zero-warning build,
  2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, formatting, and the Compatibility graphical
  launch. The restricted run passed every non-Godot stage before the known `user://logs` sandbox
  crash; the identical full command passed on the host.
- The technical recommendation is not product-owner approval, human pacing or route-usability
  evidence, human accessibility acceptance, or accepted runtime geometry.

## Exact next bounded slice

Obtain an explicit product-owner decision on the proposed anchor coordinates, north-west Closed
Forest Path relocation, route-distance budget, camera framing, and five empty-area content budgets.
If changes are requested, revise only the external proposal and rerun its validator plus
`./scripts/quality.sh`.

Do not implement the layout from a generic continuation request: the explicit approval gate remains
closed. After approval, make one coordinated greybox-only change covering the accepted 3200x1800
world bounds, camera limits/tuning, eleven anchor moves, collision and navigation geometry,
provenance, focused tests, and Godot smokes. Preserve all topology, scenario, dialogue, clue, and
gameplay rules. Bulk Direction A production art, audio, release, signing, and independent human
playtesting remain later work.
