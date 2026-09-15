using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral misquotation-evidence decision for an exact request.</summary>
public interface ICharacterDialogueMisquotationEvidenceDecisionModel
{
    /// <summary>Returns one misquotation-evidence decision while observing caller cancellation.</summary>
    ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        CancellationToken cancellationToken);
}
