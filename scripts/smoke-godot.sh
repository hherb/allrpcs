#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

export ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1
export ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=1

# Every launch goes through run_check (scripts/smoke-lib.sh): it fails on a nonzero exit, on an
# unhandled .NET exception in the output, and (with a frame deadline) on a check that never quits.
# Launches listed in smoke-quarantine.txt still run but are reported instead of failing.
smoke_quarantine_file="$script_dir/smoke-quarantine.txt"
. "$script_dir/smoke-lib.sh"

# Import every resource first. --import exits by itself (with 0) once importing finishes, so a cold
# checkout is fully imported before any check runs; quality.py imports the same way.
run_check - godot --headless --editor --path "$repository_root/game" --import
python3 "$script_dir/check-shipping-policy.py"
run_check - godot --headless --editor --path "$repository_root/game" \
    --script "$script_dir/check-shipping-scene.gd"
# Prove the wrapper itself (pass, hang, throw and async-throw probes) before trusting any check
# through it. The self-test always fails fast, even when ALLRPCS_SMOKE_CONTINUE=1 collects the
# failures of the launches below.
(unset ALLRPCS_SMOKE_CONTINUE; "$script_dir/test-smoke-harness.sh")
run_check 30 godot --headless --path "$repository_root/game" \
    res://DirectionAImportProof.tscn
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-shrine-southern-road-runtime.gd"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-woodland-shrine-runtime.gd"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-woodland-village-edge-runtime.gd"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-village-edge-stream-runtime.gd"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-stream-laundry-runtime.gd"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-closed-forest-runtime.gd"
run_check 30 ALLRPCS_RUNTIME_TERRAIN_GROUND_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_GARDEN_FOLIAGE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_PREPARATION_TABLE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_HERB_BED_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_SEEKER_WALK_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 godot --headless --fixed-fps 60 --path "$repository_root/game" res://CameraFramingSmokeCheck.tscn
run_check 30 ALLRPCS_ADAPTIVE_DISPLAY_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPTIONAL_OVERLAY_VISIBILITY_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_NAVIGATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_PROJECTION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_FIRST_PATIENT_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_SICKHOUSE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_FIRST_PATIENT_ADMINISTRATION_INPUT_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_FIRST_PATIENT_STABILIZATION_TESTIMONY_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_SAGE_CONSULTATION_INPUT_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_FIRST_PATIENT_RECOVERY_RESTORATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_INPUT_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_VILLAGE_TRANSITION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_STREAM_LAUNDRY_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_WOODLAND_CLEARING_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK=route \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_ENVIRONMENTAL_STATE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SEEKER_CUSTOMIZATION_SELECTION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_NEW_GAME_CUSTOMIZATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SAVE_CONTINUE_CHECK=1 \
    godot --headless --path "$repository_root/game" res://SaveContinueRelaunchCheck.tscn
run_check 120 ALLRPCS_SEEKER_BACKGROUND_ROUTE_STATE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SEEKER_SCRIBE_INSPECTION_ACCESS_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SEEKER_APOTHECARY_PREPARATION_ACCESS_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SEEKER_TRAVELLER_ROAD_WAGON_ACCESS_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_SOUTHERN_ROAD_WAGON_CHECK=1 \
    godot --headless --fixed-fps 60 --path "$repository_root/game"
run_check 120 ALLRPCS_OPENING_REGION_CLOSED_FOREST_PATH_CHECK=1 \
    godot --headless --fixed-fps 60 --path "$repository_root/game"
run_check - godot --headless --path "$repository_root/game" \
    --script "$repository_root/scripts/check-whole-channel-runtime.gd"
run_check 1500 ALLRPCS_HEALER_COMMAND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_INDICATOR_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_OUTCOME_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_OUTCOME_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_OBJECT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_OBJECT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_EVENT_LOG_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_EVENT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INITIAL_WORLD_EVENT_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_TIME_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MEMORY_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_MAP_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_PROGRESS_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_AUTHORITY_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CONSUMPTION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_PRESENTATION_CHECK=success \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_PRESENTATION_CHECK=failed \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_PRESENTATION_CHECK=restored \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_OBSERVATION_ROUTING_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK=missing \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK=route \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WAGON_CLUE_ROUTE_CHECK=missing \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WAGON_CLUE_ROUTE_CHECK=route \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_MAP_PRESENTATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_MAP_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_MAP_OPEN_CLOSE_INPUT_CHECK=1 \
    godot --headless --path "$repository_root/game"
# Each launch owns a fresh one-shot customization state.
for modal in map journal; do
    for opening in custom reading; do
        for closing in custom reading; do
            for baseline in active suspended; do
                run_check 120 ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK=1 \
                    ALLRPCS_MODAL_INPUT_CHECK="$modal-$opening-$closing-$baseline" godot --headless --path "$repository_root/game"
            done
        done
        run_check 120 ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK=1 \
            ALLRPCS_MODAL_INPUT_CHECK="$modal-$opening-reading-active-restore" godot --headless --path "$repository_root/game"
    done
done
run_check 30 ALLRPCS_SEEKER_MAP_NOTE_EDITING_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENING_REGION_MAP_VISIT_VERIFICATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_MAP_TESTIMONY_REPORTING_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_REPRESENTATIVE_JOURNAL_COMPOSITION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_SECTION_SELECTION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_SECTION_PRESENTATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_SECTION_PRESENTATION_REFRESH_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_SECTION_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_JOURNAL_OPEN_CLOSE_INPUT_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FIRST_PATIENT_STATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FIRST_PATIENT_CONTEXT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FIRST_PATIENT_PREPARATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FIRST_PATIENT_ADMINISTRATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FIRST_PATIENT_SAVE_STATE_CAPTURE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_WORLD_EVENT_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_OBJECT_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_CHARACTER_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_PASSIVE_CANDIDATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_PASSIVE_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_PASSIVE_CANDIDATE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_REINTERPRETATION_CANDIDATE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_REINTERPRETATION_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_REINTERPRETATION_CANDIDATE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SEEKER_PERCEPTION_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_PERCEPTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_FOCUSED_SEARCH_OUTCOME_RESOLUTION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_DISCOVERY_SAVE_STATE_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WORLD_EVENT_SAVE_STATE_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SAVE_SNAPSHOT_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 300 ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 ALLRPCS_RESTORE_REGRESSION=participation ALLRPCS_SAVE_SNAPSHOT_RESTORATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SAVE_SNAPSHOT_CAPTURE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SAVE_SNAPSHOT_JSON_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SAVE_DOCUMENT_STORAGE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_SAVE_LOAD_INPUT_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_AUTONOMOUS_CONVERSATION_BUDGET_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_DEPARTURE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_INDEPENDENT_TASK_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_OCCURRENCE_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_TRAVEL_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_INDEPENDENT_TASK_TRAVEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_RETURN_TRAVEL_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_INDEPENDENT_TASK_RETURN_TRAVEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_RETURN_REJOIN_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_ROUND_TRIP_COORDINATOR_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_STEP_CLOCK_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_TIMED_ROUND_TRIP_COORDINATOR_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_LIVE_ROUND_TRIP_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_INDEPENDENT_TASK_BODY_PRESENTATION_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_MAIN_SCENE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_MOVE_MAIN_SCENE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_PRESENTATION_FLOW_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_MOVE_PRESENTATION_FLOW_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OBJECT_SABOTAGE_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OBJECT_SABOTAGE_MODEL_PRESENTATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_CONCEALMENT_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OBJECT_SABOTAGE_PRESENTATION_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WATER_CASK_INTERACTION_FOCUS_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WATER_CASK_SABOTAGE_INVESTIGATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WATER_CASK_MOVE_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WATER_CASK_SUBSTITUTION_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionConsiderationControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageContaminationControllerSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageContaminationTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageContaminationModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterSabotageActionModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterSabotageActionModelCompositionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionSelectionCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionConcealmentRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionMoveRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionSubstitutionRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionContaminationRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionMisquotationRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionDelayRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionInfluenceRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionRequestComposerSmokeCheck.tscn
run_check 300 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionAuthorBindingCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterSabotageActionRequestConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterSabotageActionConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayResolutionCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceRecoveryModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayRecoveryModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceRecoveryModelCompositionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayRecoveryModelCompositionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryResolutionCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryResolutionCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryCandidateControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryCandidateControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryCandidateRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageDelayRecoveryStartCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryCandidateRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceRecoveryStartCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceModelCompositionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageDelayModelCompositionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterTargetSabotageInfluenceConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterTargetSabotageInfluenceResolutionCoordinatorSmokeCheck.tscn
run_check 120 ALLRPCS_TARGET_SABOTAGE_INFLUENCE_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_TARGET_SABOTAGE_DELAY_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_TARGET_SABOTAGE_DELAY_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_TARGET_SABOTAGE_INFLUENCE_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_ACTION_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_ACTION_REQUEST_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_ORDINARY_TURN_AUTHORING_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_TURN_OPPORTUNITY_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_MOVE_RECOVERY_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_SUBSTITUTION_RECOVERY_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_CONTAMINATION_RECOVERY_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_MISQUOTATION_VIEW_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_DELAY_RECOVERY_REQUEST_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_SABOTAGE_OUTCOME_INFLUENCE_RECOVERY_REQUEST_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceExposureControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceViewControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidencePresentationSelectionControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidencePresentationControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidencePresentationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidencePresentationPanelSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceReviewPanelSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterDialogueMisquotationEvidenceReviewSelectionSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationOccurrenceCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceCandidateControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceCandidateRequestControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterDialogueMisquotationEvidenceExposureCoordinatorSmokeCheck.tscn
run_check 120 ALLRPCS_DIALOGUE_MISQUOTATION_EVIDENCE_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_DIALOGUE_MISQUOTATION_OCCURRENCE_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_OBJECT_SABOTAGE_CONTAMINATION_CONSIDERATION_MAIN_SCENE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_WATER_CASK_CONTAMINATION_SABOTAGE_INVESTIGATION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionModelSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSubstitutionFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageSubstitutionPresentationControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveConsiderationControllerSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMoveTaskControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveModelSourceSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageMoveFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageMovePresentationControllerSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentConsiderationControllerSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentTaskControllerSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://CharacterObjectSabotageConcealmentPresentationCoordinatorSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageConcealmentModelSourceSmokeCheck.tscn
run_check 120 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageConsiderationFlowSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageSourceSmokeCheck.tscn
run_check 30 godot --headless --path "$repository_root/game" \
    res://TemporaryCharacterObjectSabotageFlowSmokeCheck.tscn
run_check 120 ALLRPCS_PARTICIPATION_ROUND_TASK_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_PARTICIPATION_MODEL_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_ORDINARY_PLAYER_PARTICIPATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_ORDINARY_PARTICIPATION_SCHEDULING_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_ORDINARY_CHARACTER_PARTICIPANT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 1000 ALLRPCS_ORDINARY_SAGE_INTERJECTION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 1000 ALLRPCS_ORDINARY_BOUNDED_LOOP_CHECK=1 godot --headless --path "$repository_root/game"
run_check 1000 ALLRPCS_ORDINARY_AUTONOMOUS_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 1000 ALLRPCS_ORDINARY_PARTICIPATION_CANCELLATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_INPUT_CHECK=awaiting godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_INPUT_CHECK=speaking godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_DIALOGUE_INPUT_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_DIALOGUE_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_TRANSCRIPT_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_REQUEST_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_REQUEST_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_RESPONSE_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_EXAMINATION_RESULT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_WORLD_EVENT_CONTROLLER_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK=generated godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_RESPONSE_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_TURN_RESPONSE_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_RESPONSE_SPEECH_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_COMPLETION_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_TURN_REPEAT_FLOW_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_STATUS_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_STREAM_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_STREAM_TEXT_PRESENTATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=timeout godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=cancel godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=complete godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_CHARACTER_MODEL_FLOW_CHECK=failed godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_GENERATION_INPUT_SOURCE_CHECK=1 godot --headless --path "$repository_root/game"
# Two ordinary offline generations each retain the one-second retry backoff. Pace current
# frames so both real delays fit the unchanged 120-frame fixture and 30-second generation limits.
run_check 120 ALLRPCS_OFFLINE_CHARACTER_MODEL_CHECK=1 godot --headless --path "$repository_root/game" --max-fps 30
run_check 180 ALLRPCS_SCRIPTED_CHARACTER_MODEL_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_INVALID_MODEL_RESPONSE_CHECK=1 godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_ORDINARY_MODEL_CANCELLATION_CHECK=1 godot --headless --path "$repository_root/game"
run_check 180 ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=retry godot --headless --path "$repository_root/game"
run_check 120 ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=terminal godot --headless --path "$repository_root/game"
# The check waits on the scene's 30-second wall-clock model deadline and fails itself after 45
# seconds (about 2,900 frames here). Capping the frame rate bounds those 45 seconds to 5,400
# frames on any machine, so the frame deadline only catches a check that stops evaluating.
run_check 6000 ALLRPCS_ORDINARY_MODEL_TIMEOUT_CHECK=1 \
    godot --headless --path "$repository_root/game" --max-fps 120
# The check polls asynchronous model completions within its own 240-frame bound; uncapped headless
# frames can spend that bound in well under a second, so cap the frame rate (240 frames = 2 s).
run_check 240 ALLRPCS_ORDINARY_MODEL_SWITCH_CHECK=1 \
    godot --headless --path "$repository_root/game" --max-fps 120
run_check 120 ALLRPCS_ORDINARY_MALFORMED_MODEL_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_STORAGE_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_CHARACTER_MODEL_SETTINGS_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_CONTROLLER_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
    ALLRPCS_OPENAI_CREDENTIAL_TEST_STORE=1 \
    ALLRPCS_CHARACTER_MODEL_CONNECTION_PANEL_CHECK=1 \
    godot --headless --path "$repository_root/game"

# Audio Main checks (#422). The music check awaits physics frames, whose process-frame cost grows
# with an uncapped frame rate, so --max-fps 120 bounds it; the material check's 0.8 s timers and the
# footstep check's physics ticks run under --fixed-fps 60 so their frame counts are deterministic.
# The footstep deadline keeps the check's own 5,000-tick navigation wait.
run_check 30 ALLRPCS_AUDIO_MAIN_CHECK=1 godot --headless --path "$repository_root/game"
run_check 330 ALLRPCS_MUSIC_MAIN_CHECK=1 \
    godot --headless --path "$repository_root/game" --max-fps 120
run_check 330 ALLRPCS_MATERIAL_MAIN_CHECK=1 \
    godot --headless --fixed-fps 60 --path "$repository_root/game"
run_check 5400 ALLRPCS_FOOTSTEP_MAIN_CHECK=1 \
    godot --headless --fixed-fps 60 --path "$repository_root/game"
run_check 120 ALLRPCS_POSITIONAL_MAIN_CHECK=1 godot --headless --path "$repository_root/game"
# Retained terrain underlay checks that used to run only from the frozen scripts/quality.sh list.
run_check 30 ALLRPCS_RUNTIME_GARDEN_GROUND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_SQUARE_GROUND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_SICKHOUSE_GROUND_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_WAGON_GROUND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK=1 godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_VILLAGE_EDGE_GROUND_CHECK=1 \
    godot --headless --path "$repository_root/game"
run_check 30 ALLRPCS_RUNTIME_WOODLAND_CLEARING_DRESSING_CHECK=1 \
    godot --headless --path "$repository_root/game"
# Character animation import proof, at the fixed frame rate its frozen verifier uses (playback
# order checks are frame-timing sensitive).
run_check - godot --headless --fixed-fps 100 --path "$repository_root/game" \
    --script "$repository_root/game/CharacterAnimationImportProofCheck.gd"
# Stopped audio proof and diagnostic scenes: without their output gates (which need Movie Maker)
# they must load, start stopped and never quit on their own, so --quit-after ends them and only an
# exception or script error fails the launch.
run_check - godot --headless --path "$repository_root/game" \
    res://AudioBusesControlsProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://CaskCoverHandlingAudioOutputCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://CaskCoverHandlingRuntimeSmokeCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://FoliageAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://FootstepAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://HandToolTapAudioOutputCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://HandToolTapRuntimeSmokeCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://HealerFootstepAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://HerbCollectionAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://InteriorAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://MaterialAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://MusicAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://PositionalAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://RainAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://RemedyCoolingAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://RemedyHeatingAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://RemedySteepingAudioRuntimeProof.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://WaterContactAudioOutputCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://WaterContactRuntimeSmokeCheck.tscn --quit-after 30
run_check - godot --headless --path "$repository_root/game" \
    res://WindAudioRuntimeProof.tscn --quit-after 30

# The current speech fixture includes microphone toolbar focus, with no capture/provider.
speech_check_dir=$(mktemp -d "${TMPDIR:-/tmp}/allrpcs-speech-smoke.XXXXXX")
trap 'rm -rf "$speech_check_dir"' EXIT HUP INT TERM
for mode in unavailable headless; do
    run_check 3600 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 \
        ALLRPCS_SPEECH_CONFIG="$speech_check_dir/absent.json" ALLRPCS_DIALOGUE_SPEECH_CHECK=1 \
        ALLRPCS_DIALOGUE_SPEECH_MODE="$mode" ALLRPCS_DIALOGUE_SPEECH_REPORT="$speech_check_dir/$mode.json" \
        godot --headless --max-fps 60 --path "$repository_root/game"
done

for mode in pending once hidden hiddenreply unavailable stopped timeout held invalid generation-held generation-stopped generation-timeout budget; do
    run_check 300 ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 \
        ALLRPCS_SPEECH_CONFIG="$speech_check_dir/absent.json" ALLRPCS_DIALOGUE_OWNERSHIP_CHECK="$mode" \
        godot --headless --max-fps 60 --path "$repository_root/game"
done

run_check 300 godot --headless --path "$repository_root/game" res://MainProcessOrderCheck.tscn

smoke_summary
