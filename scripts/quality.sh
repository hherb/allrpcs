#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

cd "$repository_root"

./scripts/validate-workflows.sh
python3 art-direction/production-proof/corrected/tools/validate.py
python3 art-direction/production-proof/corrected/tools/check_opening_junction_feasibility.py
python3 art-direction/production-proof/corrected/tools/check_village_square_cross_junction_feasibility.py
python3 art-direction/production-proof/corrected/tools/validate_terrain_accessibility_review.py
python3 art-direction/world-layout-proposal/tools/validate.py
python3 scripts/validate-direction-a-import-proof.py
python3 scripts/validate-runtime-terrain.py
python3 art-direction/production-proof/corrected/tools/validate_village_edge_square_connector.py --check-captures
python3 art-direction/production-proof/corrected/tools/check_square_garden_path_feasibility.py
python3 scripts/validate-runtime-plants.py
python3 scripts/validate-runtime-preparation-table.py
python3 scripts/validate-runtime-herb-beds.py
python3 scripts/validate-runtime-seeker-walk.py
dotnet restore Allrpcs.sln --disable-parallel -m:1 /nr:false
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --no-build
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj --no-build
dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj --no-build
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
./scripts/smoke-godot.sh
godot --path game --quit-after 60
