using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene installation of final return travel and party rejoin.</summary>
public partial class CharacterIndependentTaskReturnRejoinControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_RETURN_REJOIN_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterIndependentTaskDepartureController taskController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskOccurrenceController occurrenceController =
            GetNode<CharacterIndependentTaskOccurrenceController>(
                "../CharacterIndependentTaskOccurrenceController");
        CharacterIndependentTaskTravelController outboundController =
            GetNode<CharacterIndependentTaskTravelController>(
                "../CharacterIndependentTaskTravelController");
        CharacterIndependentTaskReturnTravelController returnController =
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "../CharacterIndependentTaskReturnTravelController");
        TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "../TemporaryCharacterIndependentTaskOccurrenceSource");
        TemporaryCharacterIndependentTaskReturnTravelSource returnSource =
            GetNode<TemporaryCharacterIndependentTaskReturnTravelSource>(
                "../TemporaryCharacterIndependentTaskReturnTravelSource");
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
        CharacterIndependentTaskTravelPlan outboundPlan = outboundController.Plan!;
        CharacterIndependentTaskTravelProgress outboundProgress = outboundController.Progress!;
        CharacterIndependentTaskActivitySnapshot activeActivities = taskController.Activities!;
        CharacterPartySnapshot separatedParty = taskController.Party!;
        WorldEventLogSnapshot eventLog = worldController.EventLog!;
        WorldTimeSnapshot worldTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot transcript = transcriptController.Transcript!;
        if (!occurrenceController.TryArriveAndOccur(
                occurrenceSource.CreateOccurrence(active),
                outboundPlan,
                outboundProgress,
                outboundPlan.Waypoints.Single(),
                activeActivities,
                eventLog,
                worldTime,
                transcript)
            || occurrenceController.LastArrivalOccurrenceResult is not { } arrival
            || taskController.Activities is not { } completedActivities
            || completedActivities.Activities.Single() is not { } completedActivity
            || returnController.Plan is not { } returnPlan
            || returnController.Progress is not { } initialReturnProgress)
        {
            Fail("The authored task did not reach its exact initialized return boundary.");
            return;
        }

        CharacterPartySnapshot staleParty = new(separatedParty.MemberCharacterIds);
        CharacterIndependentTaskReturnTravelPlan distinctPlan =
            returnSource.CreatePlan(arrival, completedActivity);
        CharacterIndependentTaskReturnTravelProgress distinctProgress = new(returnPlan);
        if (returnController.TryArriveAndRejoin(
                returnPlan,
                initialReturnProgress,
                returnPlan.Waypoints.Single(),
                staleParty)
            || returnController.TryArriveAndRejoin(
                distinctPlan,
                initialReturnProgress,
                distinctPlan.Waypoints.Single(),
                separatedParty)
            || returnController.TryArriveAndRejoin(
                returnPlan,
                distinctProgress,
                returnPlan.Waypoints.Single(),
                separatedParty)
            || !HasExactState(
                taskController,
                returnController,
                completedActivities,
                separatedParty,
                returnPlan,
                initialReturnProgress))
        {
            Fail("Stale or mismatched return input changed scene-owned party state.");
            return;
        }

        if (!returnController.TryArriveAndRejoin(
                returnPlan,
                initialReturnProgress,
                returnPlan.Waypoints.Single(),
                separatedParty)
            || returnController.Progress is not { IsComplete: true } completedReturnProgress
            || ReferenceEquals(completedReturnProgress, initialReturnProgress)
            || !ReferenceEquals(completedReturnProgress.Plan, returnPlan)
            || taskController.Party is not { } rejoinedParty
            || ReferenceEquals(rejoinedParty, separatedParty)
            || rejoinedParty.MemberCharacterIds.Count != separatedParty.MemberCharacterIds.Count + 1
            || !ReferenceEquals(
                rejoinedParty.MemberCharacterIds[^1],
                completedActivity.Assignment.CharacterId)
            || !separatedParty.MemberCharacterIds.SequenceEqual(
                rejoinedParty.MemberCharacterIds.Take(separatedParty.MemberCharacterIds.Count))
            || !ReferenceEquals(taskController.Activities, completedActivities)
            || taskController.ActiveActivity is not null)
        {
            Fail("The exact final return did not atomically rejoin its completed task character.");
            return;
        }

        if (returnController.TryArriveAndRejoin(
                returnPlan,
                completedReturnProgress,
                returnPlan.Waypoints.Single(),
                rejoinedParty)
            || !HasExactState(
                taskController,
                returnController,
                completedActivities,
                rejoinedParty,
                returnPlan,
                completedReturnProgress)
            || !RejectsNulls(
                returnController,
                returnPlan,
                completedReturnProgress,
                rejoinedParty))
        {
            Fail("Repeated or null return input did not preserve rejoined scene state.");
            return;
        }

        GD.Print(
            "Independent-task return-rejoin controller smoke passed: exact final return progress "
            + "and stable bounded party membership installed atomically, while stale, mismatched, "
            + "repeated, and null inputs preserved every scene-owned snapshot.");
        GetTree().Quit();
    }

    private static bool HasExactState(
        CharacterIndependentTaskDepartureController taskController,
        CharacterIndependentTaskReturnTravelController returnController,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress) =>
        ReferenceEquals(taskController.Activities, activities)
        && ReferenceEquals(taskController.Party, party)
        && taskController.ActiveActivity is null
        && ReferenceEquals(returnController.Plan, plan)
        && ReferenceEquals(returnController.Progress, progress);

    private static bool RejectsNulls(
        CharacterIndependentTaskReturnTravelController controller,
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress,
        CharacterPartySnapshot party) =>
        ThrowsArgumentNull(() => controller.TryArriveAndRejoin(
            null!, progress, plan.Waypoints[0], party))
        && ThrowsArgumentNull(() => controller.TryArriveAndRejoin(
            plan, null!, plan.Waypoints[0], party))
        && ThrowsArgumentNull(() => controller.TryArriveAndRejoin(
            plan, progress, plan.Waypoints[0], null!));

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
