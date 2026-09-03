using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search activity controller checks when explicitly enabled.
/// </summary>
public partial class FocusedSearchActivityControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FOCUSED_SEARCH_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

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

        FocusedSearchActivitySnapshot initialSnapshot = controller.Activities;
        if (controller.SearcherCharacterId.Value != "seeker"
            || !AssertState(initialSnapshot, expectedVisible: false, "empty initial state"))
        {
            return;
        }

        CharacterId unrelatedCharacterId = new("healer");
        ObjectId firstTargetId = new("concealed-herb");
        ObjectId secondTargetId = new("partial-inscription");
        ObjectId unrelatedTargetId = new("residue");
        FocusedSearchRequest firstRequest = new(
            controller.SearcherCharacterId,
            firstTargetId);
        FocusedSearchRequest secondRequest = new(
            controller.SearcherCharacterId,
            secondTargetId);
        FocusedSearchRequest unrelatedRequest = new(
            unrelatedCharacterId,
            unrelatedTargetId);

        if (controller.TryStartSearch(unrelatedRequest)
            || !AssertUnchanged(initialSnapshot, [], expectedVisible: false, "unrelated start"))
        {
            return;
        }

        if (!controller.TryStartSearch(firstRequest))
        {
            Fail("Focused-search controller smoke failed eligible start.");
            return;
        }

        FocusedSearchActivitySnapshot startedSnapshot = controller.Activities;
        FocusedSearchActivity firstActivity = startedSnapshot.Entries.Single();
        if (!ReferenceEquals(firstActivity.Request, firstRequest)
            || !AssertState(startedSnapshot, expectedVisible: true, "successful start"))
        {
            return;
        }

        if (controller.TryStartSearch(secondRequest)
            || !AssertUnchanged(
                startedSnapshot,
                [firstActivity],
                expectedVisible: true,
                "ineligible second start")
            || controller.TryCancelSearch(secondRequest)
            || !AssertUnchanged(
                startedSnapshot,
                [firstActivity],
                expectedVisible: true,
                "failed cancellation"))
        {
            return;
        }

        if (!controller.TryCancelSearch(firstRequest)
            || !AssertState(
                controller.Activities,
                expectedVisible: false,
                "successful cancellation"))
        {
            return;
        }

        if (!controller.TryStartSearch(secondRequest))
        {
            Fail("Focused-search controller smoke failed completion-path start.");
            return;
        }

        FocusedSearchActivitySnapshot completionSnapshot = controller.Activities;
        FocusedSearchActivity secondActivity = completionSnapshot.Entries.Single();
        if (!ReferenceEquals(secondActivity.Request, secondRequest)
            || controller.TryCompleteSearch(firstRequest)
            || !AssertUnchanged(
                completionSnapshot,
                [secondActivity],
                expectedVisible: true,
                "failed completion")
            || !controller.TryCompleteSearch(secondRequest)
            || !AssertState(
                controller.Activities,
                expectedVisible: false,
                "successful completion")
            || !AssertInputsUnchanged(
                unrelatedCharacterId,
                firstTargetId,
                secondTargetId,
                unrelatedTargetId,
                firstRequest,
                secondRequest,
                unrelatedRequest))
        {
            return;
        }

        GD.Print(
            "Focused-search activity controller smoke passed: empty initialization, start, "
            + "fail-closed unrelated and ineligible operations, cancellation, completion, "
            + "presentation, exact instance retention, and unchanged inputs were correct.");
        GetTree().Quit();
    }

    private bool AssertUnchanged(
        FocusedSearchActivitySnapshot expectedSnapshot,
        FocusedSearchActivity[] expectedActivities,
        bool expectedVisible,
        string scenario)
    {
        if (!ReferenceEquals(controller.Activities, expectedSnapshot)
            || controller.Activities.Entries.Count != expectedActivities.Length
            || !controller.Activities.Entries
                .Zip(expectedActivities)
                .All(pair => ReferenceEquals(pair.First, pair.Second)))
        {
            Fail($"Focused-search controller smoke failed {scenario}: state changed.");
            return false;
        }

        return AssertState(expectedSnapshot, expectedVisible, scenario);
    }

    private bool AssertState(
        FocusedSearchActivitySnapshot expectedSnapshot,
        bool expectedVisible,
        string scenario)
    {
        if (!ReferenceEquals(controller.Activities, expectedSnapshot))
        {
            Fail($"Focused-search controller smoke failed {scenario}: snapshot mismatch.");
            return false;
        }

        if (indicator.Visible != expectedVisible)
        {
            Fail(
                $"Focused-search controller smoke failed {scenario}: expected indicator "
                + $"visibility {expectedVisible}, received {indicator.Visible}.");
            return false;
        }

        return true;
    }

    private bool AssertInputsUnchanged(
        CharacterId unrelatedCharacterId,
        ObjectId firstTargetId,
        ObjectId secondTargetId,
        ObjectId unrelatedTargetId,
        FocusedSearchRequest firstRequest,
        FocusedSearchRequest secondRequest,
        FocusedSearchRequest unrelatedRequest)
    {
        bool unchanged = ReferenceEquals(
                firstRequest.SearcherCharacterId,
                controller.SearcherCharacterId)
            && ReferenceEquals(firstRequest.TargetObjectId, firstTargetId)
            && ReferenceEquals(secondRequest.SearcherCharacterId, controller.SearcherCharacterId)
            && ReferenceEquals(secondRequest.TargetObjectId, secondTargetId)
            && ReferenceEquals(unrelatedRequest.SearcherCharacterId, unrelatedCharacterId)
            && ReferenceEquals(unrelatedRequest.TargetObjectId, unrelatedTargetId);
        if (!unchanged)
        {
            Fail("Focused-search activity controller changed a supplied request or identifier.");
        }

        return unchanged;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
