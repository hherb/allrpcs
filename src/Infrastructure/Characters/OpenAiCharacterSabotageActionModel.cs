using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Retains the typed decision contract over the shared OpenAI transport.</summary>
public sealed class OpenAiCharacterSabotageActionModel : ICharacterSabotageActionDecisionModel
{
    private readonly OpenAiResponsesStructuredClient<CharacterSabotageActionConsiderationRequest,
        CharacterSabotageActionDecision> client;

    /// <summary>Creates an adapter over caller-owned dependencies and the default API base.</summary>
    public OpenAiCharacterSabotageActionModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings)
        : this(client, credentialSource, settings, OpenAiEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned dependencies and a validated API base.</summary>
    public OpenAiCharacterSabotageActionModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        OpenAiEndpoint endpoint,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        this.client = new(client, credentialSource, settings, endpoint,
            OpenAiCharacterSabotageActionRequestSerializer.Serialize,
            OpenAiCharacterSabotageActionSseCompletion.CompleteAsync,
            timeoutOptions);
    }

    /// <inheritdoc />
    public ValueTask<CharacterSabotageActionDecision> DecideAsync(
        CharacterSabotageActionConsiderationRequest request,
        CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
