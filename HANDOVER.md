# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. Direction A, painterly storybook gouache, remains selected;
Direction D is only the documented contingency. Phase 9.2 has a deterministic 66-asset corrected
pipeline with seventeen editable OpenRaster masters. Exact copies are used only by the isolated
`DirectionAImportProof.tscn`; `Main.tscn` and ordinary gameplay still reference none of this art.

The display and character proof now use a 1280x720 base design viewport with aspect expansion. A
16-inch 3456x2234 fullscreen display corresponds to a representative 1280x827 logical view. Every
Seeker walk and idle/breathe frame is at least 144 logical pixels visibly tall on a 168-pixel export
canvas with a fixed 156-pixel planted-foot baseline. Interaction frames use the same canvas and
baseline. Exact 2x masters were rebuilt from the retained high-resolution source sheets; the prior
48–56-pixel exports were not enlarged and no new character design was generated. The user reviewed
the representative full-screen scale and accepted it as “that’s better.”

The existing 96-pixel path, 80–96-pixel props, terrain pixels, animation frame counts, and animation
timings remain unchanged. They stay legible and proportionate in the enlarged proof, so no prior
terrain or object artwork needs to be redone. The terrain colour-vision evidence was regenerated only
to bind its report to the new manifest hash; its source pixels and descriptive measurements did not
change.

The authored gameplay world is still 1600x900. The camera minimum zoom is now 0.8 because a 1280x720
viewport must not reveal beyond those bounds. This means the current world is only 1.25 base screens
wide and tall; enlarging the authored world is a separate layout/content decision, not part of the
scale rebaseline.

## Verified evidence and boundaries

- `art-direction/production-proof/corrected/tools/build.py` is byte-deterministic: rebuilding the
  complete proof left the SHA-256 of the full Git binary diff unchanged.
- The corrected-proof validator passes all 66 assets, exact hashes, alpha/seam/geometry gates,
  144-pixel walk and idle visibility, 156-pixel baselines, animation uniqueness and timing,
  navigation clearance, grayscale cues, and all seventeen editable masters.
- The terrain accessibility generator and independent validator pass all 40 fixed original,
  grayscale, protanopia, deuteranopia, and tritanopia views. These are descriptive simulations and
  measurements, not human accessibility acceptance.
- Import validation passes exact parity for all 66 Godot proof copies. The isolated proof smoke
  verifies twelve animations and 48 unique character textures with no gameplay authority.
- Host-local `./scripts/quality.sh` passes both art validators, all import hashes, zero-warning build,
  2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests, scenario validation,
  every credential-free Godot smoke, formatting, and the Compatibility graphical launch. The
  restricted run reached the known `user://logs` crash only after every non-Godot stage passed; the
  identical full command passed on the host.
- Do not connect proof art to `Main.tscn`, alter gameplay authority, or claim independent human
  accessibility acceptance without a separately reviewed slice.

## Exact next bounded slice

Before changing runtime geometry or producing bulk art, create one external 3200x1800 authored-world
layout proposal for user review. Preserve the current eleven location anchors, route topology,
scenario order, and all gameplay rules. Document proposed anchor coordinates, route lengths,
screen-to-screen travel distances at the existing 240-pixel-per-second movement speed, camera/world
limits, navigation consequences, and which empty areas would require new terrain or content. Include
one labelled full-region diagram and representative 1280x720 and 1280x827 camera frames. Keep the
proposal outside `game/`; do not move runtime nodes, change navigation polygons, add locations,
dialogue, clues, art, or gameplay in this planning slice.

After explicit layout approval, implement the accepted world bounds, camera tuning, anchor moves,
navigation geometry, provenance sources, tests, and smokes as one coordinated greybox-only slice.
Bulk Direction A production art remains later work.
