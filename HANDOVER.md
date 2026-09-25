# Allrpcs Handover

## Current state

`main` includes merged PR #374 at `dbef4b5`. Branch `codex/phase-8-enterable-sickhouse` completes two
bounded Phase 8 landmark slices: an enterable sickhouse and a reachable healer-garden preparation
station.

The sickhouse retains the exact projection anchor and first-patient binding. Five collision-backed wall
sections leave one 64-pixel north entrance. The navigation surface reserves 24-pixel wall clearance,
connects the entrance corridor to the interior, and retains all twelve earlier topology routes. The exact
sickhouse anchor is the patient approach; the Seeker can enter, reach it, and leave. A pure route policy
rejects wall-crossing and out-of-bounds paths.

The healer's garden retains its exact projection anchor, the structurally valid first-patient context,
and that context's exact remedy recipe. Its greybox garden beds and reachable preparation station do not
start or advance preparation. Disease, treatment, dialogue, focus, discovery, and ordinary provider or
model startup remain unchanged.

## Validation and review

- Test-first focused Application builds failed at the absent sickhouse layout, route policy, and garden
  preparation-scene binding types.
- Seven focused host-local Application tests pass for exact provenance, bounded geometry, entrance
  clearance, wall and world-bound route rejection, exact garden recipe, and station placement.
- Focused host-local Godot smokes pass all twelve opening-region routes, the exact patient binding, the
  collision-backed Seeker enter/approach/leave route, and the reachable inert garden station.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 600 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the Compatibility launch.
- Complete-diff review confirmed whitespace and 120-character cleanliness, kept every changed C# file
  below 500 lines, and restored exact shared polygon seams after the initial sickhouse mesh split had
  disconnected earlier bridge and east-bank routes.
- No preparation interaction or progression, treatment, dialogue, focus, discovery, other landmark,
  production art, weather, lighting, sound, story variation, journal, map, provider, persistence,
  production UI, audio, signing, release, or distribution work was added.

## Exact next bounded slice

Build the village-square and village-edge greybox transitions without adding their later investigations.

Acceptance boundary:

1. Bind only the exact retained `village-square` and `village-edge` projection anchors to distinct,
   bounded scene areas; rebuilt, foreign, missing, overlapping, or out-of-bounds bindings fail closed.
2. Add collision-aware greybox boundaries and explicit transition openings while retaining the existing
   24-pixel endpoint clearance and every authored opening-region route.
3. Prove the Seeker can travel garden -> village square -> village edge and return over synchronized
   Godot navigation without crossing a boundary or leaving the 1600-by-900 region.
4. Stop before the stream, laundry, woodland, shrine, road, wagon, closed path, clue placement,
   discovery, dialogue, inhabitants, production art, weather, lighting, sound, journal, or map.
5. Add no treatment or preparation progression and no Phase 7 cadence, provider, persistence,
   production UI, audio, signing, release, or distribution work.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
