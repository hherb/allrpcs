using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed move model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageMoveModelSource : Node
{
    private readonly ICharacterObjectSabotageMoveDecisionModel offlineModel = new UnavailableMoveModel();
    private CharacterObjectSabotageMoveModelBinding? binding;
    private ICharacterObjectSabotageMoveDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured move-model binding once.</summary>
    public bool TryInitializeBinding(CharacterObjectSabotageMoveModelBinding configured)
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
        CharacterObjectSabotageMoveModelBinding expected,
        CharacterObjectSabotageMoveModelBinding replacement)
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
    public bool TryClearBinding(CharacterObjectSabotageMoveModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact move model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterObjectSabotageMoveDecisionModel model)
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
    public ICharacterObjectSabotageMoveDecisionModel TakeModel()
    {
        ICharacterObjectSabotageMoveDecisionModel model = suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableMoveModel : ICharacterObjectSabotageMoveDecisionModel
    {
        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterObjectSabotageMoveDecision>(
                new InvalidOperationException("Move decisions are unavailable."));
        }
    }
}
