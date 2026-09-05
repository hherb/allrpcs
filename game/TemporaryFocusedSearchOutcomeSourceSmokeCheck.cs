using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary focused-search outcome source checks when explicitly enabled.
/// </summary>
public partial class TemporaryFocusedSearchOutcomeSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FOCUSED_SEARCH_OUTCOME_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryFocusedSearchOutcomeSource source = null!;
    private FocusedSearchActivityController controller = null!;
    private Line2D indicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryFocusedSearchOutcomeSource>(
            "../TemporaryFocusedSearchOutcomeSource");
        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        indicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        bool expectedIndicatorVisibility = indicator.Visible;
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("temporary-outcome-target");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);

        FocusedSearchOutcome first = source.CreateOutcome(request);
        FocusedSearchOutcome second = source.CreateOutcome(request);

        if (ReferenceEquals(first, second)
            || !ReferenceEquals(first.Request, request)
            || !ReferenceEquals(second.Request, request)
            || !ReferenceEquals(first.ResultingVisibilityState, VisibilityState.Identified)
            || !ReferenceEquals(second.ResultingVisibilityState, VisibilityState.Identified)
            || !ReferenceEquals(request.SearcherCharacterId, searcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, targetObjectId))
        {
            Fail("Temporary focused-search outcome source smoke failed exact result construction.");
            return;
        }

        if (!RejectsNullRequest())
        {
            Fail("Temporary focused-search outcome source smoke failed null rejection.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || indicator.Visible != expectedIndicatorVisibility)
        {
            Fail(
                "Temporary focused-search outcome source smoke changed controller state or "
                + "presentation.");
            return;
        }

        GD.Print(
            "Temporary focused-search outcome source smoke passed: exact request and identified "
            + "state retention, distinct repeated results, null rejection, and unchanged "
            + "controller snapshots and presentation were correct.");
        GetTree().Quit();
    }

    private bool RejectsNullRequest()
    {
        try
        {
            source.CreateOutcome(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
