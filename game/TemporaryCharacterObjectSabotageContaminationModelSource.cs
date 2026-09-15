using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed contamination model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationModelSource : Node
{
    private readonly ICharacterObjectSabotageContaminationDecisionModel offlineModel =
        new UnavailableContaminationModel();
    private CharacterObjectSabotageContaminationModelBinding? binding;
    private ICharacterObjectSabotageContaminationDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured contamination-model binding once.</summary>
    public bool TryInitializeBinding(CharacterObjectSabotageContaminationModelBinding configured)
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
        CharacterObjectSabotageContaminationModelBinding expected,
        CharacterObjectSabotageContaminationModelBinding replacement)
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
    public bool TryClearBinding(CharacterObjectSabotageContaminationModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact contamination model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterObjectSabotageContaminationDecisionModel model)
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
    public ICharacterObjectSabotageContaminationDecisionModel TakeModel()
    {
        ICharacterObjectSabotageContaminationDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableContaminationModel
        : ICharacterObjectSabotageContaminationDecisionModel
    {
        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterObjectSabotageContaminationDecision>(
                new InvalidOperationException("Contamination decisions are unavailable."));
        }
    }
}
