using Allrpcs.Application.Investigations;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Retains the request, schema and decision policy for interpreting a proposed hypothesis.</summary>
public static class InvestigationHypothesisDecisions
{
    /// <summary>Gets the exact investigation_hypothesis_recognition_decision contract.</summary>
    public static StructuredDecisionDescriptor<InvestigationHypothesisRecognitionRequest, InvestigationHypothesisDecision>
        Recognition
    { get; } = new(
            "investigation_hypothesis_recognition_decision",
            InvestigationHypothesisRequestJsonSerializer.DeveloperInstructions,
            InvestigationHypothesisRequestJsonSerializer.Serialize,
            InvestigationHypothesisDecisionJson.WriteSchema,
            InvestigationHypothesisDecisionJson.Deserialize,
            128, InvestigationHypothesisDecisionJson.MaximumDocumentLength,
            requireLifecycle: true, allowMetadataAfterTextDone: true);
}
