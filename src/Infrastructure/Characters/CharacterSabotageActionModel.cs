using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterSabotageActionModel : ICharacterSabotageActionDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterSabotageActionModel(IStructuredDecisionClient<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterSabotageActionDecision> DecideAsync(
        CharacterSabotageActionConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
