#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

export ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1
export ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=1

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
ALLRPCS_AUTONOMOUS_CONVERSATION_BUDGET_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_DEPARTURE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_INDEPENDENT_TASK_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_OCCURRENCE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_TRAVEL_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_INDEPENDENT_TASK_TRAVEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_RETURN_TRAVEL_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_CHARACTER_INDEPENDENT_TASK_RETURN_TRAVEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_RETURN_REJOIN_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_ROUND_TRIP_COORDINATOR_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_STEP_CLOCK_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_TIMED_ROUND_TRIP_COORDINATOR_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_LIVE_ROUND_TRIP_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_INDEPENDENT_TASK_BODY_PRESENTATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_MAIN_SCENE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_MOVE_MAIN_SCENE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_PRESENTATION_FLOW_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_OBJECT_SABOTAGE_MOVE_PRESENTATION_FLOW_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_OBJECT_SABOTAGE_MODEL_PRESENTATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_OBJECT_SABOTAGE_CONCEALMENT_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_OBJECT_SABOTAGE_PRESENTATION_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WATER_CASK_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WATER_CASK_SABOTAGE_INVESTIGATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WATER_CASK_MOVE_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_WATER_CASK_SUBSTITUTION_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionConsiderationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageContaminationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageContaminationTaskControllerSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageContaminationModelSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceModelSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceTaskControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowSmokeCheck.tscn \
    --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceExposureControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceViewControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceExposureCoordinatorSmokeCheck.tscn --quit-after 120
ALLRPCS_DIALOGUE_MISQUOTATION_EVIDENCE_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_OBJECT_SABOTAGE_CONTAMINATION_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_WATER_CASK_CONTAMINATION_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game" --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionTaskControllerSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionModelSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionFlowSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionPresentationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveConsiderationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveTaskControllerSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveModelSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveConsiderationFlowSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveFlowSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMovePresentationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentConsiderationControllerSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentTaskControllerSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentPresentationCoordinatorSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageConcealmentModelSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageConsiderationFlowSmokeCheck.tscn --quit-after 120
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSourceSmokeCheck.tscn --quit-after 30
godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageFlowSmokeCheck.tscn --quit-after 30
ALLRPCS_PARTICIPATION_ROUND_TASK_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_PARTICIPATION_MODEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_PLAYER_PARTICIPATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_PARTICIPATION_SCHEDULING_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 120
ALLRPCS_ORDINARY_CHARACTER_PARTICIPANT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 30
ALLRPCS_ORDINARY_SAGE_INTERJECTION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 1000
ALLRPCS_ORDINARY_BOUNDED_LOOP_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 1000
ALLRPCS_ORDINARY_AUTONOMOUS_FLOW_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 1000
ALLRPCS_ORDINARY_PARTICIPATION_CANCELLATION_CHECK=1 godot --headless --path "$repository_root/game" --quit-after 1000
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
