# Allrpcs Handover

## Current state

PR #19 merged Phase 2c8 into `origin/main` at merge commit `90c62d8` on 2026-09-01.
The current branch, `codex/phase-2c9-character-goal`, is based directly on that verified merge.

Phase 2c9 is implemented, reviewed, and locally validated. It adds only an immutable authored
goal value and an ordered goal collection. The broader character-model roadmap item remains
unticked because the existing character values have not yet been composed and reviewed as one
complete aggregate.

The private remote has open draft PR #20 for this branch. Reviewed slice commit `73a5c05` passed
all five jobs in hosted run `33464116859`. This handover-only evidence update creates a new branch
head, so re-query its final checks rather than assuming the earlier run applies. Hosted desktop
outputs are unsigned development evidence only; pull-request merge remains separately authorized.

At the start of the next session, verify the live branch, pull request, and checks before acting:

```sh
git status --short --branch
git log -4 --oneline --decorate
git fetch origin main
gh pr view codex/phase-2c9-character-goal --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c9-character-goal --repo hherb/allrpcs
```

## Phase 2c9 result

- `CharacterGoal` has a stable canonical `Key` and a nonblank authored `Description` bounded to
  500 UTF-16 code units. Valid key and description text is preserved exactly; record equality
  uses both values.
- `CharacterGoalSet` permits an empty sequence, preserves authored order, copies caller input,
  exposes a runtime read-only collection, and rejects null entries and duplicate keys using
  ordinal equality.
- Nineteen focused tests cover key and description validation, exact text preservation, value
  equality, empty and ordered collection behavior, input copying, null and duplicate rejection,
  runtime immutability, and the deliberately limited public surfaces.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c9 without ticking the broader character-model item.

Review confirmed that the goal boundary reuses the shared canonical-key and authored-text
validators, owns its ordered collection, and exposes no priority, status, progress, deadline,
plan, reward, visibility, mutation, behavior-selection, authorization, or executable-policy
surface. No correctness, architecture, or scope issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because the goal types did not exist.
After implementation, the restricted environment compiled the suite but did not complete its
.NET test-host transport. The identical host-shell run passed all 19 focused tests.

The complete `./scripts/quality.sh` sequence then passed on Apple Silicon macOS with .NET SDK
8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 172 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, goal-correctness, or runtime-behavior claim. Releases, artifact
distribution, credentials, signing, notarization, repository visibility changes, and pull-request
merge remain unauthorized.

## Exact next bounded slice

After the user reports that the Phase 2c9 pull request merged, take **Phase 2c10: immutable
character aggregate**. This completes the structural character-model roadmap item without adding
mutable state or runtime mechanics.

Acceptance boundary for Phase 2c10:

1. Add one immutable `Character` Domain aggregate that composes exactly one existing
   `CharacterIdentity`, `PublicCharacterBackground`, `PrivateCharacterBackground`,
   `CharacterCapabilitySet`, `CharacterLimitationSet`, `CharacterControllerAssignment`,
   `CharacterConditionSet`, `CharacterInventory`, `CharacterKnowledge`, `CharacterBeliefSet`,
   `CharacterRelationshipSet`, and `CharacterGoalSet`.
2. Reject null components and require the controller assignment's `CharacterId` to equal the
   identity's `Id`. Retain the supplied immutable component instances; add no other cross-field
   policy.
3. Add focused pure tests for exact composition, valid empty child collections, null rejection,
   identifier mismatch rejection, the complete read-only public property surface, and the absence
   of setters, mutation methods, events, behavior selection, or executable policy.
4. Preserve `Domain -> []`; update `README.md`, `ROADMAP.md`, and this file. Tick the broader
   character-model roadmap item only if review confirms that every listed value is composed and
   the full validation sequence passes.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c10:

- Do not add mutable character state, lifecycle methods, event handling, or runtime registration.
- Do not add relationship reciprocity, self-relationship policy, graph traversal, goal priority,
  progress or completion, capability authorization, action validation, or behavior selection.
- Do not add public/private filtered views, perception snapshots, dialogue/model context,
  retrieval, provider instructions, or LLM orchestration.
- Do not define scenario content, persistence, serialization, database/save schemas, migrations,
  Godot resources or nodes, UI, providers, or plugin surfaces.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not change repository visibility, merge pull requests, create releases, distribute
  artifacts, or add credentials without explicit user authorization.

## Working conventions

- Start from the verified merged base and take only the exact bounded slice above.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Run `./scripts/quality.sh`, inspect all intended paths, and stage only the reviewed slice.
- Report native/manual, signing, release, and distribution evidence without overstating it.
