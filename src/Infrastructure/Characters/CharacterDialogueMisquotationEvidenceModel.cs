using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Adapts the shared provider client to the existing typed decision contract.</summary>
public sealed class CharacterDialogueMisquotationEvidenceModel : ICharacterDialogueMisquotationEvidenceDecisionModel
{
    private readonly IStructuredDecisionClient<CharacterDialogueMisquotationEvidenceConsiderationRequest, CharacterDialogueMisquotationEvidenceDecision> client;

    /// <summary>Retains the exact provider-neutral client without performing I/O.</summary>
    public CharacterDialogueMisquotationEvidenceModel(IStructuredDecisionClient<CharacterDialogueMisquotationEvidenceConsiderationRequest, CharacterDialogueMisquotationEvidenceDecision> client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
    }

    /// <inheritdoc />
    public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request, CancellationToken cancellationToken) => client.DecideAsync(request, cancellationToken);
}
