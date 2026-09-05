#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

ALLRPCS_NAVIGATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_HEALER_COMMAND_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 1500
ALLRPCS_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_INDICATOR_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_OUTCOME_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
