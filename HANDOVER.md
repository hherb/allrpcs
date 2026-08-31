# Allrpcs Handover

## Current state

PR #11 merged Phase 2b into `origin/main` at merge commit `8da279e` on 2026-08-31.
The current branch, `codex/phase-2c1-character-identity`, is based directly on that verified
merge.

Phase 2c1, character identity and background boundary, is implemented, reviewed, and locally
validated on the current branch. It adds immutable engine-independent Domain values for a
character's stable typed identity and explicitly separate public and private authored background
text. The broader character-model roadmap item remains unticked because capabilities,
limitations, condition, inventory, knowledge, beliefs, relationships, goals, and controller
assignment do not yet exist.

The user authorized committing, pushing, and opening a draft pull request after review. At this
pre-publication handoff point, no new branch commit or pull request exists yet. Public visibility,
merging, releases, artifact distribution, credentials, signing, and notarization remain
unauthorized.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c1-character-identity --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c1-character-identity --repo hherb/allrpcs
```

## Phase 2c1 implementation

The slice remains within its original boundary:

- `src/Domain/Characters/CharacterIdentity.cs` pairs an existing stable `CharacterId` with an
  authored display name. It retains both values and compares by value.
- A display name must contain at least one non-whitespace character and is bounded to 80 UTF-16
  code units. Valid names are retained exactly: no trimming, case folding, Unicode normalization,
  or other rewriting occurs.
- `PublicCharacterBackground` and `PrivateCharacterBackground` are distinct immutable types, so a
  future filtered view can carry the public value without implicitly carrying private text.
- Both background types explicitly accept the empty string to mean no authored background,
  reject null and values over 4,000 UTF-16 code units, retain all valid content exactly, and
  compare by value within their own boundary.
- `AuthoredCharacterText` centralizes bounded validation without exposing a general-purpose
  metadata or registration surface.
- `tests/Domain.Tests/Characters/CharacterIdentityTests.cs` proves identity retention, value
  equality, exact-name preservation, the nonblank rule, both length edges, and null rejection.
- `tests/Domain.Tests/Characters/CharacterBackgroundTests.cs` proves exact authored-text
  preservation, the explicit empty policy, structural type distinction, value equality, the
  shared length edge, oversize rejection, and null rejection.
- `README.md` documents the public contract and exclusions. `ROADMAP.md` records Phase 2c1
  progress while deliberately leaving the broader character-model item unticked.

## Review and validation

### Automated evidence

On 2026-08-31, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- the test-first focused Domain run initially failed because the character namespace did not
  exist, then passed all 44 Domain tests after implementation;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed;
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers -m:1 /nr:false` built all seven
  projects with zero warnings and zero errors; and
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, all 44 Domain tests, all 100 Application tests, all six
  architecture reference tests, all four Godot smokes, and the bounded 60-frame native graphical
  launch.

The navigation smoke synchronized the 98-vertex, 66-polygon region at navigation map iteration 2
and found the bridge route while rejecting representative obstacle targets. The healer-command,
interaction-focus, and movement-boundary smokes repeated their complete established acceptance
paths without a regression.

The focused test host first compiled inside the restricted environment, then could not bind its
loopback socket. The identical focused test and complete quality sequence passed from the host
shell. Formatting and the focused zero-warning build passed in the restricted environment. No
sandbox failure is being reported as a product result.

### Native evidence

No manual play acceptance is required for this engine-independent Domain slice. The complete
quality sequence launched the current native scene with Godot's Compatibility renderer on the
Apple M3 Max and exited cleanly after 60 frames. The existing Godot smokes passed as regression
checks only. This slice makes no visual, input, physics, scene, renderer, controller-behavior, or
interaction claim.

## Review findings

The reviewed boundary keeps public and private authored backgrounds in unrelated sealed record
types rather than a combined transport object. That makes accidental carriage of private text
less likely when a future filtered view needs only public background. Their same-sized bounds are
intentional, but their equality domains remain separate.

The identity composes the already-reviewed `CharacterId` rather than introducing a second key
format. Validation returns the original strings, so identity and background values preserve
authored whitespace, casing, line endings, and Unicode content. Blank display names fail closed,
while an empty background has a documented meaning. Domain retains no project references. No
correctness, security, architecture, or scope issue remained after review.

## Limitations and exclusions

- Phase 2c1 does not define a complete character aggregate or mutable character state.
- It does not add capabilities, limitations, condition, inventory, knowledge, beliefs,
  relationships, goals, controller instances, controller assignment, or controller behavior.
- Public/private background separation is a Domain type boundary only. No filtered perception,
  authorization policy, dialogue/model context, scenario character, or provider prompt exists.
- Private background is not hidden chain-of-thought, credentials, arbitrary metadata, or a
  provider instruction channel.
- Length is measured by .NET `string.Length` in UTF-16 code units. No grapheme-counting,
  localization policy, content moderation, markup, or rich-text interpretation was added.
- No framework-specific converters, JSON behavior, save schema, database mapping, migration,
  Godot resource, node, UI, provider, orchestration, registration, or plugin surface was added.
- Existing native behavior and hosted cross-platform builds are regression surfaces, not new
  Phase 2c1 behavior claims. No manual or cross-platform character-specific acceptance was
  required or performed.

## Exact next bounded slice

After the user reports that the Phase 2c1 pull request merged, take **Phase 2c2: character
capability and limitation boundary**. This continues, but does not complete or tick, the broader
authoritative character-model roadmap item.

Acceptance boundary for Phase 2c2:

1. Add small immutable, engine-independent Domain values that keep authored character
   capabilities structurally distinct from authored limitations.
2. Give each entry a stable canonical key and bounded authored description, reuse the existing
   identifier-text rules rather than inventing normalization, and define an explicit empty-set
   policy.
3. Add focused pure tests for value equality, key and description bounds, exact authored-text
   preservation, stable collection ordering, duplicate-key rejection, and public/private or
   executable-policy non-assumptions.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader character-model
   roadmap item unticked until all of its listed state is represented and reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c2:

- Do not yet add condition, inventory, knowledge, beliefs, relationships, goals, controller
  assignment, mutable character state, or a complete character aggregate.
- Do not treat a capability as permission, an executable action, a Godot input, an affordance, or
  proof that an attempted action will succeed. Do not make limitations provider prompt rules.
- Do not define action validation, perception filtering, dialogue/model context, scenario
  characters, authorization policy, or persistence/serialization behavior.
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
