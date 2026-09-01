# Allrpcs Handover

## Current state

PR #14 merged Phase 2c3 into `origin/main` at merge commit `2eff73e` on 2026-09-01. The
current branch, `codex/phase-2c4-character-condition`, is based directly on that verified merge.

Phase 2c4, the character-condition vocabulary boundary, is implemented, reviewed, and locally
validated on the current branch. It adds an immutable qualitative authored condition and an
ordered, copied, duplicate-free collection without adding disease or runtime health mechanics.
The broader character-model roadmap item remains unticked because inventory, knowledge, beliefs,
relationships, and goals do not yet exist.

The private remote has open draft PR #15 for this branch. The reviewed implementation, local
evidence, and initial handover are commit `b671810`; this hosted-evidence update follows it. All
five credential-free jobs in pull-request run `33455001527` passed at that commit: pure C# quality,
the Linux headless Godot smoke, and unsigned development builds on Linux x64, macOS Universal 2,
and Windows x64. The unexpired hosted artifacts are named `UNSIGNED-DEVELOPMENT-linux-x64`,
`UNSIGNED-DEVELOPMENT-macos-universal`, and `UNSIGNED-DEVELOPMENT-windows-x64`. The macOS and
Windows jobs rejected a bundle signature and Authenticode signature respectively before upload.
These artifacts are development evidence, not releases or approved distribution artifacts. This
documentation commit creates a new branch head, so re-query its final checks rather than assuming
the implementation commit's result applies to it. Pull-request merge remains unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c4-character-condition --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c4-character-condition --repo hherb/allrpcs
```

## Phase 2c4 implementation and review

- `CharacterCondition` is a sealed immutable Domain record with only `Key` and `Description`.
- Keys use the existing 1-to-64-character canonical identifier-text grammar.
- Descriptions require a non-whitespace character, are limited to 500 UTF-16 code units, and
  preserve valid authored text exactly without trimming or normalization.
- `CharacterConditionSet` explicitly permits an empty sequence, preserves authored order, copies
  caller input, and rejects null entries and duplicate keys using ordinal comparison.
- Twenty focused tests cover equality, structural separation, key and description boundaries,
  empty-set behavior, ordering, copying, null and duplicate rejection, and the deliberately
  limited public surface.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c4 progress without ticking the broader character-model item.

Review confirmed that the implementation reuses the existing canonical-text, authored-text, and
ordered-entry helpers without coupling condition to capability or limitation types. The public
surface cannot express disease stages, symptoms, severity, progression, treatment, timers, death,
or runtime behavior. No correctness, architecture, or scope issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because the condition types did not
exist, then all 20 focused tests passed after implementation. The complete local quality sequence
passed on Apple Silicon macOS with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 106 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

The restricted environment compiled the focused tests but prevented the .NET test host from
binding its loopback socket. The identical focused test and the complete quality sequence passed
from the host shell. No sandbox failure, process launch, or screenshot is reported as product
evidence.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, disease, health-mechanic, or runtime-behavior claim.

Explicitly excluded and still absent:

- disease stages, infection, transmission, progression, treatment, stabilization, cure, death,
  symptoms, severity math, timers, meaningful-time events, and health mechanics;
- inventory, knowledge, beliefs, relationships, goals, mutable character state, and a complete
  character aggregate;
- visibility or public/private classification, perception filtering, authorization, capability
  checks, controller switching, action validation, and provider instructions;
- scenario characters, dialogue/model context, persistence, serialization, Godot resources or
  nodes, UI, save/database schemas, migrations, providers, and runtime registration; and
- releases, artifact distribution, credentials, signing, notarization, and repository visibility
  changes. Pull-request merge remains separately authorized.

## Exact next bounded slice

After the user reports that the Phase 2c4 pull request merged, take **Phase 2c5: character
inventory-reference boundary**. This continues, but does not complete or tick, the broader
authoritative character-model roadmap item.

Acceptance boundary for Phase 2c5:

1. Add one immutable Domain value representing the character inventory's current ordered
   references to existing stable `ObjectId` values.
2. Explicitly allow an empty inventory, preserve authored order, copy caller input, expose no
   mutable collection, and reject null entries and duplicate identifiers.
3. Add focused pure tests for empty behavior, stable ordering, input copying, typed identifier
   retention, null and duplicate rejection, and the absence of quantities, capacity, ownership,
   transfer, visibility, or executable-policy surfaces.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until all of its listed state is represented and
   reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c5:

- Do not define object or item models, quantities, stacks, slots, weight, capacity, containers,
  equipment, ownership policy, give, take, drop, use, crafting, consumption, or action validation.
- Do not yet add knowledge, beliefs, relationships, goals, mutable character state, or a complete
  character aggregate.
- Do not attach visibility, public/private classification, perception filtering, authorization,
  capability checks, controller behavior, or provider instructions to inventory references.
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
