using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies one caller-composed concealment model or one stable offline default.</summary>
public partial class TemporaryCharacterObjectSabotageConcealmentModelSource : Node
{
    private readonly ICharacterObjectSabotageConcealmentDecisionModel offlineModel =
        new UnavailableConcealmentModel();
    private CharacterObjectSabotageConcealmentModelBinding? binding;
    private ICharacterObjectSabotageConcealmentDecisionModel? suppliedModel;

    /// <summary>Retains one exact configured concealment-model binding once.</summary>
    public bool TryInitializeBinding(CharacterObjectSabotageConcealmentModelBinding configured)
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
        CharacterObjectSabotageConcealmentModelBinding expected,
        CharacterObjectSabotageConcealmentModelBinding replacement)
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
    public bool TryClearBinding(CharacterObjectSabotageConcealmentModelBinding expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (!ReferenceEquals(binding, expected))
        {
            return false;
        }

        binding = null;
        return true;
    }

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

    /// <summary>Takes the exact one-shot, configured, or stable fail-closed offline model.</summary>
    public ICharacterObjectSabotageConcealmentDecisionModel TakeModel()
    {
        ICharacterObjectSabotageConcealmentDecisionModel model =
            suppliedModel ?? binding?.Model ?? offlineModel;
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
