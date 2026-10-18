using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterObjectSabotageSubstitutionModel : ICharacterObjectSabotageSubstitutionDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterObjectSabotageSubstitutionConsiderationRequest, CharacterObjectSabotageSubstitutionDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterObjectSabotageSubstitutionModel(IStructuredDecisionClient<CharacterObjectSabotageSubstitutionConsiderationRequest, CharacterObjectSabotageSubstitutionDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
