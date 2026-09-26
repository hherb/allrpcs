# Allrpcs Handover

## Current state

`main` includes merged PR #380 at `016a974`. Branch
`codex/phase-8-southern-road-wagon` completes Phase 8's southern-road and abandoned-wagon
greybox shell while keeping wagon discovery inactive.

`OpeningRegionSouthernRoadWagonSceneBinding` retains the exact existing old-shrine layout and
accepts only the opening projection's exact `southern-road` and `abandoned-wagon` anchors, their
exact shrine/road and road/wagon topology connections, and one non-overlapping in-world wagon
area with endpoint clearance. Rebuilt, foreign, swapped, malformed, overlapping, out-of-bounds,
and insufficient-clearance inputs fail closed.

`OpeningRegionSouthernRoadWagonLayout` defines a 24-pixel-clear road from the exact southern-road
anchor to the sole north opening of an inert wagon shell. Two road edges and five shell sections
are exposed as exact finite segments. The pure route policy accepts the authored centerline and
rejects road, shell, or world-bound crossings without treating collinear disjoint routes as
collisions.

The main scene derives every collision segment and greybox line from that validated layout and
retains one childless marker at the exact wagon anchor. The switch-gated proof follows the existing
physical shrine -> clearing -> laundry -> stream crossing, stays east of the village shell, enters
the road on-axis, reaches the wagon, and returns. It also proves representative road/wagon
collision rejection and preserves the shrine discovery graph. The wagon footprint was narrowed
during review so the established bottom-border probe remains independent; the obsolete generic
lower-terrain clear probe now exercises the authored open road corridor.

`wagon-clue`, `wagon-residue`, passive noticing, focused search, collection, presentation, and
evidence remain absent from ordinary play. Closed-path behavior, dialogue, inhabitants, remedy
progression, production art, weather, lighting, sound, journal, map, provider, audio, signing,
release, and distribution work remain untouched.

`ROADMAP.md` marks the southern-road/abandoned-wagon greybox complete.

## Validation and review

- Focused Application tests cover exact retained provenance, geometry, minimum clearance, and
  malformed, rebuilt, foreign, swapped, overlapping, out-of-bounds, misaligned-opening,
  collinear-disjoint, road-crossing, and wagon-crossing cases.
- The switch-gated host smoke passes exact geometry/provenance, synchronized and physically clear
  round-trip travel, representative collisions, inert marker state, absent clue/residue state, and
  retained shrine snapshots.
- The first sandboxed `./scripts/quality.sh` passed the zero-warning build, all .NET tests, and the
  scenario validator, then crashed before Godot scene execution because Godot could not open
  `user://logs`. The identical host run exposed two legacy movement-probe conflicts with the new
  shell; review narrowed the wagon footprint and moved only the obsolete open-terrain probe.
- After those fixes, host-local `./scripts/quality.sh` passed workflow validation, formatting, a
  zero-warning build, 2,789 Domain, 644 Application, 1,220 Infrastructure, and 8 architecture
  tests, the representative scenario validator, every credential-free Godot smoke, and the
  graphical Compatibility launch.
- Review keeps every new C# file below 500 lines. The 120-character and whitespace checks are
  clean. No wagon discovery state, input, evidence, or broader Phase 8 behavior was introduced.
- Draft PR #381 targets `main` from implementation commit `05ecec9`. Credential-free CI run
  `36213617312` ended with all five jobs failed before any step was instantiated (`steps: []`), so
  those hosted results are Actions/account infrastructure and provide no product-test evidence;
  the complete host-local validation above remains the executed evidence.

## Exact next bounded slice

Build the closed deeper-forest path greybox boundary without opening the deeper forest.

Acceptance boundary:

1. Retain the existing exact `OpeningRegionWoodlandClearingLayout` and bind its projection's exact
   `closed-deeper-forest-path` anchor into one bounded approach aligned with the clearing's existing
   `ClosedForestPathOpening` and the projection's minimum path clearance.
2. Author only greybox approach boundaries and one visibly closed collision-backed gate/marker.
   Ordinary startup and all input must leave the gate closed.
3. Add pure Application tests for exact projection and clearing provenance, approach geometry,
   clearance, and malformed, rebuilt, foreign, wrong-role, overlapping, out-of-bounds,
   misaligned, open-gate, or boundary-crossing rejection.
4. Add one switch-gated main-scene smoke for synchronized and physically clear travel from the
   clearing to the closed gate, representative side/gate collision rejection, and preservation of
   the clearing rare-specimen, shrine discovery, and road/wagon snapshots.
5. Add no deeper-forest region, unlocking condition, dialogue, inhabitants, wagon or specimen
   discovery, remedy progression, production art, weather, lighting, sound, journal, map,
   provider, audio, signing, release, or distribution work.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
