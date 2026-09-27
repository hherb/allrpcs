using Allrpcs.Application.Investigations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Consumes one exact scene-accepted route completion without gameplay effects.</summary>
public partial class OpeningRegionInvestigationRouteCompletionConsumptionCoordinator : Node
{
    private OpeningRegionInvestigationRouteCompletionCoordinator completions = null!;

    /// <summary>Gets the exact consumed completion, or null before explicit consumption.</summary>
    public OpeningRegionInvestigationRouteCompletion? ConsumedCompletion { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        completions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
    }

    /// <summary>Consumes one exact token retained by the scene completion owner once.</summary>
    public bool TryConsume(OpeningRegionInvestigationRouteCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        if (ConsumedCompletion is not null
            || !completions.AcceptedCompletions.Any(accepted =>
                ReferenceEquals(accepted, completion)))
        {
            return false;
        }

        ConsumedCompletion = completion;
        return true;
    }
}
