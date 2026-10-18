using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed stream contract over the shared decision lifecycle.</summary>
public sealed class OpenAiCharacterObjectSabotageMoveStreamAccumulator
{
    /// <summary>Gets the maximum accepted serialized semantic-event length.</summary>
    public const int MaximumEventLength = 64_000;

    private readonly OpenAiStructuredDecisionAccumulator<CharacterObjectSabotageMoveConsiderationRequest,
        CharacterObjectSabotageMoveDecision> stream;

    /// <summary>Binds the stream to the exact request and existing decision policy.</summary>
    public OpenAiCharacterObjectSabotageMoveStreamAccumulator(
        CharacterObjectSabotageMoveConsiderationRequest request)
    {
        stream = new(request,
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize,
            CharacterObjectSabotageMoveDecisionJsonDeserializer.MaximumDocumentLength,
            requireLifecycle: false,
            allowMetadataAfterTextDone: true);
    }

    /// <summary>Returns the exact decision only after verified successful completion.</summary>
    public CharacterObjectSabotageMoveDecision? Apply(string serializedEvent) => stream.Apply(serializedEvent);
}
