using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed concealment model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageConcealmentModelSource : Node
{
    private readonly ICharacterObjectSabotageConcealmentDecisionModel offlineModel =
        new UnavailableConcealmentModel();
    private ICharacterObjectSabotageConcealmentDecisionModel? suppliedModel;

    /// <summary>Retains one exact concealment model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterObjectSabotageConcealmentDecisionModel model)
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
    public ICharacterObjectSabotageConcealmentDecisionModel TakeModel()
    {
        ICharacterObjectSabotageConcealmentDecisionModel model = suppliedModel ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableConcealmentModel
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromException<CharacterObjectSabotageConcealmentDecision>(
                new InvalidOperationException("Concealment decisions are unavailable."));
        }
    }
}
