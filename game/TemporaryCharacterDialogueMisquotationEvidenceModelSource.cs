using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed misquotation-evidence model or one stable offline default.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceModelSource : Node
{
    private readonly ICharacterDialogueMisquotationEvidenceDecisionModel offlineModel =
        new UnavailableMisquotationEvidenceModel();
    private CharacterDialogueMisquotationEvidenceModelBinding? binding;
    private ICharacterDialogueMisquotationEvidenceDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured misquotation-evidence model binding once.</summary>
    public bool TryInitializeBinding(CharacterDialogueMisquotationEvidenceModelBinding configured)
    {
        ArgumentNullException.ThrowIfNull(configured);
        if (binding is not null)
        {
            return false;
        }

        binding = configured;
        return true;
    }

    /// <summary>Atomically replaces the exact expected configured binding for future decisions.</summary>
    public bool TryReplaceBinding(
        CharacterDialogueMisquotationEvidenceModelBinding expected,
        CharacterDialogueMisquotationEvidenceModelBinding replacement)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(replacement);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = replacement;
        return true;
    }

    /// <summary>Clears only the exact expected binding while preserving any one-shot model.</summary>
    public bool TryClearBinding(CharacterDialogueMisquotationEvidenceModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact misquotation-evidence model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterDialogueMisquotationEvidenceDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (suppliedModel is not null)
        {
            return false;
        }

        suppliedModel = model;
        return true;
    }

    /// <summary>Takes the exact one-shot, configured, or stable fail-closed offline model.</summary>
    public ICharacterDialogueMisquotationEvidenceDecisionModel TakeModel()
    {
        ICharacterDialogueMisquotationEvidenceDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableMisquotationEvidenceModel
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterDialogueMisquotationEvidenceDecision>(
                new InvalidOperationException("Misquotation-evidence decisions are unavailable."));
        }
    }
}
