# Allrpcs Handover

## Current state

`main` includes merged PR #375 at `a9409ca`. Branch `codex/phase-8-village-transitions`
completes the next Phase 8 landmark slice: collision-aware village-square and village-edge
transitions, plus the display-scaling correction identified during review.

`OpeningRegionVillageTransitionSceneBinding` accepts only the projection's exact retained
`village-square` and `village-edge` anchors in distinct bounded areas with 24-pixel endpoint
clearance. The layout retains explicit garden, square, edge, stream-branch, and woodland-branch
openings. Thirteen scene-authored collision sections and the synchronized navigation surface admit
the exact garden -> square -> edge round trip and its return while retaining all twelve existing
opening-region routes. Rebuilt, foreign, missing, swapped, overlapping, out-of-bounds, narrow, and
oversized-wall inputs fail closed.

The new runtime geometry uses named constants in `OpeningRegionVillageTransitionGeometry`; raw
coordinates in `Main.tscn` remain authored Godot scene data and are exercised against the binding,
collision, and navigation contracts. The 1600-by-900 value remains world space, not screen size.
The window now treats 960-by-540 as its minimum physical size and design viewport, remains resizable,
and uses Godot `canvas_items` scaling with `expand` aspect handling so larger, differently shaped,
and fullscreen windows do not change world coordinates or stretch one axis independently.

## Validation and review

- Test-first focused Application compilation initially failed at the absent village binding and layout
  types; eight focused village-transition tests now pass.
- The switch-gated village smoke passes exact binding, thirteen collision bodies, synchronized
  navigation, physical garden/square/edge travel, wall blocking, and the return route.
- Review found and fixed three integration defects: a square boundary initially occupied the established
  Seeker origin, a navigation opening admitted collision-unsafe corner paths for the healer, and an
  intermediate square placement crossed the established bridge. Exact sickhouse-return regression
  coverage, healer follow, both bridge directions, and all twelve topology routes now pass.
- Review also replaced parallel navigation numbers with the named shared geometry source and added
  fail-closed oversized-wall validation plus collision-shape verification.
- The adaptive-display smoke passes the exact 960-by-540 minimum, resizable window, canvas-item scaling,
  and expanded aspect configuration. This is configuration evidence, not complete manual high-DPI,
  multi-monitor, or every-platform fullscreen QA; that later roadmap item remains open.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 608 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- Draft PR #376 targets `main`. Its Credential-free CI run ended with all five jobs failed before any
  step was instantiated (`steps: []`), so those hosted results are Actions/account infrastructure and
  provide no product-test evidence; the host-local validation above remains the executed evidence.
- Complete-diff review confirmed whitespace and 120-character cleanliness and kept every changed C#
  file below 500 lines.
- No stream, laundry, woodland, clue placement, investigation, discovery, dialogue, inhabitants,
  preparation or treatment progression, production art, weather, lighting, sound, journal, map,
  provider, persistence, production UI, audio, signing, release, or distribution work was added.

## Exact next bounded slice

Build the stream and abandoned-laundry greybox without adding their later investigation content.

Acceptance boundary:

1. Bind only the exact retained `stream` and `abandoned-laundry` projection anchors to distinct,
   bounded world-space scene areas; rebuilt, foreign, missing, overlapping, out-of-bounds, or
   insufficient-clearance bindings fail closed.
2. Keep new geometry in one named source of truth; Godot scene coordinates are authored world data,
   never viewport or device dimensions. Preserve the resizable 960-by-540 minimum/design viewport,
   `canvas_items` scaling, `expand` aspect handling, and fullscreen capability.
3. Add collision-aware stream-bank and inert laundry greybox geometry with explicit route openings.
   Preserve the existing bridge, village transitions, 24-pixel endpoint clearance, default actor
   origins, healer movement, and every authored opening-region route.
4. Prove synchronized Seeker travel village edge -> stream -> abandoned laundry and return without
   crossing a bank, building boundary, or the 1600-by-900 world bound.
5. Stop before laundry clues or interactions, woodland, rare specimen, shrine, road, wagon, closed
   path, discovery, dialogue, inhabitants, production art, weather, lighting, sound, journal, or map.
6. Add no preparation or treatment progression and no Phase 7 cadence, provider, persistence,
   production UI, audio, signing, release, or distribution work.
7. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
