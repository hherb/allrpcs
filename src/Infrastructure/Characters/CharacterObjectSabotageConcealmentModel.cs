using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterObjectSabotageConcealmentModel : ICharacterObjectSabotageConcealmentDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterObjectSabotageConcealmentConsiderationRequest, CharacterObjectSabotageConcealmentDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterObjectSabotageConcealmentModel(IStructuredDecisionClient<CharacterObjectSabotageConcealmentConsiderationRequest, CharacterObjectSabotageConcealmentDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
        CharacterObjectSabotageConcealmentConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
