using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies direct-response participation only to legacy ordinary-flow smokes.</summary>
public partial class OrdinaryParticipationSmokeModelSupplier : Node
{
    private const string EnabledEnvironmentVariable =
        "ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL";
    private const string EnabledEnvironmentValue = "1";
    private static readonly string[] ExcludedCheckEnvironmentVariables =
    [
        "ALLRPCS_CHARACTER_MODEL_COMPOSITION_CHECK",
        "ALLRPCS_PARTICIPATION_MODEL_SOURCE_CHECK",
        "ALLRPCS_ORDINARY_AUTONOMOUS_FLOW_CHECK",
        "ALLRPCS_ORDINARY_PARTICIPATION_CANCELLATION_CHECK",
    ];

    private readonly DirectResponseModel model = new();
    private TemporaryCharacterTurnParticipationModelSource source = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool enabled = OS.GetEnvironment(EnabledEnvironmentVariable) == EnabledEnvironmentValue
            && !ExcludedCheckEnvironmentVariables.Any(
                variable => !string.IsNullOrEmpty(OS.GetEnvironment(variable)));
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        source = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "../TemporaryCharacterTurnParticipationModelSource");
        if (!source.TrySupplyModel(model))
        {
            throw new InvalidOperationException(
                "The legacy ordinary-flow smoke participation model could not initialize.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = source.TrySupplyModel(model);
    }

    private sealed class DirectResponseModel : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                CharacterTurnParticipationKind.Respond,
                null));
        }
    }
}
