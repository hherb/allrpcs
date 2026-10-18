using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed decision contract over the shared OpenAI transport.</summary>
public sealed class OpenAiCharacterObjectSabotageMoveModel : ICharacterObjectSabotageMoveDecisionModel
{
    private readonly OpenAiResponsesStructuredClient<CharacterObjectSabotageMoveConsiderationRequest,
        CharacterObjectSabotageMoveDecision> client;

    /// <summary>Creates an adapter over caller-owned dependencies and the default API base.</summary>
    public OpenAiCharacterObjectSabotageMoveModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings)
        : this(client, credentialSource, settings, OpenAiEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned dependencies and a validated API base.</summary>
    public OpenAiCharacterObjectSabotageMoveModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        OpenAiEndpoint endpoint,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        this.client = new(client, credentialSource, settings, endpoint,
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize,
            OpenAiCharacterObjectSabotageMoveSseCompletion.CompleteAsync,
            timeoutOptions);
    }

    /// <inheritdoc />
    public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
