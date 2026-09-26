# Allrpcs Handover

## Current state

`main` includes merged PR #382 at `d5cfd98`. Branch
`codex/phase-8-environmental-state` adds scene-owned qualitative environmental state and one exact
location-scoped greybox presentation boundary.

`OpeningRegionEnvironmentalStateSource` creates the representative baseline from the projection's
exact opening topology and owns that same instance as the exact current snapshot at ordinary
startup. `OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator` accepts only the
old-shrine source's exact current presentation, the source's exact baseline/current pair, and its
own exact replacement woodland light and sound states. It delegates the existing pure transition
and installs the varied snapshot once through exact-instance compare-and-swap. Null, absent,
rebuilt, foreign, stale, already-varied, and repeated paths remain inert.

`OpeningRegionLayeredInscriptionEnvironmentalVariationInstallationPolicy` keeps installation
authority provider- and scene-neutral. The installed variation retains baseline weather and every
non-woodland region while changing only the exact woodland lighting and sound states. Discovery,
evidence, the baseline, time, persistence, rendered lighting, and audio playback remain unchanged.

`OpeningRegionEnvironmentalLocationPresentationProjection` resolves only an exact retained
location against one exact current region set. Its immutable result retains the exact location,
region, weather, lighting, and sound instances and exposes fixed greybox text containing only their
canonical keys. `OpeningRegionEnvironmentalGreyboxPresentationController` accepts only the exact
current snapshot and exact old-shrine location once. Its dedicated label stays hidden at ordinary
startup; its public current presentation remains available only while the source, label visibility,
and exact text remain current.

The switch-gated main-scene proof starts from inert baseline ownership, rejects absent/rebuilt
variation inputs, performs the exact inscription discovery, installs one exact environmental
variation, rejects a rebuilt state and repeated install, preserves weather/non-woodland regions and
the exact discovery/evidence graph, rejects a foreign location, and presents the exact old-shrine
canonical-key text once. No ordinary input invokes the environmental coordinators yet.

`ROADMAP.md` keeps the broader weather, lighting, sound, and story-state item open. Automatic
post-discovery environmental flow, rendered light, ambient playback, weather simulation, time
progression, persistence, production assets, other location presentations, other discovery
variations, deeper-forest content, dialogue, inhabitants, journal, map, signing, release, and
distribution remain untouched.

## Validation and review

- Focused TDD added 4 Application tests: 2 for exact baseline/current/presentation/variation
  installation authority and null/foreign rejection, plus 2 for exact location projection and
  null/foreign-location rejection. The existing variation suite now totals 5 tests.
- The sandboxed `./scripts/quality.sh` passed the zero-warning build, all .NET tests, and scenario
  validation, then crashed before Godot execution because Godot could not open `user://logs`.
- The identical host-local `./scripts/quality.sh` passed workflow validation, formatting, a
  zero-warning build, 2,796 Domain, 658 Application, 1,220 Infrastructure, and 8 architecture tests,
  the representative scenario validator, every credential-free Godot smoke, and the graphical
  Compatibility launch.
- Session review found that the greybox controller's current token initially survived external
  label hiding or text replacement. The getter now requires the exact current region set, visible
  label, and exact projected text; one-shot internal ownership still prevents replay after staleness.
  The focused build, format check, whitespace check, and host Godot smoke passed after that fix.
- Every new C# file is below 500 lines; the 120-character and whitespace checks are clean.
- Draft PR #383 targets `main` from implementation commit `e95a0a5`. Credential-free CI run
  `36216677563` ended with all five jobs failed before any step was instantiated (`steps: []`), so
  those hosted results are Actions/account infrastructure and provide no product-test evidence;
  the complete host-local validation above remains the executed evidence.

## Exact next bounded slice

Connect one successful ordinary layered-inscription discovery to the existing qualitative
environmental variation and old-shrine greybox presentation, without adding environmental assets.

Acceptance boundary:

1. Add one narrow scene flow coordinator that accepts only the old-shrine source's exact current
   `OpeningRegionLayeredInscriptionPresentation`, the environmental source's exact baseline/current
   pair, the variation coordinator's exact authored replacement states, and an empty/current
   greybox presenter. Preflight the complete graph before changing environmental or label state.
2. Delegate only to the existing variation coordinator, exact location projection, and greybox
   presentation controller. Retain one exact completed flow token only after the varied snapshot and
   exact old-shrine presentation are both current. Do not duplicate transition or lookup rules.
3. In `Main._UnhandledInput`, invoke that flow only when the existing mapped I action successfully
   creates the exact inscription presentation. Preserve the discovery commit independently if the
   downstream environmental preflight rejects; do not retry or roll discovery back.
4. Null, released, unrelated, rebuilt, foreign, stale, already-varied, already-presented, and
   repeated inputs must leave environmental and label state unchanged. Ordinary startup and every
   unrelated input remain inert.
5. Add no rendered light, shader, weather particles, audio stream or playback, timer, time
   progression, persistence, provider/model work, production asset, broader UI, or other location
   and discovery variation.
6. Add focused Application coverage only if a new pure boundary is required, plus one switch-gated
   main-scene smoke proving one ordinary I-key discovery -> exact variation -> exact label flow and
   rejected/repeated-path inertness. Run `./scripts/quality.sh`, review the complete diff, and leave
   one exact next Phase 8 slice.
