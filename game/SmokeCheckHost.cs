using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Attaches only the engine checks a launch requests, so the shipping Main scene holds none. An
/// entry is requested when any of its gates is set, and the check still evaluates its own gate
/// value. Entries keep their former Main.tscn order and relative position (directly after the named
/// sibling, behind earlier hosted checks with the same anchor), so process and input order are
/// unchanged. They attach during Main._EnterTree, so a hosted check enters the tree before Main's
/// scene children; Godot readies children in insertion order, so it readies after them and before
/// Main itself. A fixture that other checks or proofs reach also lists their gates. Exports attach
/// nothing.
/// </summary>
internal static class SmokeCheckHost
{
    private static readonly Entry[] Entries =
    [
        Check<AdaptiveDisplaySmokeCheck>("AdaptiveDisplay", "ALLRPCS_ADAPTIVE_DISPLAY_CHECK"),
        Check<AudioSettingsMainSmokeCheck>("AudioSettingsPanel", "ALLRPCS_AUDIO_MAIN_CHECK"),
        Check<MusicAudioRuntimeSmokeCheck>("OpeningRegionMusicAudioController", "ALLRPCS_MUSIC_MAIN_CHECK"),
        Check<OptionalOverlayVisibilitySmokeCheck>(
            "OptionalOverlayVisibilityController", "ALLRPCS_OPTIONAL_OVERLAY_VISIBILITY_CHECK"),
        Check<RuntimeTerrainGroundSmokeCheck>("Ground", "ALLRPCS_RUNTIME_TERRAIN_GROUND_CHECK"),
        Check<NavigationSmokeCheck>("GreyboxNavigation", "ALLRPCS_NAVIGATION_CHECK"),
        Check<OpeningRegionProjectionSmokeCheck>(
            "OpeningRegionProjectionSource", "ALLRPCS_OPENING_REGION_PROJECTION_CHECK"),
        Check<HealerCommandSmokeCheck>("OpeningRegionProjectionSource", "ALLRPCS_HEALER_COMMAND_CHECK"),
        Check<OpeningRegionFirstPatientSceneSmokeCheck>(
            "OpeningRegionFirstPatientSceneSource", "ALLRPCS_OPENING_REGION_FIRST_PATIENT_SCENE_CHECK"),
        Check<OpeningRegionSickhouseNavigationSmokeCheck>(
            "OpeningRegionFirstPatientSceneSource", "ALLRPCS_OPENING_REGION_SICKHOUSE_CHECK"),
        Check<OpeningRegionRemedyPreparationSceneSmokeCheck>(
            "OpeningRegionRemedyPreparationSceneSource", "ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_SCENE_CHECK"),
        Check<RuntimeGardenFoliageSmokeCheck>(
            "OpeningRegionRemedyPreparationSceneSource", "ALLRPCS_RUNTIME_GARDEN_FOLIAGE_CHECK"),
        Check<RuntimePreparationTableSmokeCheck>(
            "OpeningRegionRemedyPreparationSceneSource", "ALLRPCS_RUNTIME_PREPARATION_TABLE_CHECK"),
        Check<RuntimeHerbBedSmokeCheck>("OpeningRegionRemedyPreparationSceneSource", "ALLRPCS_RUNTIME_HERB_BED_CHECK"),
        Check<RuntimeSeekerWalkSmokeCheck>(
            "OpeningRegionRemedyPreparationSceneSource", "ALLRPCS_RUNTIME_SEEKER_WALK_CHECK"),
        Check<OpeningRegionRemedyPreparationInputSmokeCheck>(
            "OpeningRegionRemedyPreparationInputCoordinator", "ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_INPUT_CHECK"),
        Check<OpeningRegionFirstPatientAdministrationInputSmokeCheck>(
            "OpeningRegionFirstPatientAdministrationInputCoordinator",
            "ALLRPCS_OPENING_REGION_FIRST_PATIENT_ADMINISTRATION_INPUT_CHECK"),
        Check<OpeningRegionFirstPatientStabilizationTestimonySmokeCheck>(
            "OpeningRegionFirstPatientStabilizationTestimonyCoordinator",
            "ALLRPCS_OPENING_REGION_FIRST_PATIENT_STABILIZATION_TESTIMONY_CHECK"),
        Check<OpeningRegionSageConsultationInputSmokeCheck>(
            "OpeningRegionSageConsultationInputCoordinator", "ALLRPCS_OPENING_REGION_SAGE_CONSULTATION_INPUT_CHECK"),
        Check<OpeningRegionFirstPatientRecoveryRestorationSmokeCheck>(
            "OpeningRegionSageConsultationInputCoordinator",
            "ALLRPCS_OPENING_REGION_FIRST_PATIENT_RECOVERY_RESTORATION_CHECK"),
        Check<OpeningRegionVillageTransitionSmokeCheck>(
            "OpeningRegionVillageTransitionSceneSource", "ALLRPCS_OPENING_REGION_VILLAGE_TRANSITION_CHECK"),
        Check<OpeningRegionStreamLaundrySmokeCheck>(
            "OpeningRegionStreamLaundrySceneSource", "ALLRPCS_OPENING_REGION_STREAM_LAUNDRY_CHECK"),
        Check<OpeningRegionWoodlandClearingSmokeCheck>(
            "OpeningRegionWoodlandClearingSceneSource", "ALLRPCS_OPENING_REGION_WOODLAND_CLEARING_CHECK"),
        Check<OpeningRegionEnvironmentalStateSmokeCheck>(
            "OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator",
            "ALLRPCS_OPENING_REGION_ENVIRONMENTAL_STATE_CHECK"),
        Check<SeekerCustomizationSelectionSceneSmokeCheck>(
            "SeekerCustomizationSelectionSceneSource", "ALLRPCS_SEEKER_CUSTOMIZATION_SELECTION_CHECK"),
        Check<SeekerBackgroundRouteVariationSceneSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_SEEKER_BACKGROUND_ROUTE_STATE_CHECK"),
        Check<SeekerScribeInspectionAccessSceneSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_SEEKER_SCRIBE_INSPECTION_ACCESS_CHECK"),
        Check<SeekerApothecaryPreparationAccessSceneSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_SEEKER_APOTHECARY_PREPARATION_ACCESS_CHECK"),
        Check<SeekerTravellerRoadWagonAccessSceneSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_SEEKER_TRAVELLER_ROAD_WAGON_ACCESS_CHECK"),
        Check<SeekerCustomizationMenuSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_MODAL_INPUT_CHECK",
            "ALLRPCS_SEEKER_CUSTOMIZATION_MENU_CHECK"),
        Check<OpeningRegionOldShrineSmokeCheck>(
            "SeekerBackgroundRouteAccessSceneSource", "ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK"),
        Check<OpeningRegionSouthernRoadWagonSmokeCheck>(
            "OpeningRegionWagonClueFocusedSearchRoutingCoordinator",
            "ALLRPCS_OPENING_REGION_SOUTHERN_ROAD_WAGON_CHECK"),
        Check<OpeningRegionClosedForestPathSmokeCheck>(
            "OpeningRegionClosedForestPathSceneSource", "ALLRPCS_OPENING_REGION_CLOSED_FOREST_PATH_CHECK"),
        Check<InteractionFocusSmokeCheck>("CharacterModelGenerationFlowController", "ALLRPCS_INTERACTION_FOCUS_CHECK"),
        Check<SeekerMovementBoundarySmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_BOUNDARY_CHECK"),
        Check<FocusedSearchActivityIndicatorSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FOCUSED_SEARCH_INDICATOR_CHECK"),
        Check<FocusedSearchActivityControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FOCUSED_SEARCH_CONTROLLER_CHECK"),
        Check<FocusedSearchOutcomeControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FOCUSED_SEARCH_OUTCOME_CONTROLLER_CHECK"),
        Check<TemporaryFocusedSearchOutcomeSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FOCUSED_SEARCH_OUTCOME_SOURCE_CHECK"),
        Check<TemporaryWorldObjectSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_OBJECT_SOURCE_CHECK"),
        Check<WorldObjectControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_OBJECT_CONTROLLER_CHECK"),
        Check<WorldEventLogControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_EVENT_LOG_CONTROLLER_CHECK"),
        Check<TemporaryWorldEventSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_EVENT_SOURCE_CHECK"),
        Check<InitialWorldEventRecordingSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_INITIAL_WORLD_EVENT_CHECK"),
        Check<WorldTimeControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_TIME_CONTROLLER_CHECK"),
        Check<WorldEventControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_EVENT_CONTROLLER_CHECK"),
        Check<CharacterMemoryControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_MEMORY_CONTROLLER_CHECK"),
        Check<JournalControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_CONTROLLER_CHECK"),
        Check<SeekerMapControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_CONTROLLER_CHECK"),
        Check<OpeningRegionInvestigationRouteProgressControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_PROGRESS_CHECK"),
        Check<OpeningRegionInvestigationRouteAuthorityCoordinatorSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_AUTHORITY_CHECK"),
        Check<OpeningRegionInvestigationRouteCompletionCoordinatorSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CHECK"),
        Check<OpeningRegionInvestigationRouteCompletionConsumptionCoordinatorSmokeCheck>(
            "CharacterModelGenerationFlowController",
            "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_CONSUMPTION_CHECK"),
        Check<OpeningRegionInvestigationRouteCompletionPresentationSmokeCheck>(
            "CharacterModelGenerationFlowController",
            "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_PRESENTATION_CHECK"),
        Check<OpeningRegionInvestigationRouteObservationRoutingCoordinatorSmokeCheck>(
            "CharacterModelGenerationFlowController",
            "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_OBSERVATION_ROUTING_CHECK"),
        Check<OpeningRegionStreamResiduePassiveNoticingSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK"),
        Check<OpeningRegionWagonClueFocusedSearchRouteSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WAGON_CLUE_ROUTE_CHECK"),
        Check<SeekerMapPresentationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_PRESENTATION_CHECK"),
        Check<SeekerMapPanelSmokeCheck>("CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_PANEL_CHECK"),
        Check<SeekerMapOpenCloseInputSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_OPEN_CLOSE_INPUT_CHECK"),
        Check<SeekerMapNoteEditingSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_NOTE_EDITING_CHECK"),
        Check<OpeningRegionMapVisitVerificationSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_OPENING_REGION_MAP_VISIT_VERIFICATION_CHECK"),
        Check<SeekerMapTestimonyReportingSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SEEKER_MAP_TESTIMONY_REPORTING_CHECK"),
        Check<RepresentativeJournalCompositionSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_REPRESENTATIVE_JOURNAL_COMPOSITION_CHECK"),
        Check<JournalSectionSelectionSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_SECTION_SELECTION_CHECK"),
        Check<JournalSectionPresentationSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_SECTION_PRESENTATION_CHECK"),
        Check<JournalSectionPresentationRefreshSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_SECTION_PRESENTATION_REFRESH_CHECK"),
        Check<JournalSectionPanelSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_SECTION_PANEL_CHECK"),
        Check<JournalOpenCloseInputSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_JOURNAL_OPEN_CLOSE_INPUT_CHECK"),
        Check<FirstPatientStateControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FIRST_PATIENT_STATE_CONTROLLER_CHECK"),
        Check<TemporaryFirstPatientTreatmentContextSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FIRST_PATIENT_CONTEXT_SOURCE_CHECK"),
        Check<FirstPatientPreparationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FIRST_PATIENT_PREPARATION_CHECK"),
        Check<FirstPatientAdministrationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FIRST_PATIENT_ADMINISTRATION_CHECK"),
        Check<FirstPatientSaveStateCaptureSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FIRST_PATIENT_SAVE_STATE_CAPTURE_CHECK"),
        Check<FocusedSearchWorldEventSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_FOCUSED_SEARCH_WORLD_EVENT_CHECK"),
        Check<DiscoverySaveStateRestorationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_DISCOVERY_SAVE_STATE_RESTORATION_CONTROLLER_CHECK"),
        Check<WorldEventSaveStateRestorationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_WORLD_EVENT_SAVE_STATE_RESTORATION_CONTROLLER_CHECK"),
        Check<SaveSnapshotRestorationControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SAVE_SNAPSHOT_RESTORATION_CONTROLLER_CHECK"),
        Check<SaveSnapshotCaptureControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SAVE_SNAPSHOT_CAPTURE_CONTROLLER_CHECK"),
        Check<SaveSnapshotJsonControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_SAVE_SNAPSHOT_JSON_CONTROLLER_CHECK"),
        Check<SaveDocumentStorageControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", SaveDocumentStorageController.SmokeCheckEnvironmentVariable),
        Check<SaveLoadInputSmokeCheck>(
            "CharacterModelGenerationFlowController", SaveLoadInputController.SmokeCheckEnvironmentVariable),
        Check<CharacterTurnActivityControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_CONTROLLER_CHECK"),
        Check<TemporaryCharacterTurnSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_SOURCE_CHECK"),
        Check<CharacterTurnCancellationInputSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_INPUT_CHECK"),
        Check<DialogueInputSmokeCheck>("CharacterModelGenerationFlowController", "ALLRPCS_DIALOGUE_INPUT_CHECK"),
        Check<DialogueEventControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_DIALOGUE_EVENT_CONTROLLER_CHECK"),
        Check<CharacterTurnTranscriptControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_TRANSCRIPT_CHECK"),
        Check<TemporaryCharacterTurnRequestSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_REQUEST_SOURCE_CHECK"),
        Check<CharacterTurnRequestFlowSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_REQUEST_FLOW_CHECK"),
        Check<TemporaryCharacterTurnResponseSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_RESPONSE_SOURCE_CHECK"),
        Check<TemporaryCharacterExaminationResultSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_EXAMINATION_RESULT_SOURCE_CHECK"),
        Check<CharacterTurnWorldEventOccurrenceControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_TURN_WORLD_EVENT_CONTROLLER_CHECK"),
        Check<AutonomousConversationBudgetControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_AUTONOMOUS_CONVERSATION_BUDGET_CHECK"),
        Check<CharacterIndependentTaskDepartureControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_INDEPENDENT_TASK_DEPARTURE_CONTROLLER_CHECK"),
        Check<TemporaryCharacterIndependentTaskSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_INDEPENDENT_TASK_SOURCE_CHECK"),
        Check<CharacterIndependentTaskOccurrenceControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_INDEPENDENT_TASK_OCCURRENCE_CONTROLLER_CHECK"),
        Check<CharacterIndependentTaskTravelControllerSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_INDEPENDENT_TASK_TRAVEL_CONTROLLER_CHECK"),
        Check<TemporaryCharacterIndependentTaskTravelSourceSmokeCheck>(
            "CharacterModelGenerationFlowController", "ALLRPCS_CHARACTER_INDEPENDENT_TASK_TRAVEL_SOURCE_CHECK"),
        Check<CharacterIndependentTaskReturnTravelControllerSmokeCheck>(
            "CharacterIndependentTaskReturnTravelController",
            "ALLRPCS_INDEPENDENT_TASK_RETURN_TRAVEL_CONTROLLER_CHECK"),
        Check<TemporaryCharacterIndependentTaskReturnTravelSourceSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_CHARACTER_INDEPENDENT_TASK_RETURN_TRAVEL_SOURCE_CHECK"),
        Check<CharacterIndependentTaskReturnRejoinControllerSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_RETURN_REJOIN_CONTROLLER_CHECK"),
        Check<CharacterIndependentTaskRoundTripCoordinatorSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_ROUND_TRIP_COORDINATOR_CHECK"),
        Check<CharacterIndependentTaskStepClockControllerSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_STEP_CLOCK_CONTROLLER_CHECK"),
        Check<CharacterIndependentTaskTimedRoundTripCoordinatorSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_TIMED_ROUND_TRIP_COORDINATOR_CHECK"),
        Check<IndependentTaskLiveRoundTripFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_LIVE_ROUND_TRIP_FLOW_CHECK"),
        Check<CharacterIndependentTaskBodyPresentationControllerSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_INDEPENDENT_TASK_BODY_PRESENTATION_CONTROLLER_CHECK"),
        Check<TemporaryCharacterObjectSabotageMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_OBJECT_SABOTAGE_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageMoveMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_OBJECT_SABOTAGE_MOVE_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageSubstitutionMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageSubstitutionPresentationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_PRESENTATION_FLOW_CHECK"),
        Check<TemporaryCharacterObjectSabotageSubstitutionConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterDialogueMisquotationEvidenceConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_DIALOGUE_MISQUOTATION_EVIDENCE_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterTargetSabotageInfluenceConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_TARGET_SABOTAGE_INFLUENCE_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterTargetSabotageDelayConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_TARGET_SABOTAGE_DELAY_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_TARGET_SABOTAGE_INFLUENCE_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterTargetSabotageDelayRecoveryConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_TARGET_SABOTAGE_DELAY_RECOVERY_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterDialogueMisquotationOccurrenceMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_DIALOGUE_MISQUOTATION_OCCURRENCE_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageMovePresentationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_MOVE_PRESENTATION_FLOW_CHECK"),
        Check<WaterCaskMoveSabotageInvestigationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_WATER_CASK_MOVE_SABOTAGE_INVESTIGATION_CHECK"),
        Check<WaterCaskSubstitutionSabotageInvestigationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_WATER_CASK_SUBSTITUTION_SABOTAGE_INVESTIGATION_CHECK"),
        Check<TemporaryCharacterObjectSabotageConsiderationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageModelPresentationMainSceneSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_MODEL_PRESENTATION_MAIN_SCENE_CHECK"),
        Check<CharacterObjectSabotageConcealmentPresentationControllerSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_OBJECT_SABOTAGE_CONCEALMENT_PRESENTATION_CHECK"),
        Check<TemporaryCharacterObjectSabotagePresentationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_OBJECT_SABOTAGE_PRESENTATION_FLOW_CHECK"),
        Check<WaterCaskInteractionFocusSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_WATER_CASK_INTERACTION_FOCUS_CHECK"),
        Check<WaterCaskSabotageInvestigationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_WATER_CASK_SABOTAGE_INVESTIGATION_CHECK"),
        Check<CharacterTurnParticipationRoundTaskControllerSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_PARTICIPATION_ROUND_TASK_CHECK"),
        Check<TemporaryCharacterTurnParticipationModelSourceSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_PARTICIPATION_MODEL_SOURCE_CHECK"),
        Check<OrdinaryPlayerDialogueParticipationCoordinatorSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_PLAYER_PARTICIPATION_CHECK"),
        Check<OrdinaryAutonomousDialogueFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_AUTONOMOUS_FLOW_CHECK"),
        Check<OrdinaryParticipationCancellationFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_PARTICIPATION_CANCELLATION_CHECK"),
        Check<OrdinaryPlayerDialogueParticipationSchedulingSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_PARTICIPATION_SCHEDULING_CHECK"),
        Check<TemporaryOrdinaryCharacterParticipantSourceSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController",
            "ALLRPCS_ORDINARY_CHARACTER_PARTICIPANT_SOURCE_CHECK"),
        Check<OrdinarySageInterjectionFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_SAGE_INTERJECTION_CHECK"),
        Check<OrdinaryBoundedConversationLoopSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_ORDINARY_BOUNDED_LOOP_CHECK"),
        Check<CharacterTurnResponseFlowSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_CHARACTER_TURN_RESPONSE_FLOW_CHECK"),
        Check<CharacterTurnResponsePresentationSmokeCheck>(
            "CharacterIndependentTaskBodyPresentationController", "ALLRPCS_CHARACTER_TURN_RESPONSE_PRESENTATION_CHECK"),
        Check<CharacterTurnResponseSpeechFlowSmokeCheck>(
            "TemporaryCharacterTurnResponseSpeechRecorder", "ALLRPCS_CHARACTER_TURN_RESPONSE_SPEECH_FLOW_CHECK"),
        Check<CharacterExaminationFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK"),
        Check<CharacterTurnCompletionFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_TURN_COMPLETION_FLOW_CHECK"),
        Check<CharacterTurnRepeatFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_TURN_REPEAT_FLOW_CHECK"),
        Check<CharacterModelGenerationStatusPresentationSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_STATUS_PRESENTATION_CHECK"),
        Check<CharacterModelStreamControllerSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_STREAM_CHECK"),
        Check<CharacterModelStreamTextPresentationSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_STREAM_TEXT_PRESENTATION_CHECK"),
        Check<CharacterModelGenerationFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_FLOW_CHECK"),
        Check<TemporaryCharacterModelGenerationInputSourceSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_GENERATION_INPUT_SOURCE_CHECK"),
        Check<TemporaryOfflineCharacterModelSourceSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_OFFLINE_CHARACTER_MODEL_CHECK"),
        Check<TemporaryCharacterModelSourceSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_SCRIPTED_CHARACTER_MODEL_CHECK"),
        Check<CharacterModelInvalidResponseFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_INVALID_MODEL_RESPONSE_CHECK"),
        Check<CharacterModelOrdinaryCancellationSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_ORDINARY_MODEL_CANCELLATION_CHECK"),
        Check<CharacterModelOrdinaryProviderFailureSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK"),
        Check<CharacterModelOrdinaryTimeoutSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_ORDINARY_MODEL_TIMEOUT_CHECK"),
        Check<CharacterModelOrdinaryModelSwitchSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_ORDINARY_MODEL_SWITCH_CHECK"),
        Check<CharacterModelOrdinaryMalformedStreamSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_ORDINARY_MALFORMED_MODEL_CHECK"),
        Check<LiveCharacterModelOrdinaryFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_LIVE_OLLAMA_ORDINARY_ACTION",
            "ALLRPCS_LIVE_OLLAMA_ORDINARY_WAIT", "ALLRPCS_LIVE_OPENAI_ORDINARY_ACTION",
            "ALLRPCS_LIVE_OPENAI_ORDINARY_WAIT"),
        Check<CharacterModelSettingsStorageControllerSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            CharacterModelSettingsStorageController.SmokeCheckEnvironmentVariable),
        Check<TemporaryCharacterModelCompositionControllerSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK"),
        Check<TemporaryCharacterSabotageActionConsiderationMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_SABOTAGE_ACTION_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageActionRequestMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_SABOTAGE_ACTION_REQUEST_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeMoveRecoveryMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_SABOTAGE_OUTCOME_MOVE_RECOVERY_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeSubstitutionRecoveryMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_SABOTAGE_OUTCOME_SUBSTITUTION_RECOVERY_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeContaminationRecoveryMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_SABOTAGE_OUTCOME_CONTAMINATION_RECOVERY_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeMisquotationViewMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_SABOTAGE_OUTCOME_MISQUOTATION_VIEW_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeDelayRecoveryRequestMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_SABOTAGE_OUTCOME_DELAY_RECOVERY_REQUEST_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageOutcomeInfluenceRecoveryRequestMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_SABOTAGE_OUTCOME_INFLUENCE_RECOVERY_REQUEST_MAIN_SCENE_CHECK"),
        Check<TemporaryCharacterObjectSabotageContaminationConsiderationMainSceneSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_OBJECT_SABOTAGE_CONTAMINATION_CONSIDERATION_MAIN_SCENE_CHECK"),
        Check<WaterCaskContaminationSabotageInvestigationFlowSmokeCheck>(
            "TemporaryCharacterTurnCompletionController",
            "ALLRPCS_WATER_CASK_CONTAMINATION_SABOTAGE_INVESTIGATION_CHECK"),
        Check<CharacterModelSettingsControllerSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_SETTINGS_CONTROLLER_CHECK"),
        Check<CharacterModelSettingsPanelSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_SETTINGS_PANEL_CHECK"),
        Check<OpenAiCredentialControllerSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_OPENAI_CREDENTIAL_CONTROLLER_CHECK"),
        Check<OpenAiCredentialPanelSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_OPENAI_CREDENTIAL_PANEL_CHECK"),
        Check<CharacterModelConnectionPanelSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_CHARACTER_MODEL_CONNECTION_PANEL_CHECK"),
        Check<WorldObjectBoundInteractionFocusSmokeCheck>(
            "TemporaryCharacterTurnCompletionController", "ALLRPCS_WORLD_OBJECT_INTERACTION_FOCUS_CHECK"),
        Check<TemporarySeekerCharacterSourceSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_SEEKER_CHARACTER_SOURCE_CHECK"),
        Check<PassiveNoticingCandidateControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_PASSIVE_CANDIDATE_CONTROLLER_CHECK"),
        Check<PassiveNoticingResolutionControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_PASSIVE_RESOLUTION_CONTROLLER_CHECK"),
        Check<TemporaryPassiveNoticingCandidateSourceSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_PASSIVE_CANDIDATE_SOURCE_CHECK"),
        Check<ObservationReinterpretationCandidateControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_REINTERPRETATION_CANDIDATE_CONTROLLER_CHECK"),
        Check<ObservationReinterpretationResolutionControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_REINTERPRETATION_RESOLUTION_CONTROLLER_CHECK"),
        Check<TemporaryObservationReinterpretationCandidateSourceSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_REINTERPRETATION_CANDIDATE_SOURCE_CHECK"),
        Check<TemporarySeekerPerceptionSourceSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_SEEKER_PERCEPTION_SOURCE_CHECK"),
        Check<FocusedSearchPerceptionControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_FOCUSED_SEARCH_PERCEPTION_CONTROLLER_CHECK"),
        Check<FocusedSearchOutcomeResolutionControllerSmokeCheck>(
            "TemporarySeekerPerceptionSource", "ALLRPCS_FOCUSED_SEARCH_OUTCOME_RESOLUTION_CONTROLLER_CHECK"),
        Check<CharacterSabotageOrdinaryTurnAuthoringMainSceneSmokeCheck>(
            "DialogueOverlay", "ALLRPCS_SABOTAGE_ORDINARY_TURN_AUTHORING_MAIN_SCENE_CHECK"),
        Check<CharacterSabotageTurnOpportunityControllerSmokeCheck>(
            "DialogueOverlay", "ALLRPCS_SABOTAGE_TURN_OPPORTUNITY_CONTROLLER_CHECK"),
        Check<RuntimeGardenGroundSmokeCheck>("GardenGroundPresentation", "ALLRPCS_RUNTIME_GARDEN_GROUND_CHECK"),
        Check<RuntimeSquareGroundSmokeCheck>("SquareGroundPresentation", "ALLRPCS_RUNTIME_SQUARE_GROUND_CHECK"),
        Check<RuntimeSickhouseGroundSmokeCheck>(
            "SickhouseGroundPresentation", "ALLRPCS_RUNTIME_SICKHOUSE_GROUND_CHECK"),
        Check<RuntimeWagonGroundSmokeCheck>("WagonGroundPresentation", "ALLRPCS_RUNTIME_WAGON_GROUND_CHECK"),
        Check<RuntimeShrineGroundSmokeCheck>("ShrineGroundPresentation", "ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK"),
        Check<RuntimeLaundryGroundSmokeCheck>("LaundryGroundPresentation", "ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK"),
        Check<RuntimeVillageEdgeGroundSmokeCheck>(
            "VillageEdgeGroundPresentation", "ALLRPCS_RUNTIME_VILLAGE_EDGE_GROUND_CHECK"),
        Check<RuntimeWoodlandClearingDressingSmokeCheck>(
            "WoodlandClearingDressingPresentation", "ALLRPCS_RUNTIME_WOODLAND_CLEARING_DRESSING_CHECK"),
        Check<MaterialAudioRuntimeSmokeCheck>(
            "OpeningRegionMaterialAudioController", "ALLRPCS_MATERIAL_MAIN_CHECK", "ALLRPCS_POSITIONAL_MAIN_CHECK"),
        Check<FootstepAudioRuntimeSmokeCheck>("OpeningRegionFootstepAudioController", "ALLRPCS_FOOTSTEP_MAIN_CHECK"),
        Check<PositionalAudioRuntimeSmokeCheck>(
            "OpeningRegionPositionalAudioController", "ALLRPCS_POSITIONAL_MAIN_CHECK"),
        Check<InteriorAudioRuntimeSmokeCheck>("OpeningRegionInteriorAudioSource", "ALLRPCS_INTERIOR_MAIN_CHECK"),
        Check<WaterContactRuntimeSmokeCheck>(
            "OpeningRegionWaterContactAudioController", "ALLRPCS_WATER_MAIN_CHECK", "ALLRPCS_WATER_OUTPUT_CHECK"),
        Check<CaskCoverHandlingRuntimeSmokeCheck>(
            "OpeningRegionCaskCoverAudioController", "ALLRPCS_CASK_MAIN_CHECK", "ALLRPCS_CASK_OUTPUT_CHECK"),
        Check<HandToolTapRuntimeSmokeCheck>(
            "OpeningRegionHandToolTapAudioController", "ALLRPCS_TOOL_MAIN_CHECK", "ALLRPCS_FOLIAGE_OUTPUT_CHECK",
            "ALLRPCS_FOLIAGE_MAIN_CHECK", "ALLRPCS_TOOL_OUTPUT_CHECK", "ALLRPCS_HEALER_FOOTSTEP_OUTPUT_CHECK",
            "ALLRPCS_HEALER_FOOTSTEP_CHECK", "ALLRPCS_HERB_LAST_CASE_CHECK", "ALLRPCS_HERB_OUTPUT_CHECK",
            "ALLRPCS_HERB_MAIN_CHECK", "ALLRPCS_RAIN_OUTPUT_CHECK", "ALLRPCS_RAIN_MAIN_CHECK",
            "ALLRPCS_COOLING_OUTPUT_CHECK", "ALLRPCS_COOLING_MAIN_CHECK", "ALLRPCS_HEATING_OUTPUT_CHECK",
            "ALLRPCS_HEATING_MAIN_CHECK", "ALLRPCS_STEEPING_OUTPUT_CHECK", "ALLRPCS_STEEPING_MAIN_CHECK",
            "ALLRPCS_WIND_OUTPUT_CHECK", "ALLRPCS_WIND_MAIN_CHECK"),
        Check<HealerFootstepRuntimeSmokeCheck>(
            "OpeningRegionHealerFootstepAudioController", "ALLRPCS_HEALER_FOOTSTEP_CHECK"),
        Check<FoliageAudioRuntimeSmokeCheck>("OpeningRegionFoliageAudioController", "ALLRPCS_FOLIAGE_MAIN_CHECK"),
        Check<WindAudioRuntimeSmokeCheck>("OpeningRegionWindAudioController", "ALLRPCS_WIND_MAIN_CHECK"),
        Check<RainAudioRuntimeSmokeCheck>("OpeningRegionRainAudioController", "ALLRPCS_RAIN_MAIN_CHECK"),
        Check<RemedyHeatingAudioRuntimeSmokeCheck>(
            "OpeningRegionRemedyHeatingAudioController", "ALLRPCS_HEATING_MAIN_CHECK"),
        Check<RemedySteepingAudioRuntimeSmokeCheck>(
            "OpeningRegionRemedySteepingAudioController", "ALLRPCS_STEEPING_MAIN_CHECK"),
        Check<RemedyCoolingAudioRuntimeSmokeCheck>(
            "OpeningRegionRemedyCoolingAudioController", "ALLRPCS_COOLING_MAIN_CHECK"),
        Check<HerbCollectionAudioRuntimeSmokeCheck>(
            "OpeningRegionHerbCollectionAudioController", "ALLRPCS_HERB_MAIN_CHECK"),
        Scene("ControlSettingsMainSmokeCheck", "ControlSettingsPanel", "ALLRPCS_CONTROLS_MAIN_CHECK"),
        Scene("DialogueAccessibilityMainSmokeCheck", "ControlSettingsPanel", "ALLRPCS_DIALOGUE_ACCESSIBILITY_CHECK"),
        Check<DialogueSpeechMainSmokeCheck>("DialogueSpeechController", "ALLRPCS_DIALOGUE_SPEECH_CHECK"),
        Check<PlayerSpeechMainSmokeCheck>("PlayerSpeechController", "ALLRPCS_PLAYER_ASR_CHECK"),
        Check<SpaceHoldSpeechSmokeCheck>("SpaceHoldSpeechController", "ALLRPCS_SPACE_HOLD_CHECK"),
        Check<DialogueOwnershipMainSmokeCheck>("SpaceHoldSpeechController", "ALLRPCS_DIALOGUE_OWNERSHIP_CHECK"),
    ];

    /// <summary>Called from <see cref="Main._EnterTree"/>, while Main's children are not yet ready.</summary>
    internal static void Attach(Main main)
    {
        // Exports exclude every harness script and scene.
        if (OS.HasFeature("template"))
        {
            return;
        }

        // Forward order keeps both index and ready (insertion) order among hosted checks; the
        // count places each one after the checks already attached behind the same sibling.
        Dictionary<string, int> attachedAfter = [];
        foreach (Entry entry in Entries)
        {
            if (main.HasNode(entry.Name) || !entry.Gates.Any(gate => OS.GetEnvironment(gate).Length > 0))
            {
                continue;
            }

            // Resolve the anchor before attaching, so a missing one leaves no misplaced check behind.
            int index = entry.After is null
                ? 0
                : (main.GetNodeOrNull(entry.After)
                    ?? throw new InvalidOperationException(
                        $"SmokeCheckHost anchor '{entry.After}' for {entry.Name} is not a Main child.")).GetIndex() + 1;
            string anchor = entry.After ?? string.Empty;
            int following = attachedAfter.GetValueOrDefault(anchor);
            Node check = entry.Create();
            check.Name = entry.Name;
            main.AddChild(check);
            main.MoveChild(check, index + following);
            attachedAfter[anchor] = following + 1;
        }
    }

    private static Entry Check<T>(string? after, params string[] gates)
        where T : Node, new()
    {
        return new(typeof(T).Name, () => new T(), after, gates);
    }

    private static Entry Scene(string name, string? after, params string[] gates)
    {
        string path = $"res://{name}.tscn";
        return new(
            name,
            () => (GD.Load<PackedScene>(path)
                ?? throw new InvalidOperationException($"SmokeCheckHost cannot load check scene '{path}'.")).Instantiate(),
            after,
            gates);
    }

    private sealed record Entry(string Name, Func<Node> Create, string? After, string[] Gates);
}
