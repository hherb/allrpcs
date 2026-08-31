# Allrpcs Handover

## Current state

PR #9 merged Phase 1g into `origin/main` at merge commit `a9b6965` on 2026-08-31. The current
branch, `codex/phase-2a-stable-typed-identifiers`, is based directly on that verified merge.

Phase 2a, stable typed identifier primitives, is implemented on the current branch. It adds six
opaque, immutable Domain identifier types and one shared canonical-text contract, with focused
pure tests and documentation. It adds no entity state, scenario content, Godot integration,
provider, persistence, UI, or serialization-framework behavior. The corresponding Phase 2
roadmap item is ticked.

The private remote has open draft PR #10 for this branch. The reviewed Phase 2a implementation
and local evidence are commit `0750b4c`; this documentation update follows it. All five
credential-free jobs in its first pull-request run `33388975608` passed, including pure C#
quality, the Linux headless Godot smoke, and unsigned development builds on Linux x64, macOS
Universal 2, and Windows x64. Re-query the final branch head and checks after this documentation
commit rather than assuming the earlier commit's result applies to it. The pull request is not
authorized to merge. Public visibility, merging, releases, artifact distribution, credentials,
signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2a-stable-typed-identifiers --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2a-stable-typed-identifiers --repo hherb/allrpcs
```

## Phase 2a implementation

The slice remains within its original boundary:

- `src/Domain/Identifiers/CanonicalIdentifierText.cs` defines one explicit canonical contract
  shared by all stable identifiers. Text must be 1 to 64 characters, begin with a lowercase
  ASCII letter, and then contain only lowercase ASCII letters, digits, and single internal
  hyphens. It validates without silently normalizing input.
- `CharacterId`, `ObjectId`, `LocationId`, `EvidenceId`, `KnowledgeId`, and `WorldEventId` are
  sealed immutable records. Each validates at construction, exposes only canonical `Value`,
  round-trips through `Parse` and `ToString`, and retains record equality within its own type.
- Equal canonical text does not make different identifier types equal. The identifiers remain
  opaque keys and attach no mutable entity state or scenario meaning.
- `tests/Domain.Tests/Identifiers/DomainIdentifierTests.cs` covers shortest, representative, and
  maximum-length valid construction across all six types; empty, whitespace, uppercase,
  digit-leading, malformed-hyphen, underscore, non-ASCII, over-length, and null rejection;
  same-type and cross-type equality; and canonical text round trips.
- `README.md` documents the public text/equality contract and explicit boundary. `ROADMAP.md`
  records Phase 2a completion.

## Review and validation

### Automated evidence

On 2026-08-31, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed;
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers -m:1 /nr:false` built all seven
  projects with zero warnings and zero errors;
- the focused Domain test project passed all 18 tests, including the existing assembly smoke and
  17 identifier cases; and
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, all 18 Domain tests, all 100 Application tests, all six
  architecture reference tests, all four Godot smokes, and the bounded 60-frame native graphical
  launch.

The navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map iteration 2
and found the bridge route while rejecting representative obstacle targets. The healer-command,
interaction-focus, and movement-boundary smokes all repeated their complete established Phase 1
acceptance paths without a regression.

The first hosted pull-request run `33388975608` passed pure C# quality and tests, the Linux
headless Godot smoke, and unsigned development builds on Linux x64, macOS Universal 2, and
Windows x64. Those hosted outputs remain unsigned development evidence, not releases or
distribution artifacts. The final documentation commit will create a new branch head, so its
checks must be queried separately.

The focused .NET test execution ran from the host shell because the restricted environment
denied the test runner's loopback socket. The focused format and build ran in the restricted
environment. No sandbox failure is being reported as a product result.

### Native evidence

No manual play acceptance is required for this engine-independent Domain slice. The complete
quality sequence launched the current native scene with Godot's Compatibility renderer on the
Apple M3 Max and exited cleanly after 60 frames. The existing Godot smokes passed as regression
checks only. This slice makes no visual, input, physics, scene, renderer, or interaction behavior
claim.

## Review findings

The reviewed design keeps the contract deliberately narrow. All six identifiers delegate
validation to one pure helper, expose no public setter, and use distinct sealed record types so
same-text values cannot be accidentally treated as the same domain identity. The validation
loop is culture-independent and accepts ASCII explicitly; it does not allocate normalized copies
or infer semantics from the identifier text.

Construction and parsing use the same validation path. `ToString()` returns the stored canonical
value, making round trips deterministic without introducing a serialization dependency. The
types remain in `Domain`, whose project reference list is still empty.

No correctness, security, architecture, or scope issue remained after review. Repetition across
the six small records is intentional: it preserves straightforward type-specific APIs without a
public inheritance hierarchy or reflection-based factory.

## Limitations and exclusions

- Phase 2a does not allocate identifiers, guarantee uniqueness, register entities, resolve
  references, define lifecycle, or attach mutable character, object, location, evidence,
  knowledge, or event state.
- Canonical text is a stable opaque key, not a display label, localization surface, path, UUID,
  database key policy, or proof that referenced content exists.
- The public `Parse` methods validate canonical text but do not provide framework-specific
  converters, `TryParse`, JSON behavior, save schemas, database mapping, or migration policy.
- No temporary Phase 1 marker index or authored scene order was promoted into durable identity.
- No controller type, character/world model, perception, discovery, inventory, event-log
  storage, world time, scenario content, dialogue, LLM call, provider, persistence, UI, Godot,
  credential, packaging, signing, release, or distribution path was added.
- Existing Phase 1 native behavior and hosted cross-platform builds are regression surfaces, not
  new Phase 2a behavior claims. No manual or cross-platform identifier-specific acceptance was
  required or performed.

## Exact next bounded slice

After the user reports that the Phase 2a pull request merged, take **Phase 2b: controller type
primitives**.

Acceptance boundary for Phase 2b:

1. Add one small engine-independent Domain representation for the four settled controller types:
   human, LLM, deterministic test, and dormant, with explicit stable canonical text.
2. Add focused pure tests proving the complete defined set, equality and canonical text round
   trips, and fail-closed rejection of unknown values.
3. Preserve `Domain -> []` and avoid Godot, provider, persistence, UI, serialization-framework,
   or platform dependencies.
4. Update `README.md`, `ROADMAP.md`, and this file, then commit only reviewed slice paths.

Explicit exclusions for Phase 2b:

- Do not add controller instances, provider/model selection, prompts, credentials, runtime
  orchestration, autonomous planning, deterministic character behavior, dormant disease rules,
  character models, world objects, perception, discovery, inventory, event storage, or UI.
- Do not attach controller type to a character before the authoritative character model exists.
- Do not add extensible/plugin controller registration or interpret the stable type text as a
  provider identifier.
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
