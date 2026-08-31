# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. Its bounded-camera movement, navigation,
deterministic healer-command, and interaction-focus greybox keeps testable movement, framing,
navigation-request, follow-target, command-selection, and focus-selection rules plus future
authoritative rules in engine-independent C# projects while Godot owns input mapping, physics
presentation, camera behavior, collision, navigation topology, and scene authoring.

## Prerequisites

- A 64-bit .NET 8 SDK.
- The Godot 4.6.3 .NET editor, not the standard editor without C# support.
- `actionlint` 1.7.12 or newer for local GitHub Actions validation.

Both executables must be available to the shell as `dotnet` and `godot`. Verify them before
building:

```sh
dotnet --info
godot --version
```

The verified local versions for this foundation are .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official. Root `global.json` keeps `dotnet` in the .NET 8.0.1xx feature band
and permits only the latest installed patch in that band. The Godot SDK version is pinned in
`game/Allrpcs.Game.csproj`. To update the toolchain, change these pins and the prerequisite
versions together, then rerun the complete local quality sequence before committing.

## Local quality checks

From the repository root:

```sh
./scripts/quality.sh
```

The sequence restores packages serially, rejects C# formatting drift, builds with current
recommended .NET analysis and compiler warnings treated as errors, runs the domain, application,
and architecture tests, then exercises both headless and 60-frame graphical Godot launches. The
headless launches wait for the navigation map to synchronize, prove a route through the bridge,
reject representative stream, tree, and village targets, and exercise healer follow, wait,
move completion, invalid and excluded command rejection, separation, and cross-bridge return to
follow. A separate interaction-focus launch proves nearest selection, authored-order
tie-breaking, focus changes, out-of-range clearing, and hidden and disabled exclusions. The
movement-boundary launch uses the existing Seeker body and authored collision shapes to prove all
four outer borders plus representative stream, village, and tree collisions, then proves the
bridge in both directions and open lower terrain remain clear. The sequence first runs
`actionlint` against every hosted workflow. It does not require provider credentials or network
access after restore. The project selects the Compatibility renderer for graphical launches.

To verify formatting alone without changing files, run:

```sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
```

The architecture tests inspect authored project files without loading Godot. They enforce the
project-reference directions listed below.

## Credential-free hosted checks

`.github/workflows/ci.yml` configures pull-request and `main` checks on explicit stable runner
images:

- Pure formatting, zero-warning compilation, domain tests, Application movement and camera
  framing tests, and architecture tests run on `ubuntu-24.04` without installing or launching
  Godot.
- The headless main-scene smoke runs separately on `ubuntu-24.04` with Godot 4.6.3 .NET.
- Unsigned debug exports run natively on `ubuntu-24.04`, Apple Silicon `macos-15`, and
  `windows-2025`. Their targets are Linux x64, macOS Universal 2, and Windows x64.

The workflow grants only `contents: read`, disables checkout credential persistence, defines no
provider inputs or secrets, and uses no dependency cache. Uploaded evidence is named
`UNSIGNED-DEVELOPMENT-*`, retained for seven days, and includes a plain-text limitations
manifest. The macOS job rejects a bundle-level signature and the Windows job rejects an
Authenticode signature before upload. These debug outputs are neither signed releases nor
installers and are not approved for distribution.

Every external action is pinned to a full commit SHA with its verified release tag in a comment:
`actions/checkout` v7.0.1, `actions/setup-dotnet` v6.0.0,
`chickensoft-games/setup-godot` v2.4.2, and `actions/upload-artifact` v7.0.1. Before updating
them, recheck the [supported runner labels][github-runners], action release notes, and Godot
compatibility, update the SHA and tag comment together, then run the complete local quality
sequence.

The workflow was first verified on GitHub-hosted runners in private-repository push run
[`33310947250`][push-run] and pull-request run [`33311261187`][pull-request-run]. The exact
resolved images were Ubuntu 24.04 `20260823.283.1`, macOS 15 arm64 `20260727.0256.1`, and
Windows 2025 with Visual Studio 2026 `20260824.214.3`. All quality, headless smoke, native
export, unsigned-signature rejection, and artifact-upload steps passed in both event types. See
`HANDOVER.md` for job, artifact, pull-request, and limitation evidence. Hosted debug exports
remain build evidence only, not releases.

[github-runners]: https://docs.github.com/en/actions/reference/runners/github-hosted-runners
[push-run]: https://github.com/hherb/allrpcs/actions/runs/33310947250
[pull-request-run]: https://github.com/hherb/allrpcs/actions/runs/33311261187

## Unsigned development exports

With Godot 4.6.3 .NET export templates installed, reproduce one CI export locally from the
repository root:

```sh
./scripts/export-development.sh linux-x64
./scripts/export-development.sh macos-universal
./scripts/export-development.sh windows-x64
```

Each command writes only under `artifacts/unsigned-development/`, which Git ignores. The
committed `game/export_presets.cfg` explicitly disables macOS signing and notarization and
Windows code signing. Linux has no signing step. These commands create development-build
evidence only; they do not package, sign, notarize, publish, or distribute the game.

## Launch the keyboard greybox

```sh
godot --path game
```

Use W, A, S, and D to move the gold placeholder Seeker around the 1600-by-900 test region. The
Seeker's named default speed is 240 pixels per second. Diagonal input is normalized so it does
not move faster than a cardinal direction. The camera follows with a named smoothing speed of
6 and world limits matching the region, so ordinary movement cannot reveal space beyond it.
Its named minimum, default, and maximum zoom tuning values are 0.75, 1.0, and 2.0. Zoom and
camera-pan input are intentionally not mapped yet.

Four thin collision walls keep the Seeker inside the temporary outer border. Simple collision
geometry also blocks the blue stream outside the obvious brown temporary bridge, the complete
grey village block, and a representative three-tree cluster. The bridge is traversable in both
directions and the rest of the path remains open.

The credential-free movement-boundary smoke exercises those authored shapes with deterministic,
test-only Godot physics motions from the existing Seeker body. It verifies collision sweeps and
clear paths without synthesizing keyboard input or claiming final input UX coverage:

```sh
ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path game --quit-after 30
```

A deterministically authored `NavigationRegion2D` describes future companion-pathfinding space
inside the same 1600-by-900 region. Its conservative clearance excludes the outer walls, stream
outside the bridge, village block, and tree cluster. The obvious cyan placeholder healer starts
at the named `HealerStart` position on the Seeker's side of the stream. It follows the Seeker over
that navigation surface at a named 180-pixel-per-second speed only while outside a validated
72-pixel stop distance. Its circular collision matches its 36-pixel visible footprint, so the
healer stops without overlapping the Seeker and can route through the bridge after separation.

The small **TEMP GREYBOX** panel shows the current healer mode. Press X to toggle between follow
and wait. Press M to move the healer to the obvious pink ring; the healer uses the existing
navigation surface, stops within a validated 12-pixel distance, then changes to wait. Press X
again to resume follow. Off-world and obstacle-excluded destinations are rejected without
changing the active command. These two keys, the ring, and the status panel are temporary
behavior-test controls, not final command or dialogue UX.

Two nearby temporary markers exercise interaction focus without performing an interaction. A
bright yellow **FOCUS** ring and the top-left status identify only the nearest visible, enabled
marker inside the inclusive 112-pixel range. Exact distance ties resolve by authored scene order,
so focus is stable at the Seeker's initial midpoint. Moving nearer the other marker changes focus;
moving outside both ranges clears it. A hidden marker and an obvious excluded marker exercise the
fail-closed presentation paths and can never receive the indicator. These markers, labels, range,
ordering, and indicator are temporary greybox behavior evidence, not Phase 2 objects, stable
identifiers, perception state, or interaction UX.

Every shape, color, obstacle, collision outline, navigation polygon, target marker, and status
element is an authored primitive for scale and behavior testing only; none selects final art or
terrain. Close the titled **Allrpcs** window to stop the process. To open the project in the editor
instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` contains pure movement, camera-framing, navigation-request validation,
  healer follow-target selection, healer command selection, and bounded interaction-focus
  selection and coordinates future domain use cases; it depends only on `Domain`.
- `src/Infrastructure` is the future adapter boundary and depends on `Application`.
- `game` is the Godot presentation project and depends on `Application`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Application.Tests` tests pure movement, camera-framing, navigation-request, healer
  follow-target, healer command, and interaction-focus behavior without loading Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

The Godot scene also owns credential-free engine checks for navigation topology, healer-command
movement, interaction-focus presentation, and Seeker collision boundaries. These checks load the
authored scene; they do not move Godot physics or presentation policy into `Application`.

## Stable domain identifiers

`Allrpcs.Domain.Identifiers` defines distinct immutable identifiers for characters, world
objects, locations, evidence, knowledge, and authoritative world events. They share one
canonical-text contract: 1 to 64 characters, beginning with a lowercase ASCII letter and then
containing only lowercase ASCII letters, digits, and single internal hyphens. For example,
`healer`, `village-square-2`, and `patient-stabilized` are canonical. Inputs are validated rather
than normalized, so case, whitespace, underscores, repeated hyphens, leading or trailing hyphens,
and non-ASCII text are rejected.

Each identifier's `Value` and `ToString()` return the same canonical text, and its `Parse` method
round-trips that text. Equality requires both the identifier type and its canonical value, so a
character and an object with the same text remain different identifiers. These types are opaque
keys only: they do not contain mutable entity state, scenario content, Godot types, provider or
persistence details, or serialization-framework behavior.

## Controller types

`Allrpcs.Domain.Controllers.ControllerType` defines exactly four controller types: `human`,
`llm`, `deterministic-test`, and `dormant`. `All` exposes the complete set in that stable order,
while `Value` and `ToString()` expose each type's canonical text. `Parse` accepts only those exact
values and returns the corresponding defined instance; case changes, whitespace, provider names,
and other unknown text are rejected rather than normalized or registered.

These values describe controller categories only. They do not create controller instances,
select providers or models, define prompts or behavior, attach a controller to a character, or
introduce runtime orchestration, serialization-framework, persistence, Godot, or UI behavior.

## Character identity, backgrounds, capabilities, limitations, and controller assignment

`Allrpcs.Domain.Characters.CharacterIdentity` pairs a stable `CharacterId` with an authored
display name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16
code units as measured by .NET `string.Length`, and are retained exactly rather than trimmed,
case-folded, or otherwise normalized. The immutable value compares by both identifier and display
name.

`PublicCharacterBackground` and `PrivateCharacterBackground` are separate immutable value types.
Their structural separation lets future filtered views carry a public background without also
carrying private text. Both retain authored text exactly, explicitly permit an empty string when
no background is authored, reject null, and are bounded to 4,000 UTF-16 code units. Private
background is ordinary authored character state: it is not hidden chain-of-thought, a credential
container, arbitrary metadata, or a provider instruction channel.

`CharacterCapability` and `CharacterLimitation` are structurally distinct immutable authored
values. Each has a stable key using the existing 1-to-64-character canonical identifier-text
contract and a nonblank description bounded to 500 UTF-16 code units. Valid descriptions retain
authored whitespace, casing, line endings, and Unicode exactly. `CharacterCapabilitySet` and
`CharacterLimitationSet` explicitly permit an empty sequence, preserve authored entry order,
copy caller input, and reject duplicate keys within their own boundary.

A capability describes authored character state only. It is not permission, an executable
action, an input, an affordance, or proof that an attempt will succeed. A limitation is not a
public/private classification, authorization rule, action validator, or provider prompt rule.

`CharacterControllerAssignment` associates one stable `CharacterId` with exactly one existing
closed `ControllerType`. The immutable value retains both objects exactly and compares by their
values. It records only the character's current authored controller-type association: it does not
create a controller instance, configure a provider or model, define prompts or behavior, route
input, or introduce runtime orchestration or switching policy.

This boundary does not yet define a complete character aggregate, condition, inventory,
knowledge, beliefs, relationships, goals, mutable state, perception filtering, dialogue context,
persistence, serialization, Godot, or UI behavior.

Point-and-click movement, zoom or camera-pan input, controller mappings beyond the temporary
healer-command keys, interaction actions or outcomes, Phase 2 objects and perception models,
natural-language commands, final command or interaction UI, LLM-controlled companions, providers,
persistence, credentials, signing, installers, and distribution configuration are not part of
this greybox slice.

## Licence

Allrpcs is licensed under the GNU Affero General Public License version 3 only
(`AGPL-3.0-only`). See `LICENSE` for the complete terms.
