# Allrpcs Handover

## Current state

PR #12 merged Phase 2c1 into `origin/main` at merge commit `e810427` on 2026-08-31.
All five credential-free checks passed at the merged branch head in pull-request run
`33392650343`. The current branch, `codex/phase-2c2-capability-limitations`, is based directly on
that verified merge.

Phase 2c2, the character capability and limitation boundary, is implemented, reviewed, and
locally validated on the current branch. It adds structurally distinct immutable Domain values
for authored capabilities and limitations plus ordered, duplicate-free collection boundaries.
The broader character-model roadmap item remains unticked because condition, inventory,
knowledge, beliefs, relationships, goals, and controller assignment do not yet exist.

No pull request exists for the current branch yet. The current authorization is to commit the
reviewed slice, push this branch, and open a draft pull request. It does not authorize merge,
public visibility, releases, artifact distribution, credentials, signing, or notarization.

At the start of the next session, verify current state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view codex/phase-2c2-capability-limitations --repo hherb/allrpcs \
  --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks codex/phase-2c2-capability-limitations --repo hherb/allrpcs
```

## Phase 2c2 implementation

The slice remains within its original boundary:

- `CharacterCapability` and `CharacterLimitation` are unrelated sealed record types. Each
  retains a canonical key and authored description and compares by both values within its own
  type boundary.
- Keys reuse `CanonicalIdentifierText`: 1 to 64 characters, beginning with a lowercase ASCII
  letter and then containing only lowercase ASCII letters, digits, and single internal hyphens.
  Invalid input is rejected rather than normalized.
- Descriptions must contain at least one non-whitespace character, are bounded to 500 UTF-16 code
  units, and retain valid content exactly without trimming, case folding, Unicode normalization,
  or line-ending rewriting.
- `CharacterCapabilitySet` and `CharacterLimitationSet` explicitly accept an empty sequence to
  mean that no entries are authored. They preserve authored entry order, copy caller input, and
  reject null entries and duplicate canonical keys using ordinal comparison.
- `AuthoredCharacterEntrySet` centralizes only the private ordered-copy and unique-key invariant;
  it does not expose a registration, metadata, or extensibility surface.
- Four focused test files prove value equality, structural distinction, both key and description
  length edges, exact description preservation, invalid and null rejection, empty-set behavior,
  stable ordering, input copying, duplicate-key rejection, and the absence of visibility or
  executable-policy properties.
- `README.md` documents the public contract and exclusions. `ROADMAP.md` records Phase 2c2
  progress while deliberately leaving the broader character-model item unticked.

## Review and validation

### Automated evidence

On 2026-09-01, the current implementation produced this local evidence on Apple Silicon macOS
with .NET SDK 8.0.130 and Godot 4.6.3.stable.mono.official:

- the test-first focused Domain run failed to compile because the four new public types did not
  exist, then passed all 81 Domain test cases after implementation;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed;
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers -m:1 /nr:false` built all seven
  projects with zero warnings and zero errors; and
- the final complete `./scripts/quality.sh` passed action linting, restore, formatting
  verification, the zero-warning build, all 81 Domain tests, all 100 Application tests, all six
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
checks only. This slice makes no visual, input, physics, scene, renderer, controller-behavior,
capability-execution, or limitation-enforcement claim.

## Review findings

The reviewed value boundary reuses the established canonical-text validator instead of creating
a second key grammar or normalizing authored input. Capability and limitation equality cannot
cross their unrelated record types even when key and description text match.

The collection boundaries enumerate caller input once into private read-only copies, retain the
authored ordering needed for deterministic later composition, and compare duplicate keys with
the same ordinal semantics implied by canonical ASCII text. Null and duplicate entries fail
closed. The public entry surface contains only `Key` and `Description`; capability state cannot
grant permission or execute an action, and limitation state cannot become a visibility,
authorization, or provider-prompt rule. Domain retains no project references. No correctness,
security, architecture, or scope issue remained after review.

## Limitations and exclusions

- Phase 2c2 does not define a complete character aggregate or mutable character state.
- It does not add condition, inventory, knowledge, beliefs, relationships, goals, controller
  instances, controller assignment, or controller behavior.
- A capability is authored descriptive state only, not permission, an executable action, a Godot
  input, an affordance, action validation, or proof that an attempted action will succeed.
- A limitation is authored descriptive state only, not public/private classification,
  authorization policy, enforcement logic, a provider prompt instruction, or proof of inability.
- Collection order is authored order, not priority, visibility, execution order, or evaluation
  precedence. Duplicate rejection applies only within each capability or limitation set.
- Length is measured by .NET `string.Length` in UTF-16 code units. No grapheme-counting,
  localization policy, content moderation, markup, or rich-text interpretation was added.
- No scenario character, filtered perception, dialogue/model context, framework-specific
  converter, JSON behavior, save schema, database mapping, migration, Godot resource, node, UI,
  provider, orchestration, action registry, or plugin surface was added.
- Existing native behavior and hosted cross-platform builds are regression surfaces, not new
  Phase 2c2 behavior claims. No manual or cross-platform capability-specific acceptance was
  required or performed.

## Exact next bounded slice

After the user reports that the Phase 2c2 pull request merged, take **Phase 2c3: character
controller-assignment boundary**. This continues, but does not complete or tick, the broader
authoritative character-model roadmap item.

Acceptance boundary for Phase 2c3:

1. Add a small immutable, engine-independent Domain value that pairs an existing stable
   `CharacterId` with one existing closed `ControllerType`.
2. Define the assignment as current authored/authoritative association only; retain both values
   exactly and add no controller instance, lifecycle, configuration, or behavior.
3. Add focused pure tests for value equality, all four controller types, null rejection,
   character distinction, and provider/model/behavior non-assumptions.
4. Preserve `Domain -> []`; update `README.md` and this file, but leave the broader character-model
   roadmap item unticked until all of its listed state is represented and reviewed.
5. Commit only reviewed slice paths after the current validation sequence passes.

Explicit exclusions for Phase 2c3:

- Do not yet add condition, inventory, knowledge, beliefs, relationships, goals, mutable
  character state, or a complete character aggregate.
- Do not create controller instances, provider/model configuration, prompts, tools, scheduling,
  switching policy, dormant transitions, player input routing, deterministic behavior, or runtime
  orchestration.
- Do not define scenario characters, action validation, perception filtering, dialogue/model
  context, authorization policy, or persistence/serialization behavior.
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
