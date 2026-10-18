using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the unchanged request, schema and decision policies for all ten decision kinds.</summary>
/// <remarks>The lifecycle flags are OpenAI-only stream policies; the 128-token bound serves OpenAI and Ollama.</remarks>
public static class CharacterStructuredDecisions
{
    private const int DecisionOutputTokens = 128;

    /// <summary>Gets the exact character_dialogue_misquotation_evidence_decision contract.</summary>
    public static StructuredDecisionDescriptor<CharacterDialogueMisquotationEvidenceConsiderationRequest, CharacterDialogueMisquotationEvidenceDecision>
        DialogueMisquotationEvidence
    { get; } = new(
            "character_dialogue_misquotation_evidence_decision",
            CharacterDialogueMisquotationEvidenceRequestContent.DeveloperInstructions,
            CharacterDialogueMisquotationEvidenceRequestContent.SerializeTrustedContext,
            CharacterDialogueMisquotationEvidenceDecisionJsonSchemaWriter.Write,
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize,
            DecisionOutputTokens, CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterObjectSabotageMoveDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterSabotageActionDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterTargetSabotageDelayDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterTargetSabotageInfluenceDecisionJsonDeserializer.MaximumDocumentLength,
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
            DecisionOutputTokens, CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);

}
