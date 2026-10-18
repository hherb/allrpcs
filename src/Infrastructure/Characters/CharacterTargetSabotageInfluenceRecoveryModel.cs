using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterTargetSabotageInfluenceRecoveryModel : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterTargetSabotageInfluenceRecoveryModel(IStructuredDecisionClient<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
