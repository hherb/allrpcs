#!/bin/sh
set -eu
cd /private/tmp/allrpcs-laundry-quality-01a10b90
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --no-build
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj --no-build
dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj --no-build
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
./scripts/smoke-godot.sh
ALLRPCS_RUNTIME_GARDEN_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SQUARE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SICKHOUSE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_WAGON_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
godot --path game --quit-after 60
