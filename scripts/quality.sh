#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

cd "$repository_root"

./scripts/validate-workflows.sh
python3 scripts/validate-garden-ground-runtime.py --historical-quality
python3 scripts/validate-direction-a-import-proof.py
python3 scripts/validate-square-ground-runtime.py --historical-garden
python3 scripts/validate-sickhouse-ground-runtime.py --historical-square
python3 scripts/validate-wagon-ground-runtime.py --historical-sickhouse
python3 scripts/validate-shrine-ground-runtime.py --historical-wagon
python3 scripts/validate-laundry-ground-runtime.py --historical-shrine
python3 scripts/validate-laundry-ground-runtime.py --check-rejections
dotnet restore Allrpcs.sln --disable-parallel -m:1 /nr:false
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
ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK=1 godot --headless --path game --quit-after 30
godot --path game --quit-after 60
