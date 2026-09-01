# Allrpcs Handover

## Current state

PR #15 merged Phase 2c4 into `origin/main` at merge commit `b5eb800` on 2026-09-01.
The current branch, `codex/phase-2c5-character-inventory-reference`, is based directly on that
verified merge.

Phase 2c5 is implemented, reviewed, and locally validated. It adds only an immutable ordered
collection of existing `ObjectId` references for a character inventory. The broader
character-model roadmap item remains unticked because knowledge, beliefs, relationships, and
goals do not yet exist.

The reviewed slice is ready for its authorized commit, push, and draft pull request. Re-query the
live branch, pull request, and checks after publication rather than relying on this sentence.

At the start of the next session, verify current state:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c5-character-inventory-reference --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c5-character-inventory-reference --repo hherb/allrpcs
```

## Phase 2c5 implementation and review

- `CharacterInventory` contains only `ObjectReferences` as an `IReadOnlyList<ObjectId>`.
- An empty inventory is valid; authored order and the existing typed identifiers are retained.
- Construction copies caller input and rejects null collections, null entries, and duplicate
  identifiers using `ObjectId` value equality.
- The owned collection is read-only at runtime and exposes no quantities, capacity, ownership,
  transfer, visibility, or executable-policy surface.
- Seven focused tests cover empty behavior, ordering, typed retention, input copying, runtime
  immutability, null and duplicate rejection, and the deliberately limited public surface.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c5 without ticking the broader character-model item.

Review confirmed that the type reuses only the existing stable `ObjectId` boundary and does not
pretend that object or item models already exist. The implementation owns its collection, keeps
order deterministic, and exposes no mutation path or mechanics. No correctness, architecture,
or scope issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because `CharacterInventory` did not
exist. All seven focused tests then passed after implementation. The complete local quality
sequence passed on Apple Silicon macOS with .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 113 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

The restricted environment compiled the focused tests but prevented the .NET test host from
binding its loopback socket. The identical focused test and complete quality sequence passed from
the host shell. No sandbox failure, process launch, or screenshot is reported as product evidence.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, object-model, item-model, inventory-mechanic, or runtime-behavior claim.

Explicitly excluded and still absent:

- object and item models, quantities, stacks, slots, weight, capacity, containers, equipment,
  ownership policy, give, take, drop, use, crafting, consumption, and action validation;
- knowledge, beliefs, relationships, goals, mutable character state, and a complete aggregate;
- visibility, public/private classification, perception filtering, authorization, capability
  checks, controller behavior, and provider instructions;
- scenario content, persistence, serialization, save/database schemas, migrations, Godot
  resources or nodes, UI, providers, runtime registration, and plugin surfaces; and
- releases, artifact distribution, credentials, signing, notarization, and repository visibility
  changes. Pull-request merge remains separately authorized.

## Exact next bounded slice

After the user reports that the Phase 2c5 pull request merged, take **Phase 2c6: character
knowledge-reference boundary**. This continues, but does not complete or tick, the broader
authoritative character-model roadmap item.

Acceptance boundary for Phase 2c6:

1. Add one immutable Domain value representing a character's current ordered references to
   existing stable `KnowledgeId` values.
2. Explicitly allow an empty collection, preserve authored order, copy caller input, expose no
   mutable collection, and reject null entries and duplicate identifiers.
3. Add focused pure tests for empty behavior, stable ordering, input copying, typed identifier
   retention, null and duplicate rejection, runtime immutability, and the absence of knowledge
   content, evidence, belief, confidence, visibility, perception, or executable-policy surfaces.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until all listed state is represented and reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c6:

- Do not define knowledge content, world truths, evidence, claims, beliefs, confidence,
  provenance, discovery rules, reinterpretation, or public/private knowledge policy.
- Do not yet add relationships, goals, mutable character state, or a complete aggregate.
- Do not attach visibility, perception filtering, authorization, capability checks, controller
  behavior, dialogue/model context, retrieval, or provider instructions to knowledge references.
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
