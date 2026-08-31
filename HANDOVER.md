# Allrpcs Handover

## Current state

PR #13 merged Phase 2c2 into `origin/main` at merge commit `384d023` on 2026-09-01.
The current branch, `codex/phase-2c3-controller-assignment`, is based directly on that verified
merge.

Phase 2c3, the character controller-assignment boundary, is implemented, reviewed, and locally
validated on the current branch. It adds one immutable, engine-independent Domain value that
pairs an existing stable character identifier with one existing closed controller type. The
broader character-model roadmap item remains unticked because condition, inventory, knowledge,
beliefs, relationships, and goals do not yet exist.

The implementation has not yet been published. Push and draft pull-request creation are
authorized for this slice. The pull request is not authorized to merge. Public visibility,
releases, artifact distribution, credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c3-controller-assignment --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c3-controller-assignment --repo hherb/allrpcs
```

## Phase 2c3 implementation

The slice remains within its original boundary:

- `CharacterControllerAssignment` is a sealed immutable record in `Domain`.
- Its constructor accepts one existing `CharacterId` and one existing `ControllerType`, rejects
  null values, and retains both supplied objects exactly.
- Record equality compares the typed character identifier and closed controller type. Different
  character identifiers or controller types therefore produce different assignments.
- The value represents only the current authored, authoritative association. Its public surface
  contains only `CharacterId` and `ControllerType`.
- Five focused tests prove exact retention, value equality, character distinction, all four
  controller types, null rejection, and the absence of provider, model, configuration, or
  behavior properties.
- `README.md` documents the contract and exclusions. `ROADMAP.md` records Phase 2c3 progress
  while deliberately leaving the broader character-model item unticked.

## Review and validation

### Automated evidence

On 2026-09-01, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- the test-first focused Domain run failed to compile because
  `CharacterControllerAssignment` did not exist, then passed all five focused tests after
  implementation;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed;
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers -m:1 /nr:false` built all seven
  projects with zero warnings and zero errors;
- the quality sequence passed workflow linting, serial restore, formatting verification, the
  zero-warning build, all 86 Domain tests, all 100 Application tests, and all six architecture
  reference tests; and
- the four constituent Godot smokes passed, followed by a bounded 60-frame native graphical
  launch using the Compatibility renderer on the Apple M3 Max.

The navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map iteration 2
and found the bridge route while rejecting representative obstacle targets. The healer-command,
interaction-focus, and movement-boundary smokes repeated their complete established acceptance
paths without a regression.

The focused test host first compiled inside the restricted environment, then could not bind its
loopback socket. The identical focused test passed from the host shell. Aggregate quality
invocations later stalled after their pure tests while the untraced smoke wrapper had no live
Godot child; only those validation processes were interrupted. The same smoke wrapper passed
with shell tracing, and each of its four Godot commands plus the bounded native launch also passed
individually from the host shell. No sandbox failure, stalled shell, process launch, or screenshot
is being reported as a product result.

### Native evidence

No manual play acceptance is required for this engine-independent Domain slice. The bounded
native regression launch opened the current scene with Godot's Compatibility renderer on the
Apple M3 Max and exited cleanly after 60 frames. The existing Godot smokes passed as regression
checks only. This slice makes no visual, input, physics, scene, renderer, controller-behavior, or
controller-switching claim.

## Review findings

The assignment composes the already-reviewed typed identifier and closed controller primitive
rather than introducing another identifier grammar, controller registry, or stringly typed
controller value. Null inputs fail closed, while valid objects are neither copied nor normalized.
The record's structural equality matches the value boundary: the same character and controller
type compare equal, while character identity and controller category remain independently
significant.

The public surface is intentionally only the two associated values. It cannot create a
controller, choose a provider or model, hold configuration, define prompts or tools, route player
input, schedule decisions, switch controller types, or execute behavior. Domain retains no
project references. No correctness, security, architecture, or scope issue remained after
review.

## Limitations and exclusions

- Phase 2c3 does not define a complete character aggregate or mutable character state.
- It does not add condition, inventory, knowledge, beliefs, relationships, or goals.
- It does not create controller instances, lifecycle, configuration, prompts, tools, scheduling,
  switching policy, dormant transitions, player input routing, deterministic behavior, runtime
  orchestration, providers, or models.
- The assignment is current authored state only. It does not record assignment history, fallback
  types, eligibility, authorization, capability, or proof that any controller is available.
- It does not define scenario characters, action validation, perception filtering,
  dialogue/model context, authorization policy, persistence, or serialization behavior.
- No Godot resource, node, UI, save schema, database mapping, migration, provider adapter,
  runtime registration, or extensible plugin surface was added.
- Existing native behavior and future hosted cross-platform builds are regression surfaces, not
  new Phase 2c3 behavior claims. No manual or cross-platform assignment-specific acceptance was
  required or performed.

## Exact next bounded slice

After the user reports that the Phase 2c3 pull request merged, take **Phase 2c4: character
condition vocabulary boundary**. This continues, but does not complete or tick, the broader
authoritative character-model roadmap item.

Acceptance boundary for Phase 2c4:

1. Add structurally separate immutable Domain values for an authored qualitative character
   condition and an ordered collection of those values.
2. Give each condition a canonical key using the existing identifier-text grammar and a
   nonblank authored description bounded to 500 UTF-16 code units; preserve valid description
   text exactly without normalization.
3. Explicitly allow an empty condition collection, preserve authored order, copy caller input,
   and reject null entries and duplicate canonical keys using ordinal comparison.
4. Add focused pure tests for equality, key and description boundaries, exact preservation,
   empty-set behavior, stable ordering, input copying, duplicate rejection, and disease/runtime
   non-assumptions.
5. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until all of its listed state is represented and
   reviewed.
6. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c4:

- Do not define disease stages, infection, transmission, progression, treatment, stabilization,
  cure, death, timers, meaningful-time events, symptoms, severity math, or health mechanics.
- Do not yet add inventory, knowledge, beliefs, relationships, goals, mutable character state,
  or a complete character aggregate.
- Do not attach visibility, public/private classification, perception filtering, authorization,
  capability checks, controller switching, action validation, or provider instructions to a
  condition.
- Do not define scenario characters, dialogue/model context, persistence, or serialization
  behavior.
- Do not add Godot nodes/resources, UI, database/save schemas, migrations, providers, runtime
  registration, or extensible plugin surfaces.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not change repository visibility, merge pull requests, create releases, distribute
  artifacts, or add credentials without explicit user authorization.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact bounded slice and preserve its exclusions.
- Tick roadmap items only after current automated and manual evidence exists.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Keep changes narrow, inspect tracked and untracked files, and stage only intended paths.
- Record branch, commit, pull request, checks, native evidence, and limitations without
  overstating unavailable platform, manual, or release validation.
