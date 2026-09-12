using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed participation model or one stable offline default.</summary>
public partial class TemporaryCharacterTurnParticipationModelSource : Node
{
    private readonly ICharacterTurnParticipationModel offlineModel =
        new UnavailableParticipationModel();
    private ICharacterTurnParticipationModel? suppliedModel;

    /// <summary>Retains one exact participation model for the next scene round only.</summary>
    public bool TrySupplyModel(ICharacterTurnParticipationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (suppliedModel is not null)
        {
            return false;
        }

        suppliedModel = model;
        return true;
    }

    /// <summary>Takes an exact one-shot supplied model or the stable fail-closed offline model.</summary>
    public ICharacterTurnParticipationModel TakeModel()
    {
        ICharacterTurnParticipationModel model = suppliedModel ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableParticipationModel : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromException<CharacterTurnParticipationDecision>(
                new InvalidOperationException("Participation decisions are unavailable."));
        }
    }
}
