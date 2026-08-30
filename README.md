# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. This foundation keeps authoritative rules
in engine-independent C# projects and uses Godot only at the presentation boundary.

## Prerequisites

- A 64-bit .NET 8 SDK.
- The Godot 4.6.3 .NET editor, not the standard editor without C# support.

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
recommended .NET analysis and compiler warnings treated as errors, runs the domain and
architecture tests, then exercises both headless and 60-frame graphical Godot launches. It
does not require provider credentials or network access after restore. The project selects the
Compatibility renderer for graphical launches.

To verify formatting alone without changing files, run:

```sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
```

The architecture tests inspect authored project files without loading Godot. They enforce the
project-reference directions listed below.

## Launch the empty application

```sh
godot --path game
```

Close the titled **Allrpcs** window to stop the process. To open the project in the editor
instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` coordinates domain use cases and depends only on `Domain`.
- `src/Infrastructure` is the future adapter boundary and depends on `Application`.
- `game` is the Godot presentation project and depends on `Application`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

Provider adapters, persistence, gameplay, credentials, and distribution configuration are not
part of this foundation slice.
