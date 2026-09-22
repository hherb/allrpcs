using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed sabotage-action model or one stable offline default.</summary>
public partial class TemporaryCharacterSabotageActionModelSource : Node
{
    private readonly ICharacterSabotageActionDecisionModel offlineModel =
        new UnavailableSabotageActionModel();
    private CharacterSabotageActionModelBinding? binding;
    private ICharacterSabotageActionDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured sabotage-action model binding once.</summary>
    public bool TryInitializeBinding(CharacterSabotageActionModelBinding configured)
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
        CharacterSabotageActionModelBinding expected,
        CharacterSabotageActionModelBinding replacement)
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
    public bool TryClearBinding(CharacterSabotageActionModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

    /// <summary>Retains one exact sabotage-action model for the next scene decision only.</summary>
    public bool TrySupplyModel(ICharacterSabotageActionDecisionModel model)
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
    public ICharacterSabotageActionDecisionModel TakeModel()
    {
        ICharacterSabotageActionDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
        suppliedModel = null;
        return model;
    }

    private sealed class UnavailableSabotageActionModel : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromException<CharacterSabotageActionDecision>(
                new InvalidOperationException("Sabotage-action decisions are unavailable."));
        }
    }
}
