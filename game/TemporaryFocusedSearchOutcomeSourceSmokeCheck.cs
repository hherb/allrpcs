using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
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
    private TemporaryWorldObjectSource worldObjectSource = null!;
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
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
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
        WorldObject exactWorldObject = worldObjectSource.WorldObjects.Entries[0];
        ObjectId targetObjectId = exactWorldObject.Identity.Id;
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        WorldObjectIdentity exactIdentity = exactWorldObject.Identity;
        WorldObjectDescription exactDescription = exactWorldObject.Description;
        ObjectId valueEqualObjectId = new(targetObjectId.Value);
        WorldObjectIdentity valueEqualIdentity = new(valueEqualObjectId, "Value-equal target");
        WorldObjectDescription valueEqualDescription = new("Retained description.");
        WorldObject valueEqualWorldObject = new(valueEqualIdentity, valueEqualDescription);
        WorldObject mismatchedWorldObject = worldObjectSource.WorldObjects.Entries[1];
        WorldObjectIdentity mismatchedIdentity = mismatchedWorldObject.Identity;
        WorldObjectDescription mismatchedDescription = mismatchedWorldObject.Description;

        FocusedSearchOutcome? first = source.CreateOutcome(request, exactWorldObject);
        FocusedSearchOutcome? second = source.CreateOutcome(request, valueEqualWorldObject);

        if (first is null
            || second is null
            || ReferenceEquals(first, second)
            || !ReferenceEquals(first.Request, request)
            || !ReferenceEquals(second.Request, request)
            || !ReferenceEquals(first.ResultingVisibilityState, VisibilityState.Identified)
            || !ReferenceEquals(second.ResultingVisibilityState, VisibilityState.Identified)
            || !ReferenceEquals(request.SearcherCharacterId, searcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, targetObjectId)
            || ReferenceEquals(valueEqualObjectId, targetObjectId)
            || valueEqualObjectId != targetObjectId)
        {
            Fail("Temporary focused-search outcome source smoke failed exact result construction.");
            return;
        }

        if (source.CreateOutcome(request, null) is not null
            || source.CreateOutcome(request, mismatchedWorldObject) is not null)
        {
            Fail("Temporary focused-search outcome source smoke failed closed object gating.");
            return;
        }

        if (!RejectsNullRequest(exactWorldObject))
        {
            Fail("Temporary focused-search outcome source smoke failed null rejection.");
            return;
        }

        if (!ReferenceEquals(exactWorldObject.Identity, exactIdentity)
            || !ReferenceEquals(exactWorldObject.Description, exactDescription)
            || !ReferenceEquals(valueEqualWorldObject.Identity, valueEqualIdentity)
            || !ReferenceEquals(valueEqualWorldObject.Description, valueEqualDescription)
            || !ReferenceEquals(valueEqualIdentity.Id, valueEqualObjectId)
            || !ReferenceEquals(mismatchedWorldObject.Identity, mismatchedIdentity)
            || !ReferenceEquals(mismatchedWorldObject.Description, mismatchedDescription)
            || !ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || indicator.Visible != expectedIndicatorVisibility)
        {
            Fail(
                "Temporary focused-search outcome source smoke changed controller state or "
                + "presentation.");
            return;
        }

        GD.Print(
            "Temporary focused-search outcome source smoke passed: exact and value-equal "
            + "world-object gating, exact request and identified-state retention, distinct "
            + "results, null and mismatch rejection, unchanged nested object inputs, and "
            + "unchanged controller snapshots and presentation were correct.");
        GetTree().Quit();
    }

    private bool RejectsNullRequest(WorldObject worldObject)
    {
        try
        {
            source.CreateOutcome(null!, worldObject);
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
