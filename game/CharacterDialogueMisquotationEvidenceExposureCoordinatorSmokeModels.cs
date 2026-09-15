using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Contains private model doubles for the exposure-coordinator smoke.</summary>
public partial class CharacterDialogueMisquotationEvidenceExposureCoordinatorSmokeCheck
{
    private sealed class ExposureCoordinatorImmediateModel(
        CharacterDialogueMisquotationEvidenceDecision decision)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed class ExposureCoordinatorWaitingModel
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        private readonly TaskCompletionSource<CharacterDialogueMisquotationEvidenceDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class ExposureCoordinatorFailingModel
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterDialogueMisquotationEvidenceDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }
}
