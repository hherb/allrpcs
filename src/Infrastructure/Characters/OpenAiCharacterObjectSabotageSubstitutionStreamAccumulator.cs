using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterObjectSabotageSubstitutionStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageSubstitutionConsiderationRequest,
        CharacterObjectSabotageSubstitutionDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterObjectSabotageSubstitutionStreamAccumulator(
        CharacterObjectSabotageSubstitutionConsiderationRequest request)
    {
        stream = new(request,
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false,
            allowMetadataAfterTextDone: true);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterObjectSabotageSubstitutionDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
