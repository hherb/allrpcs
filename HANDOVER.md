# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete on `main`. Direction A, painterly storybook gouache, remains
selected; Direction D is only the documented contingency. Phase 9.2 has a deterministic 66-asset
corrected pipeline with seventeen editable OpenRaster masters. Its terrain proof covers quiet moss,
woodland leaf litter, all five transitions between them, and the complete straight/bend/T/cross/end
packed-earth set. Exact copies exist only in the isolated Godot import proof; ordinary gameplay
references none of this art.

The existing terrain family now also has a deterministic external colour-vision and contrast review.
It changes no manifest asset, master, export, import copy, or runtime reference.

## Evidence and boundaries to retain

- `review/terrain-accessibility` contains eight side-by-side sheets and 40 individual views in fixed
  original, grayscale, protanopia, deuteranopia, and tritanopia order. The inputs are exact current
  manifest exports for quiet moss, leaf litter, packed earth, and all five ground transitions.
- `tools/terrain_accessibility_review.py` uses the documented Machado–Oliveira–Fernandes 2009
  full-severity matrices in linear sRGB, preserves alpha, and records its exact matrices and
  parameters in `report.json`. Packed earth is the exact straight-path export alpha-composited over
  exact quiet moss; only visible path pixels contribute to its measurements.
- The report binds the manifest, every input, every view, and every sheet to SHA-256 digests. It
  records mean relative luminance, luminance standard deviation, 10th-to-90th-percentile span,
  neighbouring-pixel RMS luminance delta, and pairwise material differences.
- `validate_terrain_accessibility_review.py` independently reloads manifest exports, verifies source
  hashes, recomputes every transformed RGBA pixel and metric, checks the exact PNG set and sheet
  order, and compares the complete report. `scripts/quality.sh` runs it before import validation.
- Host-local `./scripts/quality.sh` passes both art validators, all 66 exact import hashes,
  zero-warning build, 2,839 Domain, 924 Application, 1,261 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, formatting, and the Compatibility
  graphical launch. The restricted run reached the known `user://logs` crash only after all
  non-Godot stages passed; the identical full command was retried on the host.
- The simulations and numeric measurements are descriptive exact-pixel evidence, not thresholds,
  a pass/fail result, or human accessibility acceptance. No independent human art or accessibility
  judgement was supplied.

## Exact next bounded slice

Extend the same documented external-only simulation method to the existing preparation-table states
and Seeker samples. Render ordinary, noticeable, identified, and dangerous table states plus all
current Seeker walk, idle/breathe, and reach-and-inspect samples in the same five fixed views. Bind
every composed sample to its exact current manifest inputs, retain shape/pattern and silhouette
evidence separately from hue, and record descriptive foreground/background and state-difference
measurements without introducing accessibility thresholds or claiming human acceptance. Keep all
review files outside `game/`; do not change asset IDs, masters, exports, import copies, animation
timing, runtime scenes, gameplay, or UI behavior.

Do not add new terrain, objects, characters, animation frames, UI assets, runtime selection,
collision/navigation authority, audio, release, or signing work. After that review passes, the next
slice may define a bounded human accessibility review protocol for these exact external sheets; it
must not claim complete-region acceptance or modify production assets in response without a separate
reviewed slice.
