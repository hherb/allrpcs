# Allrpcs Handover

## Current state

`main` includes merged PR #377 at `a42f8ff`. Branch
`codex/phase-8-woodland-clearing` completes two bounded Phase 8 scene slices: the
woodland-clearing greybox with a concealed rare-specimen marker, followed by the old-shrine
greybox with a concealed two-layer inscription marker.

`OpeningRegionWoodlandClearingSceneBinding` accepts only the projection's exact retained
`woodland-clearing` anchor in one bounded area with 24-pixel endpoint clearance.
`OpeningRegionWoodlandClearingLayout` retains four explicit topology-facing openings without
overlapping the existing stream bank. Eight collision-backed tree-boundary sections preserve
routes toward the laundry, village edge, shrine, and closed deeper-forest path. The rare-specimen
marker is scene-authored, hidden, childless, and has no interaction, world-object, discovery,
collection, identification, or evidence authority.

`OpeningRegionOldShrineSceneBinding` accepts only the exact retained `old-shrine` anchor.
`OpeningRegionOldShrineLayout` retains explicit clearing and road openings behind five
collision-backed boundary sections. Its hidden scene-authored inscription has exactly two inert
marker layers and no script, collision, interaction, visibility-state, or evidence authority.

Both scenes keep the 1600-by-900 world, existing resizable 960-by-540 minimum/design viewport,
`canvas_items` scaling, `expand` aspect handling, fullscreen capability, bridge, stream/laundry
round trip, village transitions, actor origins, healer movement, and all twelve topology routes.

## Validation and review

- Ten focused Application tests pass for exact-anchor binding, bounded geometry, opening
  alignment, dimension rejection, wall-crossing rejection, and world-bound rejection.
- The woodland switch-gated smoke passes exact projection provenance, eight collision-backed
  tree sections, one hidden inert specimen marker, synchronized navigation from village edge and
  laundry into the clearing and back, physical opening travel, and representative wall blocking.
- The shrine switch-gated smoke passes exact provenance, five collision-backed shrine sections,
  one hidden two-layer inscription marker, synchronized and physical clearing/shrine travel, and
  representative wall blocking.
- Review corrected the first clearing footprint because its southern edge overlapped the west
  stream bank. The final footprint ends above that bank while preserving all four openings and
  the exact 24-pixel clearance boundary.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 625 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- Draft PR #378 targets `main` at `5002f02`. Credential-free CI run `36206844930` ended with all
  five jobs failed before any step was instantiated (`steps: []`), so those hosted results are
  Actions/account infrastructure and provide no product-test evidence; the host-local validation
  above remains the executed evidence.
- Every new C# file remains below 500 lines; 120-character and whitespace checks are clean.
- No rare-specimen discovery or interaction, inscription reading or discovery, authoritative
  evidence mutation, road/wagon/closed-path content, dialogue, inhabitants, preparation or
  treatment progression, production art, weather, lighting, sound, journal, map, provider,
  persistence, production UI, audio, signing, release, or distribution work was added.

## Exact next bounded slice

Bind the scene-authored layered inscription to the existing representative `shrine-inscription`
world object and its initial Seeker visibility without yet resolving or reinterpreting it.

Acceptance boundary:

1. Retain the exact `shrine-inscription` object and its exact initial `Noticed` visibility from one
   caller-owned representative discovery scenario; rebuilt, foreign, missing, duplicate, wrong-id,
   wrong-character, or non-`Noticed` inputs fail closed.
2. Bind only that exact object/visibility pair to the existing exact old-shrine binding and exact
   scene-authored `LayeredInscription` node; do not create a second marker or replace scene geometry.
3. Keep ordinary startup presentation-free and interaction-free. Exercise the binding only through
   one switch-gated smoke that proves exact provenance and unchanged scenario, scene, perception,
   world-object, and marker instances.
4. Stop before focused-search input, passive noticing, observation reinterpretation, knowledge
   acquisition, visibility transition, evidence exposure, collection, presentation, or persistence.
5. Add no road, wagon, closed-path content, dialogue, inhabitants, preparation or treatment
   progression, production art, weather, lighting, sound, journal, map, provider, production UI,
   audio, signing, release, or distribution work.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
