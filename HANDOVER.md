# Allrpcs Handover

## Current state

`main` includes merged PR #376 at `26cca8f`. Branch `codex/phase-8-stream-laundry`
completes the next Phase 8 landmark slice: collision-aware stream banks and an inert
abandoned-laundry greybox with explicit east/west route openings.

`OpeningRegionStreamLaundrySceneBinding` accepts only the projection's exact retained `stream`
and `abandoned-laundry` anchors in distinct bounded areas with 24-pixel endpoint clearance.
`OpeningRegionStreamLaundryLayout` retains the exact village-edge connection endpoint, an aligned
north bridge approach, copied non-degenerate bank polygons, and two 64-pixel laundry openings.
Missing, rebuilt, foreign, swapped, overlapping, out-of-bounds, insufficient-clearance,
narrow-opening, oversized-wall, invalid-approach, malformed-bank, and bank-crossing inputs fail
closed.

`OpeningRegionStreamLaundryGeometry` is the named source for the new world-space areas, dimensions,
approaches, and bank polygons. Authored `Main.tscn` coordinates are exercised against that source.
Six collision-backed laundry boundary sections retain the stream and later woodland routes. The
existing two collision-backed stream banks and bridge remain aligned with synchronized navigation.
The 1600-by-900 values remain world space; the existing resizable 960-by-540 minimum/design viewport,
`canvas_items` scaling, `expand` aspect handling, and fullscreen capability are unchanged.

## Validation and review

- Test-first focused Application compilation initially failed at the absent stream/laundry types;
  seven focused binding, layout, bank, and route-policy tests now pass.
- The switch-gated stream/laundry smoke passes exact projection provenance, named bank geometry,
  two collision-backed banks, six laundry boundary sections, synchronized village-edge -> stream ->
  laundry travel and return, physical openings, and representative bank/building blocking.
- Navigation, opening-region projection, and village-transition regression smokes pass, retaining the
  bridge, all eleven endpoint-clearance probes, all twelve topology routes, and the prior village
  transitions.
- Review found and fixed a physical-route defect in the first smoke draft: its direct diagonal left
  the village edge through a wall. The proof now uses the exact north opening, full endpoint clearance,
  and a south-bridge approach. Review also added fail-closed degenerate-bank rejection.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 615 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- Draft PR #377 targets `main`. Credential-free CI run `36204509901` ended with all five jobs failed
  before any step was instantiated (`steps: []`), so those hosted results are Actions/account
  infrastructure and provide no product-test evidence; the host-local validation above remains the
  executed evidence.
- Complete-diff review keeps every changed C# file below 500 lines and preserves 120-character and
  whitespace cleanliness.
- No laundry interaction or clue, woodland, rare specimen, shrine, road, wagon, closed-path,
  discovery, dialogue, inhabitant, preparation or treatment progression, production art, weather,
  lighting, sound, journal, map, provider, persistence, production UI, audio, signing, release, or
  distribution work was added.

## Exact next bounded slice

Build the woodland-clearing greybox and concealed rare-specimen placement without adding discovery
or investigation progression.

Acceptance boundary:

1. Bind only the exact retained `woodland-clearing` projection anchor to one bounded world-space
   scene area with 24-pixel endpoint clearance; rebuilt, foreign, missing, out-of-bounds, or
   insufficient-clearance bindings fail closed.
2. Keep new geometry in one named world-space source of truth. Preserve the resizable 960-by-540
   minimum/design viewport, `canvas_items` scaling, `expand` aspect handling, and fullscreen capability.
3. Add collision-aware inert clearing geometry with explicit openings toward village edge, abandoned
   laundry, old shrine, and the closed deeper-forest path. Preserve the stream/laundry round trip,
   bridge, village transitions, default actor origins, healer movement, and all twelve topology routes.
4. Place one scene-authored concealed rare-specimen marker inside the clearing without making it
   visible, interactable, discoverable, collectible, identified, or authoritative evidence.
5. Prove synchronized Seeker travel from both village edge and abandoned laundry into the clearing
   and back without crossing a tree boundary or the 1600-by-900 world bound.
6. Stop before rare-specimen discovery or interaction, shrine inscription, road, wagon, closed-path
   content, dialogue, inhabitants, production art, weather, lighting, sound, journal, or map.
7. Add no preparation or treatment progression and no provider, persistence, production UI, audio,
   signing, release, or distribution work.
8. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
