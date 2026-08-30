#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

cd "$repository_root"

./scripts/validate-workflows.sh
dotnet restore Allrpcs.sln --disable-parallel -m:1 /nr:false
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj --no-build
./scripts/smoke-godot.sh
godot --path game --quit-after 60
