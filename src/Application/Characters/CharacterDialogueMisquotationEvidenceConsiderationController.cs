using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral misquotation-evidence decision.</summary>
public static class CharacterDialogueMisquotationEvidenceConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterDialogueMisquotationEvidenceConsiderationResult> DecideAsync(
        ICharacterDialogueMisquotationEvidenceDecisionModel model,
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterDialogueMisquotationEvidenceDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterDialogueMisquotationEvidenceConsiderationResult(request, decision);
    }
}
