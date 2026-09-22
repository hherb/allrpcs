using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed delay-recovery model or one stable offline default.</summary>
public partial class TemporaryCharacterTargetSabotageDelayRecoveryModelSource : Node
{
    private readonly ICharacterTargetSabotageDelayRecoveryDecisionModel offlineModel =
        new UnavailableDelayRecoveryModel();
    private CharacterTargetSabotageDelayRecoveryModelBinding? binding;
    private ICharacterTargetSabotageDelayRecoveryDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured delay-recovery model binding once.</summary>
    public bool TryInitializeBinding(
        CharacterTargetSabotageDelayRecoveryModelBinding configured)
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
        CharacterTargetSabotageDelayRecoveryModelBinding expected,
        CharacterTargetSabotageDelayRecoveryModelBinding replacement)
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
        CharacterTargetSabotageDelayRecoveryModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact delay-recovery model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterTargetSabotageDelayRecoveryDecisionModel model)
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
    public ICharacterTargetSabotageDelayRecoveryDecisionModel TakeModel()
    {
        ICharacterTargetSabotageDelayRecoveryDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableDelayRecoveryModel
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterTargetSabotageDelayRecoveryDecision>(
                new InvalidOperationException("Delay-recovery decisions are unavailable."));
        }
    }
}
