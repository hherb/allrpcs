# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. Its bounded-camera movement greybox keeps
testable movement and framing calculations plus future authoritative rules in engine-independent
C# projects while Godot owns input mapping, physics presentation, camera behavior, collision,
and scene authoring.

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
recommended .NET analysis and compiler warnings treated as errors, runs the domain,
application, and architecture tests, then exercises both headless and 60-frame graphical Godot
launches. It first runs `actionlint` against every hosted workflow. It does not require provider
credentials or network access after restore. The project selects the Compatibility renderer
for graphical launches.

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
geometry also blocks the blue stream, the complete grey village block, and a representative
three-tree cluster. The path remains traversable. Every shape, color, obstacle, and collision
outline is an authored primitive for scale and camera testing only; none selects final art or
terrain. Close the titled **Allrpcs** window to stop the process. To open the project in the
editor instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` contains pure movement and camera-framing calculations and coordinates
  future domain use cases; it depends only on `Domain`.
- `src/Infrastructure` is the future adapter boundary and depends on `Application`.
- `game` is the Godot presentation project and depends on `Application`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Application.Tests` tests pure movement and camera-framing calculation without loading
  Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

Point-and-click movement, zoom or camera-pan input, controller mappings, companions, pathfinding,
navigation regions, providers, persistence, credentials, signing, installers, and distribution
configuration are not part of this greybox slice.

## Licence

Allrpcs is licensed under the GNU Affero General Public License version 3 only
(`AGPL-3.0-only`). See `LICENSE` for the complete terms.
