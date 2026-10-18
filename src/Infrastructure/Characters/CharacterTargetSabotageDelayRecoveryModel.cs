using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterTargetSabotageDelayRecoveryModel : ICharacterTargetSabotageDelayRecoveryDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterTargetSabotageDelayRecoveryConsiderationRequest, CharacterTargetSabotageDelayRecoveryDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterTargetSabotageDelayRecoveryModel(IStructuredDecisionClient<CharacterTargetSabotageDelayRecoveryConsiderationRequest, CharacterTargetSabotageDelayRecoveryDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
