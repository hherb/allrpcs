using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterTargetSabotageInfluenceModel : ICharacterTargetSabotageInfluenceDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterTargetSabotageInfluenceConsiderationRequest, CharacterTargetSabotageInfluenceDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterTargetSabotageInfluenceModel(IStructuredDecisionClient<CharacterTargetSabotageInfluenceConsiderationRequest, CharacterTargetSabotageInfluenceDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
