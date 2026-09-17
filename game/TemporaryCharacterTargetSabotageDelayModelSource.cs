using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed target-delay model or one stable offline default.</summary>
public partial class TemporaryCharacterTargetSabotageDelayModelSource : Node
{
    private readonly ICharacterTargetSabotageDelayDecisionModel offlineModel =
        new UnavailableDelayModel();
    private CharacterTargetSabotageDelayModelBinding? binding;
    private ICharacterTargetSabotageDelayDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured target-delay model binding once.</summary>
    public bool TryInitializeBinding(CharacterTargetSabotageDelayModelBinding configured)
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
        CharacterTargetSabotageDelayModelBinding expected,
        CharacterTargetSabotageDelayModelBinding replacement)
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
    public bool TryClearBinding(CharacterTargetSabotageDelayModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact target-delay model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterTargetSabotageDelayDecisionModel model)
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
    public ICharacterTargetSabotageDelayDecisionModel TakeModel()
    {
        ICharacterTargetSabotageDelayDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableDelayModel : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterTargetSabotageDelayDecision>(
                new InvalidOperationException("Target-delay decisions are unavailable."));
        }
    }
}
