# Allrpcs Handover

## Current state

PR #17 merged Phase 2c6 into `origin/main` at merge commit `4229759` on 2026-09-01.
The current branch, `codex/phase-2c7-character-belief`, is based directly on that verified merge.

Phase 2c7 is implemented, reviewed, and locally validated. It adds only an immutable authored
belief value and an ordered belief collection. The broader character-model roadmap item remains
unticked because relationships and goals do not yet exist.

At the start of the next session, verify current state:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c7-character-belief --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c7-character-belief --repo hherb/allrpcs
```

## Phase 2c7 implementation and review

- `CharacterBelief` contains only a canonical `Key` and a nonblank authored `Statement` bounded
  to 500 UTF-16 code units.
- Valid statements preserve whitespace, casing, line endings, and Unicode exactly. Record value
  equality uses both fields.
- `CharacterBeliefSet` permits an empty sequence, preserves authored order, copies caller input,
  exposes a runtime read-only collection, and rejects null entries and duplicate keys.
- Nineteen focused tests cover key and statement validation, exact text preservation, equality,
  empty and ordered collection behavior, input copying, null and duplicate rejection, runtime
  immutability, and the deliberately limited public surfaces.
- `Domain` retains no project references. `README.md` documents the contract and exclusions;
  `ROADMAP.md` records Phase 2c7 without ticking the broader character-model item.

Review confirmed that the value cannot be confused structurally with knowledge, uses the existing
canonical-text and authored-text validation contracts, owns its collection, preserves deterministic
order, and exposes no mutation path or belief mechanics. No correctness, architecture, or scope
issue remained after review.

## Validation and limitations

On 2026-09-01, the test-first focused run failed to compile because the belief types did not exist.
After implementation, the restricted environment compiled the focused suite but stalled when the
.NET test host attempted to open its local transport. The identical host-shell run passed all 19
focused tests.

The complete `./scripts/quality.sh` sequence then passed from the host shell on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- workflow lint, serial restore, formatting verification, and a zero-warning, zero-error build;
- all 139 Domain tests, 100 Application tests, and six architecture reference tests;
- navigation, healer-command, interaction-focus, and movement-boundary Godot smokes; and
- a bounded 60-frame native graphical regression launch with the Compatibility renderer on the
  Apple M3 Max.

No manual play acceptance is required for this engine-independent Domain slice. The native launch
and existing Godot smokes are regression evidence only. This slice makes no visual, input,
physics, scene, renderer, belief-correctness, or runtime-behavior claim.

Still explicitly excluded:

- world-truth or knowledge classification, supporting or contradicting evidence, confidence,
  provenance, attributed claims, discovery, reinterpretation, and public/private belief policy;
- relationships, goals, mutable character state, and a complete aggregate;
- visibility, perception filtering, authorization, capability checks, controller behavior,
  dialogue/model context, retrieval, provider instructions, and executable policy;
- scenario content, persistence, serialization, save/database schemas, migrations, Godot
  resources or nodes, UI, providers, runtime registration, and plugin surfaces; and
- releases, artifact distribution, credentials, signing, notarization, and repository visibility
  changes. Pull-request merge remains separately authorized.

## Exact next bounded slice

After the user reports that the Phase 2c7 pull request merged, take **Phase 2c8: character
relationship boundary**. This continues, but does not complete or tick, the broader authoritative
character-model roadmap item.

Acceptance boundary for Phase 2c8:

1. Add one immutable `CharacterRelationship` Domain value associating an existing related
   `CharacterId` with a nonblank authored summary bounded to 500 UTF-16 code units. Preserve the
   typed identifier and valid authored summary text exactly.
2. Add a structurally separate `CharacterRelationshipSet` that explicitly permits an empty
   sequence, preserves authored entry order, copies caller input, exposes no mutable collection,
   and rejects null entries and duplicate related-character identifiers using `CharacterId` value
   equality.
3. Add focused pure tests for identifier and summary validation, exact text preservation, value
   equality, empty and ordered collection behavior, input copying, null and duplicate rejection,
   runtime immutability, and the absence of scores, dimensions, reasons, history, visibility, or
   executable-policy surfaces.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader
   character-model roadmap item unticked until goals also exist and the listed state is reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c8:

- Do not add relationship scores, bounded dimensions, reasons, history, decay, mutation, event
  handling, or dialogue-driven change mechanics.
- Do not infer or enforce reciprocity, symmetry, direction inverses, self-relationship rules, or
  graph traversal without an owning character aggregate and an explicit later policy.
- Do not add public/private relationship policy, visibility, perception filtering,
  authorization, capability checks, action validation, dialogue/model context, retrieval, or
  provider instructions.
- Do not yet add goals, mutable character state, or a complete aggregate.
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
