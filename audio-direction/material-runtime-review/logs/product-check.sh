#!/bin/sh
set -eu
cd /Users/hherb/src/allrpcs
./scripts/validate-workflows.sh
python3 scripts/validate-direction-a-import-proof.py
dotnet restore Allrpcs.sln --disable-parallel -m:1 /nr:false
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --no-build
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj --no-build
dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj --no-build
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
./scripts/smoke-godot.sh
ALLRPCS_AUDIO_MAIN_CHECK=1 ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 \
    ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=1 \
    godot --headless --path game --quit-after 120

# No artwork, frozen acceptance or old-validator input changed: reuse its retained
# terminal evidence. validate-audio-runtime.py proves the exact additive Main delta
# and rejects changes to every other tracked product/art predecessor.
# Rendered-output behavior is independently measured with the stopped-by-default
# AudioBusesControlsProof scene and scripts/check-audio-output.py.
