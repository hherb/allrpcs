#!/bin/sh
set -eu
cd /Users/hherb/src/allrpcs
./scripts/smoke-godot.sh
ALLRPCS_RUNTIME_GARDEN_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SQUARE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SICKHOUSE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_WAGON_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK=1 godot --headless --path game --quit-after 30
ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK=1 godot --headless --path game --quit-after 30
godot --path game --quit-after 60
