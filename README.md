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
4.6.3.stable.mono.official.

## Build and test

From the repository root:

```sh
dotnet restore Allrpcs.sln
dotnet build Allrpcs.sln --no-restore
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
./scripts/smoke-godot.sh
```

The smoke script starts the main scene headlessly, runs one frame, and exits. It does not
require provider credentials or network access after restore. The project selects the
Compatibility renderer for graphical launches.

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

Provider adapters, persistence, gameplay, credentials, and distribution configuration are not
part of this foundation slice.
