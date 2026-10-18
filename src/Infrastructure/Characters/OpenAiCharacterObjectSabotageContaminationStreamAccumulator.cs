using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterObjectSabotageContaminationStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageContaminationConsiderationRequest,
        CharacterObjectSabotageContaminationDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterObjectSabotageContaminationStreamAccumulator(
        CharacterObjectSabotageContaminationConsiderationRequest request)
    {
        stream = new(request,
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: true,
            allowMetadataAfterTextDone: true);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterObjectSabotageContaminationDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
