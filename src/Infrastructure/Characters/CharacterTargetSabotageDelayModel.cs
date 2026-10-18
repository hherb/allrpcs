using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterTargetSabotageDelayModel : ICharacterTargetSabotageDelayDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterTargetSabotageDelayModel(IStructuredDecisionClient<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
        CharacterTargetSabotageDelayConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
