# Allrpcs Handover

## Current state

`main` includes merged PR #381 at `1c51641`. Branch
`codex/phase-8-closed-forest-gate` completes the closed deeper-forest approach and adds
provider-neutral environmental-region and inscription-variation foundations.

`OpeningRegionClosedForestPathSceneBinding` retains the exact existing woodland-clearing layout,
the projection's exact `closed-deeper-forest-path` anchor, and their exact projected connection.
`OpeningRegionClosedForestPathLayout` aligns a 48-pixel approach throat with the clearing's existing
opening, retains 24 pixels of route clearance, and closes it with one immutable gate boundary. Two
collision-backed approach sides and a visible collision-backed gate remain inside the clearing.
The distant deeper-forest anchor is provenance only and is not reachable.

The switch-gated proof reaches the near-side clearance point through synchronized navigation and
physical movement, rejects representative side and gate crossings, and preserves the concealed
rare specimen, shrine discovery, southern-road, and wagon snapshots. Review shortened only the
gate throat after the first full run found that its initial side boundary clipped the established
clearing-to-shrine route; both the retained shrine smoke and the new gate smoke now pass.

`OpeningRegionEnvironmentalRegionSet` assigns every exact opening-topology location exactly once
across four stable regions: village shelter, waterway, woodland edge, and southern road. Each region
retains exact qualitative weather, lighting, and sound states from one catalog. Missing, overlapping,
rebuilt, foreign, duplicate-key, and unretained-state graphs fail closed, while exact-location lookup
returns only the retained region.

`OpeningRegionLayeredInscriptionEnvironmentalVariationTransition` accepts only the exact existing
layered-inscription presentation from the baseline topology. It returns a new immutable snapshot
that retains weather and every non-woodland region while replacing only woodland lighting and sound
with exact caller-authored states. It changes no discovery, scene, playback, or baseline state.

`ROADMAP.md` marks the closed deeper-forest path complete. The broader weather, lighting, sound, and
story-state item remains open because no scene owner, ambient playback, or rendered lighting was
added. Deeper-forest content or unlocking, additional discovery routes, production assets, weather
or time simulation, dialogue, inhabitants, progression, journal, map, provider, audio, signing,
release, and distribution work remain untouched.

## Validation and review

- Focused TDD added 7 Application tests for exact closed-path provenance, geometry, clearance,
  malformed/rebuilt/foreign/wrong-role/overlapping/out-of-bounds/misaligned/open-gate and
  boundary-crossing rejection; 7 Domain tests for exact complete environmental regions; and 3
  Application tests for exact inscription-authorized variation.
- The first sandboxed `./scripts/quality.sh` passed the zero-warning build, all .NET tests, and the
  scenario validator, then crashed before Godot scene execution because Godot could not open
  `user://logs`. The identical host run exposed the clearing/shrine collision described above.
- After review fixes, host-local `./scripts/quality.sh` passed workflow validation, formatting, a
  zero-warning build, 2,796 Domain, 654 Application, 1,220 Infrastructure, and 8 architecture tests,
  the representative scenario validator, every credential-free Godot smoke, and the graphical
  Compatibility launch.
- Session review also replaced an implicit missing-woodland `Single` failure with an explicit
  fail-closed argument rejection and regression coverage. Every new C# file is below 500 lines;
  the 120-character and whitespace checks are clean.
- Draft PR #382 targets `main` from implementation commit `1030be9`. Credential-free CI run
  `36215387525` ended with all five jobs failed before any step was instantiated (`steps: []`), so
  those hosted results are Actions/account infrastructure and provide no product-test evidence;
  the complete host-local validation above remains the executed evidence.

## Exact next bounded slice

Add scene-owned qualitative environmental state and apply the existing pure inscription variation
once, without rendering or playing environmental assets.

Acceptance boundary:

1. Add one `OpeningRegionEnvironmentalStateSource` that creates the representative baseline from
   `OpeningRegionProjectionSource.Projection.Topology` and exposes that exact baseline as the exact
   current region set at ordinary startup.
2. Add one narrow coordinator that accepts only the old-shrine source's exact current
   `OpeningRegionLayeredInscriptionPresentation`, the environmental source's exact baseline and
   current snapshots, and scene-authored replacement woodland lighting/sound states. Delegate to
   `OpeningRegionLayeredInscriptionEnvironmentalVariationTransition` and install its varied snapshot
   once through exact-instance compare-and-swap.
3. Keep ordinary startup inert beyond baseline ownership. Null, absent, rebuilt, foreign, stale,
   already-varied, or repeated inputs must leave the exact current snapshot unchanged.
4. Wire the source and coordinator into `Main.tscn` after their projection and old-shrine owners.
   Add no automatic input handling, model work, time progression, discovery mutation, audio stream,
   rendered light, shader, weather particles, persistence, or production asset.
5. Add focused Application tests for exact installation authority and one switch-gated main-scene
   smoke proving inert startup, one exact post-presentation variation, repeated rejection, retained
   weather/non-woodland regions, and unchanged discovery/evidence state.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice for
   greybox environmental presentation boundaries.
