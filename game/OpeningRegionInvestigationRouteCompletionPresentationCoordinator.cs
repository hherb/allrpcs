using Allrpcs.Application.Investigations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds and acknowledges the first exact accepted route completion once.</summary>
public partial class OpeningRegionInvestigationRouteCompletionPresentationCoordinator : Node
{
    private OpeningRegionInvestigationRouteCompletion? boundCompletion;
    private OpeningRegionInvestigationRouteCompletionCoordinator completions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = null!;

    /// <summary>Gets the bounded safe completion presentation awaiting acknowledgement.</summary>
    public OpeningRegionInvestigationRouteCompletionPresentationSnapshot? Presentation
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        completions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
        consumption = GetNode<OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (boundCompletion is not null)
        {
            if (!IsFirstAccepted(boundCompletion))
            {
                boundCompletion = null;
                Presentation = null;
            }

            return;
        }

        if (completions.AcceptedCompletions.Count != 0)
        {
            _ = TryPresent(completions.AcceptedCompletions[0]);
        }
    }

    /// <summary>Projects only the exact first accepted token while no completion is consumed.</summary>
    public bool TryPresent(OpeningRegionInvestigationRouteCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        if (boundCompletion is not null
            || Presentation is not null
            || consumption.ConsumedCompletion is not null
            || !IsFirstAccepted(completion))
        {
            return false;
        }

        try
        {
            Presentation =
                OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(completion);
            boundCompletion = completion;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Consumes the exact bound token before closing its exact safe presentation.</summary>
    public bool TryAcknowledge(
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        OpeningRegionInvestigationRouteCompletion? completion = boundCompletion;
        if (completion is null
            || !ReferenceEquals(Presentation, presentation)
            || !IsFirstAccepted(completion)
            || !consumption.TryConsume(completion))
        {
            return false;
        }

        Presentation = null;
        boundCompletion = null;
        return true;
    }

    private bool IsFirstAccepted(OpeningRegionInvestigationRouteCompletion completion) =>
        completions.AcceptedCompletions.Count != 0
        && ReferenceEquals(completions.AcceptedCompletions[0], completion);
}
