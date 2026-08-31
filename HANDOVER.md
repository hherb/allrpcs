# Allrpcs Handover

## Current state

PR #10 merged Phase 2a into `origin/main` at merge commit `1a29cb6` on 2026-08-31. The
current branch, `codex/phase-2b-controller-types`, is based directly on that verified merge.

Phase 2b, controller type primitives, is implemented and locally validated on the current
branch. It defines the four settled controller categories as a closed Domain set with stable
canonical text and adds focused pure tests and documentation. The corresponding Phase 2 roadmap
item is ticked. It adds no controller instances or runtime behavior, character model or
assignment, provider/model selection, Godot integration, serialization framework, persistence,
or UI.

No pull request exists for the branch yet. Commit, push, and draft-pull-request creation are
authorized for this slice. Merging, public visibility, releases, artifact distribution,
credentials, signing, and notarization remain unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2b-controller-types --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2b-controller-types --repo hherb/allrpcs
```

## Phase 2b implementation

The slice remains within its original boundary:

- `src/Domain/Controllers/ControllerType.cs` defines exactly four singleton values: `Human`,
  `Llm`, `DeterministicTest`, and `Dormant`.
- Their stable canonical text is `human`, `llm`, `deterministic-test`, and `dormant`.
  `ControllerType.All` exposes the complete set in that stable order.
- `Value` and `ToString()` expose canonical text. `Parse` accepts only the four exact values and
  returns the corresponding defined instance; null and unknown text fail closed.
- Equality means the same defined controller type. Construction is private, so ordinary callers
  cannot register provider-specific or other ad hoc controller categories.
- `tests/Domain.Tests/Controllers/ControllerTypeTests.cs` proves the complete ordered set,
  canonical round trips to the defined instances, equality, and rejection of null, case changes,
  whitespace, aliases, provider-shaped values, and other unknown text.
- `README.md` documents the public contract and exclusions. `ROADMAP.md` records Phase 2b
  completion.

## Review and validation

### Automated evidence

On 2026-08-31, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- the test-first focused Domain run initially failed because the controller namespace did not
  exist, then passed all 28 Domain tests after the implementation;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed;
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers -m:1 /nr:false` built all seven
  projects with zero warnings and zero errors; and
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, all 28 Domain tests, all 100 Application tests, all six
  architecture reference tests, all four Godot smokes, and the bounded 60-frame native graphical
  launch.

The navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map iteration 2
and found the bridge route while rejecting representative obstacle targets. The healer-command,
interaction-focus, and movement-boundary smokes repeated their complete established acceptance
paths without a regression.

The focused and complete test executions ran from the host shell because the restricted
environment denied the test runner's loopback socket. Formatting and the focused zero-warning
build passed in the restricted environment. No sandbox failure is being reported as a product
result.

### Native evidence

No manual play acceptance is required for this engine-independent Domain slice. The complete
quality sequence launched the current native scene with Godot's Compatibility renderer on the
Apple M3 Max and exited cleanly after 60 frames. The existing Godot smokes passed as regression
checks only. This slice makes no visual, input, physics, scene, renderer, controller-behavior, or
interaction claim.

## Review findings

The reviewed representation is deliberately a closed class rather than an enum or open string
wrapper. That keeps canonical persistence-facing text explicit while preventing ordinary callers
from constructing provider-specific or misspelled types. Parsing returns the defined instances,
so equality and round trips do not depend on culture, normalization, reflection, or registration.

The public `All` list is backed by a read-only collection and is declared once beside the four
instances. The type lives in `Domain`, whose project reference list remains empty. No correctness,
security, architecture, or scope issue remained after review.

## Limitations and exclusions

- A controller type is only a category. It does not represent a controller instance, runtime
  lifecycle, provider, model, prompt, credentials, availability, or behavior.
- Phase 2b does not attach a controller type to a character or define character identity,
  background, capabilities, limitations, condition, inventory, knowledge, beliefs,
  relationships, or goals.
- `deterministic-test` does not yet define deterministic character behavior, and `dormant` does
  not define disease progression, sleeping-character behavior, or restoration of another
  controller.
- `Parse` is an explicit Domain parser. No framework-specific converters, `TryParse`, JSON
  behavior, save schema, database mapping, or migration policy was added.
- No controller registration or plugin system, provider/model selection, orchestration, LLM call,
  scenario content, perception, discovery, inventory, event storage, UI, Godot, credential,
  packaging, signing, release, or distribution path was added.
- Existing native behavior and hosted cross-platform builds are regression surfaces, not new
  Phase 2b behavior claims. No manual or cross-platform controller-specific acceptance was
  required or performed.

## Exact next bounded slice

After the user reports that the Phase 2b pull request merged, take **Phase 2c1: character identity
and background boundary**. This begins, but does not complete or tick, the broader authoritative
character-model roadmap item.

Acceptance boundary for Phase 2c1:

1. Add small immutable, engine-independent Domain primitives for a character's stable identity
   and explicitly separated public and private authored background text.
2. Make the public/private boundary structurally visible in the types so a future filtered view
   can expose public background without implicitly carrying private background.
3. Add focused pure tests for identity retention, value equality, explicit empty-text policy,
   bounded text validation, and preservation of authored text without normalization.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader character-model
   roadmap item unticked until all of its listed state is represented and reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c1:

- Do not yet add capabilities, limitations, condition, inventory, knowledge, beliefs,
  relationships, goals, controller assignment, mutable character state, or a complete character
  aggregate.
- Do not define perception filtering, dialogue/model context, scenario characters, provider
  prompts, authorization policy, or persistence/serialization behavior.
- Do not use private background as hidden chain-of-thought, credentials, arbitrary metadata, or
  a provider instruction channel.
- Do not add Godot nodes/resources, UI, database/save schemas, migrations, providers, runtime
  orchestration, controller behavior, or extensible registration.
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
