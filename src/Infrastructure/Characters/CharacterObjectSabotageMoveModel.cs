using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterObjectSabotageMoveModel : ICharacterObjectSabotageMoveDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterObjectSabotageMoveConsiderationRequest, CharacterObjectSabotageMoveDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterObjectSabotageMoveModel(IStructuredDecisionClient<CharacterObjectSabotageMoveConsiderationRequest, CharacterObjectSabotageMoveDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
        CharacterObjectSabotageMoveConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
