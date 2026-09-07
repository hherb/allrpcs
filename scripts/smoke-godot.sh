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
ALLRPCS_FOCUSED_SEARCH_OUTCOME_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_OBJECT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_OBJECT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_EVENT_LOG_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_EVENT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INITIAL_WORLD_EVENT_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_TIME_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_WORLD_EVENT_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_OBJECT_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SEEKER_CHARACTER_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_PASSIVE_CANDIDATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_PASSIVE_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_PASSIVE_CANDIDATE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_REINTERPRETATION_CANDIDATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_REINTERPRETATION_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_REINTERPRETATION_CANDIDATE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SEEKER_PERCEPTION_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_PERCEPTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FOCUSED_SEARCH_OUTCOME_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
