# Clearing opening and southbound shoulder comparison

Complete external-only comparison from published `main`
`a6903284c2dc689364e7e186975de86ff5752bfd`, 2026-10-06 (Australia/Sydney).
The owner instructed us to carry the approved audit forward and move to the next
slice; [continuation](continuation.json) records that instruction. No duplicate
coverage audit or repeat approval request is retained.

The four columns are retained baseline, hard support, feather-16 and feather-24:

- [Complete wide series](review/gpu-series-wide.png)
- [Complete tall series](review/gpu-series-tall.png)
- [Exact native detail comparisons](review/complete-details-1x.png)
- [Exact camera-2x detail comparisons](review/complete-details-2x.png)

All 24 captures instantiate the unchanged ordinary scene in disposable viewports.
Only root HUD layers and characters are hidden. Shells, labels, focus indication,
routes, physical owners, accepted ground and all channel layers remain. The 0.8x
tall camera center is constrained by the ordinary top limit; boards show actual
centers, and crops use recorded transforms without resampling.

The candidate envelope is 304x164 at `(1008,414)`, z=-78, above the retained
Clearing transition (-79) and below its route (-75). Select the native route alpha
within world `[1056,1240) × [450,490)`, dilate by a square radius 24, and subtract
the union of all five channel alpha planes expanded by four native pixels.
This removes 3,786 initially supported texels, including bridge decking; the
remaining hard support contains 14,777 texels. Both smoothstep feathers are
strictly inward with explicit zero padding. The full envelope contains every
support edge. Exact unchanged 256px leaf-litter paint uses world phase `(0,0)`.

Three editable 608x328 OpenRaster masters keep the masked paint, independent
coverage reference, unmasked world paint and blank Corrections layer. Clean
transparent texels have zero RGB. These are external candidates: production
inventory stays 97 assets / 48 masters, with no scene/import/manifest edits.

**Agent recommendation: retain the existing baseline.** The added hard support
brings leaf litter toward the bridge and produces a harder boundary; feathers
soften it, but the existing moss buffer remains clearer in these sampled views.
This judgment is not a new owner selection or a terrain-completeness claim.
All alternatives remain intact; no runtime promotion is proposed.

## Colour-vision and luminance slice

- [Complete native diagnostic series](readability/complete-series-1x.png)
- [Complete camera-2x diagnostic series](readability/complete-series-2x.png)

Forty images show all four variants under normal colour, protan, deutan and
tritan simulations, plus linear-light luminance. The severity-100 coefficient
tables are the published Machado/Oliveira/Fernandes data in
[colorspacious's pinned implementation](https://raw.githubusercontent.com/njsmith/colorspacious/58948923b706879a54071568c7501be3f108797c/colorspacious/cvd.py).
[Model provenance](simulation-model.json) records the source SHA-256, revision,
coefficients and method: decode sRGB, apply the forward matrix in linear RGB,
clip to display gamut and encode sRGB. Labels stay outside transformed pixels.
The native route stays visually distinct in these sampled simulations. These
static model-based observations do not establish human accessibility or complete
contrast testing; focus, characters, functional UI and other locations remain open.

## Reproduce and verify

```sh
python3 art-direction/clearing-village-edge-ground-feasibility/build.py --check
python3 art-direction/clearing-village-edge-ground-feasibility/boards.py --check
python3 art-direction/clearing-village-edge-ground-feasibility/readability.py --check
python3 art-direction/clearing-village-edge-ground-feasibility/verify.py
```

`--check` writes nothing. Authoring refuses existing outputs before any write;
the capture harness refuses existing evidence and never saves or packs Main.
The sandbox renderer exited 134 before capture; the same capture command on the
host exited 0 with all 24 frame markers and the explicit pass marker.

Independent verification checks committed Git blobs and SHA-256 for all 8,101
predecessor files outside README/ROADMAP/HANDOVER, exact phase/RGB, separate 2x
layers, channel exclusion, recorded runtime owners and bounded GPU differences.
Eight [rejection/numerical probes](review-probes.json) cover bad support, mutated
paint, viewport/transform changes, missing candidates and colour-transform vectors.
The accepted Village Edge implementation and passing zero-warning/5,033-test
quality evidence remain byte-exact and are verified; no fresh product suite or
hosted validation is claimed. [Session review](session-review.json) and
[verification](verification.json) record the consolidated results.

Next: complete external Woodland Clearing dressing compositions with the already
approved environment artwork, preserving ground, route/interaction clearance,
physical ownership and all hidden/discovery state. No runtime placement, scenario,
physics/navigation, gameplay, functional UI, audio, release or signing change.
