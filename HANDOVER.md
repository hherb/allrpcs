# Allrpcs Handover

## Current state

PR #18 merged Phase 2c7 into `origin/main` at merge commit `db6d495` on 2026-09-01.
The current branch, `codex/phase-2c8-character-relationship`, is based directly on that verified
merge.

Phase 2c8 is implemented, reviewed, and locally validated. It adds only an immutable relationship
value and an ordered relationship collection. The broader character-model roadmap item remains
unticked because goals and a complete aggregate do not yet exist. A draft pull request still
needs to be opened for this branch. Pull-request merge remains unauthorized.

At the start of the next session, verify current state:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c8-character-relationship --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c8-character-relationship --repo hherb/allrpcs
```

## Phase 2c8 implementation and review

- `CharacterRelationship` associates an existing typed `CharacterId` with a nonblank authored
  `Summary` bounded to 500 UTF-16 code units.
- Valid summaries preserve whitespace, casing, line endings, and Unicode exactly. Record value
  equality uses both fields.
- `CharacterRelationshipSet` permits an empty sequence, preserves authored order, copies caller
  input, exposes a runtime read-only collection, and rejects null entries and duplicate related
  character identifiers using `CharacterId` value equality.
- Fourteen focused tests cover typed identifier ownership, summary validation, exact text
  preservation, equality, empty and ordered collection behavior, input copying, null and
  duplicate rejection, runtime immutability, and the deliberately limited public surfaces.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c8 without ticking the broader character-model item.

Review confirmed that the relationship retains the typed related-character identifier, owns its
collection, preserves deterministic authored order, and exposes no score, dimension, reason,
history, visibility, or executable-policy surface. It deliberately neither accepts nor rejects a
self-relationship because no owning aggregate exists. No correctness, architecture, or scope
issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because the relationship types did
not exist. After implementation, the restricted environment compiled the focused suite but its
.NET test host could not bind local transport. The identical host-shell run passed all 14 focused
tests.

The complete `./scripts/quality.sh` sequence then passed from the host shell on Apple Silicon
macOS with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 153 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, relationship-correctness, or runtime-behavior claim.

Still explicitly excluded:

- relationship scores, dimensions, reasons, history, decay, mutation, events, reciprocity,
  symmetry, direction inverses, self-relationship policy, and graph traversal;
- public/private relationship policy, visibility, perception filtering, authorization,
  capability checks, action validation, dialogue/model context, retrieval, and provider
  instructions;
- goals, mutable character state, and a complete aggregate;
- scenario content, persistence, serialization, save/database schemas, migrations, Godot
  resources or nodes, UI, providers, runtime registration, and plugin surfaces; and
- releases, artifact distribution, credentials, signing, notarization, and repository visibility
  changes. Pull-request merge remains separately authorized.

## Exact next bounded slice

After the user reports that the Phase 2c8 pull request merged, take **Phase 2c9: character goal
boundary**. This continues, but does not complete or tick, the broader authoritative
character-model roadmap item.

Acceptance boundary for Phase 2c9:

1. Add one immutable `CharacterGoal` Domain value with a stable canonical `Key` and a nonblank
   authored `Description` bounded to 500 UTF-16 code units. Preserve valid key and description
   text exactly.
2. Add a `CharacterGoalSet` that explicitly permits an empty sequence, preserves authored entry
   order, copies caller input, exposes no mutable collection, and rejects null entries and
   duplicate keys using ordinal equality.
3. Add focused pure tests for key and description validation, exact text preservation, value
   equality, empty and ordered collection behavior, input copying, null and duplicate rejection,
   runtime immutability, and the absence of priority, status, progress, deadline, plan, reward,
   visibility, or executable-policy surfaces.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until a separate slice reviews and composes the listed
   state into a complete aggregate.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c9:

- Do not add goal priority, status, progress, deadlines, dependencies, plans, completion,
  rewards, mutation, event handling, or dialogue-driven change mechanics.
- Do not attach goals to controllers, select behavior, validate actions, or create executable
  policy.
- Do not yet add mutable character state or a complete aggregate.
- Do not define scenario contents, persistence, serialization, database/save schemas, migrations,
  Godot resources or nodes, UI, providers, runtime registration, or plugin surfaces.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not change repository visibility, merge pull requests, create releases, distribute
  artifacts, or add credentials without explicit user authorization.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact bounded slice and preserve its exclusions.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Run `./scripts/quality.sh`, inspect all intended paths, and stage only the reviewed slice.
- Record branch, commit, pull request, checks, native evidence, and limitations without
  overstating unavailable platform, manual, signing, release, or distribution validation.
