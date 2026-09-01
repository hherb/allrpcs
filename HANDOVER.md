# Allrpcs Handover

## Current state

PR #16 merged Phase 2c5 into `origin/main` at merge commit `a5fe0ea` on 2026-09-01.
The current branch, `codex/phase-2c6-character-knowledge-reference`, is based directly on that
verified merge.

Phase 2c6 is implemented, reviewed, and locally validated. It adds only an immutable ordered
collection of existing `KnowledgeId` references for a character. The broader character-model
roadmap item remains unticked because beliefs, relationships, and goals do not yet exist.

The private remote has open draft PR #17 for this branch. The reviewed implementation, local
evidence, and initial handover are commit `39d3a0d`. Initial pull-request run `33461366432`
started all five credential-free jobs at that commit. This hosted-evidence update creates a new
branch head, so re-query its final checks rather than assuming the initial commit's result applies
to it. Pull-request merge remains unauthorized.

At the start of the next session, verify current state:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c6-character-knowledge-reference --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c6-character-knowledge-reference --repo hherb/allrpcs
```

## Phase 2c6 implementation and review

- `CharacterKnowledge` contains only `KnowledgeReferences` as an
  `IReadOnlyList<KnowledgeId>`.
- Empty knowledge is valid; authored order and the existing typed identifiers are retained.
- Construction copies caller input and rejects null collections, null entries, and duplicate
  identifiers using `KnowledgeId` value equality.
- The owned collection is read-only at runtime and exposes no knowledge content, truth, evidence,
  claims, beliefs, confidence, provenance, discovery, visibility, perception, or executable-policy
  surface.
- Seven focused tests cover empty behavior, ordering, typed retention, input copying, runtime
  immutability, null and duplicate rejection, and the deliberately limited public surface.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c6 without ticking the broader character-model item.

Review confirmed that the type reuses only the existing stable `KnowledgeId` boundary, owns its
collection, keeps order deterministic, and exposes no mutation path or knowledge mechanics. No
correctness, architecture, or scope issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because `CharacterKnowledge` did not
exist. All seven focused tests then passed after implementation. The complete local quality
sequence passed on Apple Silicon macOS with .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 120 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

The restricted environment compiled the focused tests but prevented the .NET test host from
binding its loopback socket. The identical focused test and complete quality sequence passed from
the host shell. No sandbox failure, process launch, or screenshot is reported as product evidence.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, knowledge-content, belief, or runtime-behavior claim.

Explicitly excluded and still absent:

- knowledge content, world truths, evidence, claims, beliefs, confidence, provenance, discovery,
  reinterpretation, and public/private knowledge policy;
- relationships, goals, mutable character state, and a complete aggregate;
- visibility, perception filtering, authorization, capability checks, controller behavior,
  dialogue/model context, retrieval, and provider instructions;
- scenario content, persistence, serialization, save/database schemas, migrations, Godot
  resources or nodes, UI, providers, runtime registration, and plugin surfaces; and
- releases, artifact distribution, credentials, signing, notarization, and repository visibility
  changes. Pull-request merge remains separately authorized.

## Exact next bounded slice

After the user reports that the Phase 2c6 pull request merged, take **Phase 2c7: character belief
boundary**. This continues, but does not complete or tick, the broader authoritative
character-model roadmap item.

Acceptance boundary for Phase 2c7:

1. Add one immutable `CharacterBelief` Domain value with a canonical key and a nonblank authored
   statement bounded to 500 UTF-16 code units. Preserve valid authored statement text exactly.
2. Add a structurally separate `CharacterBeliefSet` that explicitly permits an empty sequence,
   preserves authored entry order, copies caller input, exposes no mutable collection, and rejects
   null entries and duplicate canonical keys.
3. Add focused pure tests for value validation, exact text preservation, value equality, empty and
   ordered collection behavior, input copying, null and duplicate rejection, runtime immutability,
   and the absence of truth, evidence, confidence, provenance, visibility, or executable-policy
   surfaces.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until all listed state is represented and reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c7:

- Do not classify a belief as world truth or knowledge, or attach supporting or contradicting
  evidence, confidence, provenance, attributed claims, discovery, or reinterpretation rules.
- Do not add public/private belief policy, visibility, perception filtering, authorization,
  capability checks, action validation, dialogue/model context, retrieval, or provider
  instructions.
- Do not yet add relationships, goals, mutable character state, or a complete aggregate.
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
