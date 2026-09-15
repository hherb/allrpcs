using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed substitution model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionModelSource : Node
{
    private readonly ICharacterObjectSabotageSubstitutionDecisionModel offlineModel =
        new UnavailableSubstitutionModel();
    private ICharacterObjectSabotageSubstitutionDecisionModel? suppliedModel;

    /// <summary>Retains one exact substitution model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterObjectSabotageSubstitutionDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (suppliedModel is not null)
        {
            return false;
        }

        suppliedModel = model;
        return true;
    }

    /// <summary>Takes the exact one-shot or stable fail-closed offline model.</summary>
    public ICharacterObjectSabotageSubstitutionDecisionModel TakeModel()
    {
        ICharacterObjectSabotageSubstitutionDecisionModel model = suppliedModel ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableSubstitutionModel
        : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterObjectSabotageSubstitutionDecision>(
                new InvalidOperationException("Substitution decisions are unavailable."));
        }
    }
}
