using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed decision contract over the shared OpenAI transport.</summary>
public sealed class OpenAiCharacterTargetSabotageDelayRecoveryModel : ICharacterTargetSabotageDelayRecoveryDecisionModel
{
    private readonly OpenAiResponsesStructuredClient<CharacterTargetSabotageDelayRecoveryConsiderationRequest,
        CharacterTargetSabotageDelayRecoveryDecision> client;

    /// <summary>Creates an adapter over caller-owned dependencies and the default API base.</summary>
    public OpenAiCharacterTargetSabotageDelayRecoveryModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings)
        : this(client, credentialSource, settings, OpenAiEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned dependencies and a validated API base.</summary>
    public OpenAiCharacterTargetSabotageDelayRecoveryModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        OpenAiEndpoint endpoint,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        this.client = new(client, credentialSource, settings, endpoint,
            OpenAiCharacterTargetSabotageDelayRecoveryRequestSerializer.Serialize,
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync,
            timeoutOptions);
    }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
