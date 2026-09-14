using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed move model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageMoveModelSource : Node
{
    private readonly ICharacterObjectSabotageMoveDecisionModel offlineModel = new UnavailableMoveModel();
    private ICharacterObjectSabotageMoveDecisionModel? suppliedModel;

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

    /// <summary>Takes the exact one-shot model or the stable fail-closed offline model.</summary>
    public ICharacterObjectSabotageMoveDecisionModel TakeModel()
    {
        ICharacterObjectSabotageMoveDecisionModel model = suppliedModel ?? offlineModel;
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
