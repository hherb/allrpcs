using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed substitution model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionModelSource : Node
{
    private readonly ICharacterObjectSabotageSubstitutionDecisionModel offlineModel =
        new UnavailableSubstitutionModel();
    private CharacterObjectSabotageSubstitutionModelBinding? binding;
    private ICharacterObjectSabotageSubstitutionDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured substitution-model binding once.</summary>
    public bool TryInitializeBinding(CharacterObjectSabotageSubstitutionModelBinding configured)
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
        CharacterObjectSabotageSubstitutionModelBinding expected,
        CharacterObjectSabotageSubstitutionModelBinding replacement)
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
    public bool TryClearBinding(CharacterObjectSabotageSubstitutionModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

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

    /// <summary>Takes the exact one-shot, configured, or stable fail-closed offline model.</summary>
    public ICharacterObjectSabotageSubstitutionDecisionModel TakeModel()
    {
        ICharacterObjectSabotageSubstitutionDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
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
