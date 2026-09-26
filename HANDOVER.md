# Allrpcs Handover

## Current state

`main` includes merged PR #379 at `e8d35b2`. Branch
`codex/phase-8-inscription-coordinator` completes Phase 8's player-triggered old-shrine
layered-inscription discovery while preserving fail-closed exact authority.

`OpeningRegionLayeredInscriptionDiscoveryCoordinator` retains the exact current
`OpeningRegionOldShrineSceneSource` and accepts only its exact knowledgeable Seeker. One explicit
operation constructs shrine-local access from the live Seeker position, binds the representative
scenario's exact reinterpretation candidate and trigger knowledge, resolves the existing pure
`Noticed`-to-`Identified` transition, and prepares discovery state, presentation, and filtered
`partial-inscription` evidence before the first scene mutation.

`OpeningRegionOldShrineSceneSource.TryCommitInscriptionDiscovery` revalidates the complete exact
graph plus the live Seeker and marker positions, then installs the state, reveals the existing two
inert marker layers, and retains presentation and evidence in one atomic commit. The superseded
piecemeal scene mutation methods were removed during review, so no public partial-application path
remains.

Ordinary startup remains concealed. The mapped physical I action invokes the exact coordinator;
released, unrelated, far, stale, foreign, rebuilt, missing-knowledge, already-visible, and repeated
paths remain inert. The temporary control panel and README document this ordinary activation. No
player-facing inscription copy, journal, persistence, focused-search/focus integration, new
knowledge acquisition, collection, or broader evidence route was added.

`ROADMAP.md` now marks player-triggered layered-inscription discovery complete. Road, wagon,
closed-path, dialogue, inhabitants, remedy progression, production art, weather, lighting, sound,
map, provider, audio, signing, release, and distribution work remain untouched.

## Validation and review

- A zero-warning solution build passes after the coordinator, atomic commit, input action, and
  scene wiring changes.
- The host-local switch-gated old-shrine smoke passes complete preflight, atomic accepted discovery,
  ordinary I-key activation, one-shot marker reveal and evidence exposure, null/foreign/rebuilt/
  missing-knowledge/far/moved-marker/already-visible/repeated rejection, released and unrelated
  input inertness, exact scenario/source retention, five collision-backed boundaries, and the
  synchronized clearing round trip.
- The sandboxed focused smoke again failed before scene execution because Godot could not open
  `user://logs`; the identical host-local command above passed.
- Review removed the obsolete partial scene mutation API. The new coordinator and modified shrine
  source/smoke files remain below 500 lines; 120-character and whitespace checks are clean. Existing
  long `Main.cs` and `InteractionFocusSmokeCheck.cs` received only narrow wiring/assertion edits.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,789 Domain, 638 Application, 1,220 Infrastructure, and 8 architecture tests, the representative
  scenario validator, every credential-free Godot smoke, and the graphical Compatibility launch.
  The first complete run found only a stale interaction-focus smoke expectation for the temporary
  control label; review updated that assertion and added an exact physical-I mapping check before the
  final complete pass.
- Draft PR #380 targets `main` from implementation commit `67cd97d`. Initial credential-free CI
  run `36211237831` ended with all five jobs failed before any step was instantiated (`steps: []`),
  so those hosted results are Actions/account infrastructure and provide no product-test evidence;
  the complete host-local validation above remains the executed evidence.

## Exact next bounded slice

Build the southern-road and abandoned-wagon greybox shell without activating wagon discovery.

Acceptance boundary:

1. Bind the projection's exact retained `southern-road` and `abandoned-wagon` anchors into one
   bounded collision-aware road/wagon layout that preserves the existing old-shrine connection and
   the wagon connection with the projection's minimum path clearance.
2. Author only greybox road boundaries and one inert wagon shell/marker. Keep `wagon-clue`,
   `wagon-residue`, passive noticing, focused search, collection, presentation, and evidence state
   absent from ordinary play.
3. Add pure Application tests for exact projection provenance, geometry, clearance, and malformed,
   rebuilt, foreign, swapped, overlapping, out-of-bounds, or route-crossing rejection.
4. Add one switch-gated main-scene smoke for synchronized and physically clear old-shrine ->
   southern-road -> abandoned-wagon travel and representative collision rejection. Preserve all
   existing shrine discovery, scene, navigation, and source snapshots.
5. Add no closed deeper-forest path, dialogue, inhabitants, remedy progression, production art,
   weather, lighting, sound, journal, map, provider, audio, signing, release, or distribution work.
6. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next Phase 8 slice.
