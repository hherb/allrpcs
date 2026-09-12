using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free ordinary player-dialogue participation coordination checks.</summary>
public partial class OrdinaryPlayerDialogueParticipationCoordinatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_ORDINARY_PLAYER_PARTICIPATION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 300;
    private static readonly TimeSpan OrdinaryDeadline = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan TimeoutDeadline = TimeSpan.FromMilliseconds(5);

    private int processFrames;
    private int stage;
    private OrdinaryPlayerDialogueParticipationSmokeScenario? scenario;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Ordinary participation coordination exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartDirectResponse();
                break;
            case 1:
                ObserveDirectResponse();
                break;
            case 2:
                StartSilence();
                break;
            case 3:
                ObserveSilence();
                break;
            case 4:
                StartFailure();
                break;
            case 5:
                ObserveFailure();
                break;
            case 6:
                StartCancellation();
                break;
            case 7:
                ObserveCancellation();
                break;
            case 8:
                StartTimeout();
                break;
            case 9:
                ObserveTimeout();
                break;
            case 10:
                StartInterruption();
                break;
            case 11:
                ObserveInterruption();
                break;
            case 12:
                VerifyPreflightAndSurface();
                break;
            default:
                Fail("Ordinary participation coordination entered an unknown smoke stage.");
                break;
        }
    }

    private void StartDirectResponse()
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario current =
            OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(
                CharacterTurnParticipationKind.Respond);
        SceneState before = CaptureSceneState();
        bool started = current.Coordinator.TryStart(
            current.Event,
            [current.Character],
            [current.DirectAddress],
            current.ModelSource,
            current.RoundController,
            current.BudgetController,
            current.TurnController,
            OrdinaryDeadline);
        if (!started
            || current.Model.Requests.Count != 1
            || current.RoundController.Status != CharacterTurnParticipationRoundTaskStatus.Running
            || current.Coordinator.TryStart(
                current.Event,
                [current.Character],
                [current.DirectAddress],
                current.ModelSource,
                current.RoundController,
                current.BudgetController,
                current.TurnController,
                OrdinaryDeadline)
            || !SceneStateIsUnchanged(before))
        {
            current.Dispose();
            Fail("An exact direct response did not start once without surrounding mutation.");
            return;
        }

        scenario = current;
        stage = 1;
    }

    private void ObserveDirectResponse()
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario current = scenario!;
        ProcessScenario(current);
        if (current.RoundController.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        CharacterTurnActivity? activity = current.TurnController.Activity;
        CharacterTurnParticipationConsiderationRequest request = current.Model.Requests.Single();
        if (current.RoundController.Status != CharacterTurnParticipationRoundTaskStatus.Idle
            || current.RoundController.Result is not null
            || activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(activity.Request.DialogueEvent, current.Event)
            || !ReferenceEquals(
                activity.Request.RespondingCharacterId,
                current.Character.Identity.Id)
            || !ReferenceEquals(request.HeardDialogueEvent, current.Event)
            || !ReferenceEquals(request.ScheduledCharacter, current.Character)
            || !request.IsDirectlyAddressed
            || !ReferenceEquals(current.BudgetController.Budget, current.InitialBudget))
        {
            current.Dispose();
            Fail("A complete direct response did not start its exact budget-gated follow-up.");
            return;
        }

        current.Dispose();
        scenario = null;
        stage = 2;
    }

    private void StartSilence()
    {
        scenario = OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(
            CharacterTurnParticipationKind.RemainSilent);
        StartScenarioOrFail("meaningful silence");
        stage = 3;
    }

    private void ObserveSilence()
    {
        if (ObserveNoTurnTerminal("meaningful silence"))
        {
            stage = 4;
        }
    }

    private void StartFailure()
    {
        scenario = OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(null);
        if (!scenario.Coordinator.TryStart(
                scenario.Event,
                [scenario.Character],
                [scenario.DirectAddress],
                scenario.ModelSource,
                scenario.RoundController,
                scenario.BudgetController,
                scenario.TurnController,
                OrdinaryDeadline))
        {
            Fail("The stable offline participation failure could not start.");
            return;
        }

        stage = 5;
    }

    private void ObserveFailure()
    {
        if (ObserveNoTurnTerminal("offline provider failure"))
        {
            stage = 6;
        }
    }

    private void StartCancellation()
    {
        scenario = OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(
            CharacterTurnParticipationKind.Respond,
            new OrdinaryParticipationWaitingModel());
        StartScenarioOrFail("cancellation");
        if (!scenario!.RoundController.TryCancel())
        {
            Fail("The active participation round could not be cancelled.");
            return;
        }

        stage = 7;
    }

    private void ObserveCancellation()
    {
        if (ObserveNoTurnTerminal("cancellation"))
        {
            stage = 8;
        }
    }

    private void StartTimeout()
    {
        scenario = OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(
            CharacterTurnParticipationKind.Respond,
            new OrdinaryParticipationWaitingModel());
        if (!scenario.Coordinator.TryStart(
                scenario.Event,
                [scenario.Character],
                [scenario.DirectAddress],
                scenario.ModelSource,
                scenario.RoundController,
                scenario.BudgetController,
                scenario.TurnController,
                TimeoutDeadline))
        {
            Fail("The bounded participation timeout could not start.");
            return;
        }

        stage = 9;
    }

    private void ObserveTimeout()
    {
        if (ObserveNoTurnTerminal("timeout"))
        {
            stage = 10;
        }
    }

    private void StartInterruption()
    {
        scenario = OrdinaryPlayerDialogueParticipationPreflightSmoke.CreateScenario(
            CharacterTurnParticipationKind.Respond);
        StartScenarioOrFail("player interruption");
        if (!scenario!.BudgetController.TryInterrupt())
        {
            Fail("The active autonomous budget could not record player interruption.");
            return;
        }

        stage = 11;
    }

    private void ObserveInterruption()
    {
        if (ObserveNoTurnTerminal("player interruption"))
        {
            stage = 12;
        }
    }

    private void VerifyPreflightAndSurface()
    {
        SetProcess(false);
        if (!OrdinaryPlayerDialogueParticipationPreflightSmoke.Passes())
        {
            Fail("Ordinary participation coordination failed preflight or surface checks.");
            return;
        }

        GD.Print(
            "Ordinary player-dialogue participation coordination smoke passed: one exact "
            + "directly addressed LLM listener consumed one model and started one exact "
            + "budget-gated follow-up; silence, failure, cancellation, timeout, interruption, "
            + "exhaustion, ineligible listeners, overlap, and active turns failed closed while "
            + "terminal task reset and surrounding scene state remained unchanged.");
        GetTree().Quit();
    }

    private void StartScenarioOrFail(string description)
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario current = scenario!;
        if (!current.Coordinator.TryStart(
                current.Event,
                [current.Character],
                [current.DirectAddress],
                current.ModelSource,
                current.RoundController,
                current.BudgetController,
                current.TurnController,
                OrdinaryDeadline))
        {
            Fail($"The {description} participation scenario could not start.");
        }
    }

    private bool ObserveNoTurnTerminal(string description)
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario current = scenario!;
        ProcessScenario(current);
        if (current.RoundController.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return false;
        }

        if (current.RoundController.Status != CharacterTurnParticipationRoundTaskStatus.Idle
            || current.RoundController.Result is not null
            || current.TurnController.Activity is not null)
        {
            current.Dispose();
            Fail($"The {description} participation outcome did not reset without a turn.");
            return false;
        }

        current.Dispose();
        scenario = null;
        return true;
    }

    private static void ProcessScenario(OrdinaryPlayerDialogueParticipationSmokeScenario current)
    {
        current.RoundController._Process(0);
        current.Coordinator._Process(0);
    }

    private SceneState CaptureSceneState()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        return new SceneState(
            GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController")
                .Transcript,
            worldEvents.EventLog,
            worldEvents.WorldTime,
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception);
    }

    private bool SceneStateIsUnchanged(SceneState expected)
    {
        SceneState current = CaptureSceneState();
        return ReferenceEquals(current.DialogueEvent, expected.DialogueEvent)
            && ReferenceEquals(current.Transcript, expected.Transcript)
            && ReferenceEquals(current.EventLog, expected.EventLog)
            && ReferenceEquals(current.WorldTime, expected.WorldTime)
            && ReferenceEquals(current.WorldObjects, expected.WorldObjects)
            && ReferenceEquals(current.Perception, expected.Perception);
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed record SceneState(
        object? DialogueEvent,
        object? Transcript,
        object? EventLog,
        object? WorldTime,
        object? WorldObjects,
        object? Perception);

}
