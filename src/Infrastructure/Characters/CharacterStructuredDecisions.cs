using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the unchanged request, schema and decision policies for all ten decision kinds.</summary>
public static class CharacterStructuredDecisions
{
    /// <summary>Gets the exact character_dialogue_misquotation_evidence_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterDialogueMisquotationEvidenceConsiderationRequest, CharacterDialogueMisquotationEvidenceDecision>
        DialogueMisquotationEvidence
    { get; } = new(
            "character_dialogue_misquotation_evidence_decision",
            CharacterDialogueMisquotationEvidenceRequestContent.DeveloperInstructions,
            CharacterDialogueMisquotationEvidenceRequestContent.SerializeTrustedContext,
            CharacterDialogueMisquotationEvidenceDecisionJsonSchemaWriter.Write,
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize,
            128, CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_object_sabotage_concealment_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterObjectSabotageConcealmentConsiderationRequest, CharacterObjectSabotageConcealmentDecision>
        ObjectSabotageConcealment
    { get; } = new(
            "character_object_sabotage_concealment_decision",
            CharacterObjectSabotageConcealmentRequestContent.DeveloperInstructions,
            CharacterObjectSabotageConcealmentRequestContent.SerializeTrustedContext,
            CharacterObjectSabotageConcealmentDecisionJsonSchemaWriter.Write,
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize,
            128, CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_object_sabotage_contamination_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterObjectSabotageContaminationConsiderationRequest, CharacterObjectSabotageContaminationDecision>
        ObjectSabotageContamination
    { get; } = new(
            "character_object_sabotage_contamination_decision",
            CharacterObjectSabotageContaminationRequestContent.DeveloperInstructions,
            CharacterObjectSabotageContaminationRequestContent.SerializeTrustedContext,
            CharacterObjectSabotageContaminationDecisionJsonSchemaWriter.Write,
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.Deserialize,
            128, CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_object_sabotage_move_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterObjectSabotageMoveConsiderationRequest, CharacterObjectSabotageMoveDecision>
        ObjectSabotageMove
    { get; } = new(
            "character_object_sabotage_move_decision",
            CharacterObjectSabotageMoveRequestContent.DeveloperInstructions,
            CharacterObjectSabotageMoveRequestContent.SerializeTrustedContext,
            CharacterObjectSabotageMoveDecisionJsonSchemaWriter.Write,
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize,
            128, CharacterObjectSabotageMoveDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_object_sabotage_substitution_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterObjectSabotageSubstitutionConsiderationRequest, CharacterObjectSabotageSubstitutionDecision>
        ObjectSabotageSubstitution
    { get; } = new(
            "character_object_sabotage_substitution_decision",
            CharacterObjectSabotageSubstitutionRequestContent.DeveloperInstructions,
            CharacterObjectSabotageSubstitutionRequestContent.SerializeTrustedContext,
            CharacterObjectSabotageSubstitutionDecisionJsonSchemaWriter.Write,
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize,
            128, CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_sabotage_action_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision>
        SabotageAction
    { get; } = new(
            "character_sabotage_action_decision",
            CharacterSabotageActionRequestContent.DeveloperInstructions,
            CharacterSabotageActionRequestContent.SerializeTrustedContext,
            CharacterSabotageActionDecisionJsonSchemaWriter.Write,
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize,
            128, CharacterSabotageActionDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_target_sabotage_delay_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision>
        TargetSabotageDelay
    { get; } = new(
            "character_target_sabotage_delay_decision",
            CharacterTargetSabotageDelayRequestContent.DeveloperInstructions,
            CharacterTargetSabotageDelayRequestContent.SerializeTrustedContext,
            CharacterTargetSabotageDelayDecisionJsonSchemaWriter.Write,
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize,
            128, CharacterTargetSabotageDelayDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_target_sabotage_delay_recovery_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterTargetSabotageDelayRecoveryConsiderationRequest, CharacterTargetSabotageDelayRecoveryDecision>
        TargetSabotageDelayRecovery
    { get; } = new(
            "character_target_sabotage_delay_recovery_decision",
            CharacterTargetSabotageDelayRecoveryRequestContent.DeveloperInstructions,
            CharacterTargetSabotageDelayRecoveryRequestContent.SerializeTrustedContext,
            CharacterTargetSabotageDelayRecoveryDecisionJsonSchemaWriter.Write,
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize,
            128, CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: false);

    /// <summary>Gets the exact character_target_sabotage_influence_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterTargetSabotageInfluenceConsiderationRequest, CharacterTargetSabotageInfluenceDecision>
        TargetSabotageInfluence
    { get; } = new(
            "character_target_sabotage_influence_decision",
            CharacterTargetSabotageInfluenceRequestContent.DeveloperInstructions,
            CharacterTargetSabotageInfluenceRequestContent.SerializeTrustedContext,
            CharacterTargetSabotageInfluenceDecisionJsonSchemaWriter.Write,
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize,
            128, CharacterTargetSabotageInfluenceDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

    /// <summary>Gets the exact character_target_sabotage_influence_recovery_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision>
        TargetSabotageInfluenceRecovery
    { get; } = new(
            "character_target_sabotage_influence_recovery_decision",
            CharacterTargetSabotageInfluenceRecoveryRequestContent.DeveloperInstructions,
            CharacterTargetSabotageInfluenceRecoveryRequestContent.SerializeTrustedContext,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonSchemaWriter.Write,
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize,
            128, CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

}
