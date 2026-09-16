using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed influence model or one stable offline default.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceModelSource : Node
{
    private readonly ICharacterTargetSabotageInfluenceDecisionModel offlineModel =
        new UnavailableInfluenceModel();
    private CharacterTargetSabotageInfluenceModelBinding? binding;
    private ICharacterTargetSabotageInfluenceDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured influence model binding once.</summary>
    public bool TryInitializeBinding(CharacterTargetSabotageInfluenceModelBinding configured)
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
        CharacterTargetSabotageInfluenceModelBinding expected,
        CharacterTargetSabotageInfluenceModelBinding replacement)
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
    public bool TryClearBinding(CharacterTargetSabotageInfluenceModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact influence model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterTargetSabotageInfluenceDecisionModel model)
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
    public ICharacterTargetSabotageInfluenceDecisionModel TakeModel()
    {
        ICharacterTargetSabotageInfluenceDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableInfluenceModel
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterTargetSabotageInfluenceDecision>(
                new InvalidOperationException("Influence decisions are unavailable."));
        }
    }
}
