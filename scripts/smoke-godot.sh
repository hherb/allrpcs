#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

export ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1

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
ALLRPCS_CHARACTER_MEMORY_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FIRST_PATIENT_STATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FIRST_PATIENT_CONTEXT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FIRST_PATIENT_PREPARATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FIRST_PATIENT_ADMINISTRATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_FIRST_PATIENT_SAVE_STATE_CAPTURE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
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
ALLRPCS_DISCOVERY_SAVE_STATE_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WORLD_EVENT_SAVE_STATE_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SAVE_SNAPSHOT_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SAVE_SNAPSHOT_CAPTURE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SAVE_SNAPSHOT_JSON_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_SAVE_DOCUMENT_STORAGE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_INPUT_CHECK=awaiting godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_INPUT_CHECK=speaking godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_DIALOGUE_INPUT_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_DIALOGUE_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_TRANSCRIPT_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_REQUEST_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_REQUEST_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_RESPONSE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_EXAMINATION_RESULT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_WORLD_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK=generated godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_TURN_RESPONSE_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_RESPONSE_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_TURN_RESPONSE_SPEECH_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_COMPLETION_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_TURN_REPEAT_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_STATUS_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_STREAM_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_STREAM_TEXT_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=timeout godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=cancel godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=complete godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=failed godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_MODEL_GENERATION_INPUT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OFFLINE_CHARACTER_MODEL_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_SCRIPTED_CHARACTER_MODEL_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 180
ALLRPCS_INVALID_MODEL_RESPONSE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_MODEL_CANCELLATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=retry godot --headless --path "$repository_root/game" --quit-after 180
ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=terminal godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_MODEL_TIMEOUT_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 100000000
ALLRPCS_ORDINARY_MODEL_SWITCH_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 240
ALLRPCS_ORDINARY_MALFORMED_MODEL_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_STORAGE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_CHARACTER_MODEL_CONNECTION_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
