# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. Direction A, painterly storybook gouache, remains selected;
Direction D is only the documented contingency. The deterministic 66-asset corrected proof,
seventeen editable OpenRaster masters, isolated `DirectionAImportProof.tscn`, 1280x720 base design
viewport, accepted Seeker scale, unchanged terrain pixels, and 0.8/1.0/2.0 camera tuning remain as
previously recorded. `Main.tscn` still references none of the proof art.

The external [`art-direction/world-layout-proposal`](art-direction/world-layout-proposal/README.md)
now proposes a 3200x1800 authored opening region before any runtime geometry or bulk-art work. It
retains all eleven location identities, all twelve topology edges, the existing scenario order,
240-pixel-per-second movement, 96-pixel path language, and camera zoom tuning. The non-uniform
anchor reflow uses more of the expanded east side and avoids the unauthorized crossing produced by
a literal 2x transform. It documents exact proposed coordinates, direct route lengths and travel
times, 1280x720 and 1280x827 screen spans, camera/world limits, navigation consequences, and five
empty-area content budgets.

The proposal includes one labelled full-region SVG and exact 1280x720 and 1280x827 camera-frame
SVGs. `layout-proposal.json` is their single source. The build is byte-deterministic, and its
validator checks identities, coordinates, topology, endpoint and landmark clearance, ordinary and
collinear unauthorized crossings, camera containment, content-zone bounds, accessible SVG titles,
and generated-file freshness. `scripts/quality.sh` now runs that validator.

## Verified evidence and boundaries

- Visual review corrected the first literal-scale draft because the shrine-to-road and
  clearing-to-closed-path corridors crossed without a topology node. The retained proposal has no
  non-endpoint route intersections.
- The full-region and both camera-frame diagrams were rendered and inspected. The shared
  `(2000, 1000)` frame centre shows the taller logical viewport's extra vertical area without a
  scale or horizontal-framing change.
- The scoped validator passes 11 anchors, 12 exact edges, 5 bounded content zones, 2 exact camera
  frames, accessible SVG structure, and byte-deterministic output. Focused regression checks also
  prove ordinary, collinear-overlap, and non-intersection handling.
- Host-local `./scripts/quality.sh` passes all art and import validators, the new layout validator,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, formatting, and the Compatibility
  graphical launch. The restricted run passed all non-Godot stages before the known `user://logs`
  sandbox crash; the identical full command passed on the host.
- The proposal is not accepted runtime geometry and is not human pacing, route-usability, or
  accessibility evidence. No `game/` file, runtime node, navigation polygon, collision shape,
  location, clue, dialogue, gameplay rule, or ordinary art reference changed.

## Exact next bounded slice

Obtain an explicit review decision on the proposal's anchor coordinates, north-west relocation of
the terminal Closed Forest Path, route-distance budget, camera framing, and five empty-area content
budgets. If changes are requested, revise only the external proposal and rerun its validator plus
`./scripts/quality.sh`.

Do not implement the layout without explicit approval. After approval, the next implementation
slice is one coordinated greybox-only change covering the accepted 3200x1800 world bounds, camera
limits/tuning, eleven anchor moves, collision and navigation geometry, provenance, focused tests,
and Godot smokes. Preserve all topology, scenario, dialogue, clue, and gameplay rules. Bulk
Direction A production art, audio, release, signing, and independent human playtesting remain later
work.
