using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed recovery model or one stable offline default.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource : Node
{
    private readonly ICharacterTargetSabotageInfluenceRecoveryDecisionModel offlineModel =
        new UnavailableInfluenceRecoveryModel();
    private CharacterTargetSabotageInfluenceRecoveryModelBinding? binding;
    private ICharacterTargetSabotageInfluenceRecoveryDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured influence-recovery model binding once.</summary>
    public bool TryInitializeBinding(
        CharacterTargetSabotageInfluenceRecoveryModelBinding configured)
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
        CharacterTargetSabotageInfluenceRecoveryModelBinding expected,
        CharacterTargetSabotageInfluenceRecoveryModelBinding replacement)
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
    public bool TryClearBinding(
        CharacterTargetSabotageInfluenceRecoveryModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact influence-recovery model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterTargetSabotageInfluenceRecoveryDecisionModel model)
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
    public ICharacterTargetSabotageInfluenceRecoveryDecisionModel TakeModel()
    {
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableInfluenceRecoveryModel
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterTargetSabotageInfluenceRecoveryDecision>(
                new InvalidOperationException("Influence-recovery decisions are unavailable."));
        }
    }
}
