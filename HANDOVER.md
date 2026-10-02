# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 76 manifest-bound
exports and 27 editable OpenRaster masters. Square–Garden, Square–Sickhouse, and
Square–Southern Road runtime views are owner-accepted.

The owner visually approved the Southern Road–Wagon correction on 2026-10-02 except for
an overshoot "to the left (east) side". A new external-only revision shortens the incoming
junction tip from 48 to 16 pixels, interpreting "left" as the tip below the upstream brown
road endpoint. That screen-left/east ambiguity was raised; confirmation is pending.
See the [trim and six views](art-direction/production-proof/corrected/review/southern-road-wagon-path-trim.md).
Failed-strip and original correction evidence/recipes remain intact. No production/runtime,
manifest/import, or physical/gameplay file changed; no runtime promotion is authorized.

The trim validator is in regular quality. Use Pillow-enabled `/opt/homebrew/bin/python3`;
`--check-live-baseline` freezes runtime/manifest/geometry inputs. Focused validation proves
2,789 changed pixels confined to the incoming tip, no increased alpha, and unchanged prior
evidence/baseline. Repeated captures reproduced all six PNG hashes; code and visual review
found no unresolved implementation issue. Full product validation passed in the preceding
slice; this external-only trim used focused validation and actual Compatibility captures.

## Exact next bounded slice

Confirm the intended overshoot side and obtain owner acceptance of the trimmed junction tip.
The rest of the preceding correction is visually accepted. If a different side was intended,
make only the specifically requested external correction and preserve archived evidence.
After acceptance, propose a separately approved named production/runtime promotion with exact
asset, master, imports, and presentation-substitution scope. No promotion is authorized.

Preserve physical/gameplay authority, collision, navigation, camera, scenario, dialogue, clues,
discovery, treatment, save, provider, UI, audio, release, and signing. Do not infer pacing,
usability, or accessibility acceptance. Direct-main commit/push covers reviewed work only.
