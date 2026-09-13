using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned participation-round task checks.</summary>
public partial class CharacterTurnParticipationRoundTaskControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_PARTICIPATION_ROUND_TASK_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 300;
    private static readonly TimeSpan OrdinaryDeadline = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan TimeoutDeadline = TimeSpan.FromMilliseconds(5);

    private CharacterTurnParticipationRoundTaskController controller = null!;
    private SceneState originalState = null!;
    private RecordingParticipationModel? completingModel;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<CharacterTurnParticipationRoundTaskController>(
            "../CharacterTurnParticipationRoundTaskController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Participation-round task did not finish within its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSuccessfulRound();
                break;
            case 1:
                ObserveSuccessfulRound();
                break;
            case 2:
                ObserveCancellation();
                break;
            case 3:
                ObserveTimeout();
                break;
            case 4:
                ObserveFailure();
                break;
            case 5:
                ObservePreflightFailure();
                break;
            default:
                Fail("Participation-round task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSuccessfulRound()
    {
        originalState = CaptureSceneState();
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id, sage.Identity.Id);
        completingModel = new RecordingParticipationModel(request => Decision(
            request,
            request.IsDirectlyAddressed
                ? CharacterTurnParticipationKind.Respond
                : CharacterTurnParticipationKind.RemainSilent));

        bool hasClosedSurface = HasClosedSurface();
        bool rejectsInvalidInputs = RejectsInvalidInputs(heardEvent);
        bool beganIdle = controller.Status == CharacterTurnParticipationRoundTaskStatus.Idle
            && controller.Result is null
            && !controller.TryCancel()
            && !controller.TryReset();
        bool started = controller.TryStart(
                completingModel,
                [hermit, sage],
                heardEvent,
                [new CharacterId(sage.Identity.Id.Value)],
                OrdinaryDeadline);
        bool isRunning = controller.Status == CharacterTurnParticipationRoundTaskStatus.Running
            && controller.Result is null;
        bool completedRequests = completingModel.Requests.Count == 2;
        bool rejectedOverlap = !controller.TryStart(
            completingModel,
            [],
            heardEvent,
            [],
            OrdinaryDeadline);
        if (!hasClosedSurface
            || !rejectsInvalidInputs
            || !beganIdle
            || !started
            || !isRunning
            || !completedRequests
            || !rejectedOverlap)
        {
            Fail(
                "Participation-round task rejected its bounded start or overlap contract: "
                + $"surface={hasClosedSurface}; inputs={rejectsInvalidInputs}; "
                + $"idle={beganIdle}; started={started}; running={isRunning}; "
                + $"requests={completingModel.Requests.Count}; overlap={rejectedOverlap}.");
            return;
        }

        stage = 1;
    }

    private void ObserveSuccessfulRound()
    {
        if (controller.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        CharacterTurnParticipationConsiderationRoundResult? result = controller.Result;
        RecordingParticipationModel model = completingModel!;
        if (controller.Status != CharacterTurnParticipationRoundTaskStatus.Completed
            || result is null
            || result.Requests.Count != 2
            || result.Results.Count != 2
            || !result.Requests.Zip(model.Requests)
                .All(pair => ReferenceEquals(pair.First, pair.Second))
            || !result.Results.Zip(model.Decisions)
                .All(pair => ReferenceEquals(pair.First.Decision, pair.Second))
            || controller.TryCancel()
            || controller.TryStart(model, [], CreateEvent(), [], OrdinaryDeadline)
            || !controller.TryReset()
            || controller.Status != CharacterTurnParticipationRoundTaskStatus.Idle
            || controller.Result is not null
            || controller.TryReset())
        {
            Fail("Participation-round task failed exact successful completion or reset.");
            return;
        }

        Character listener = CreateCharacter("cancelled-listener");
        DialogueEvent heardEvent = CreateEvent(listener.Identity.Id);
        if (!controller.TryStart(
                new WaitingParticipationModel(),
                [listener],
                heardEvent,
                [],
                OrdinaryDeadline)
            || !controller.TryCancel()
            || controller.TryCancel())
        {
            Fail("Participation-round task failed bounded caller cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellation()
    {
        if (controller.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        if (controller.Status != CharacterTurnParticipationRoundTaskStatus.Cancelled
            || controller.Result is not null
            || !controller.TryReset())
        {
            Fail("Participation-round task did not retain a safe cancelled outcome.");
            return;
        }

        Character listener = CreateCharacter("timed-out-listener");
        if (!controller.TryStart(
                new WaitingParticipationModel(),
                [listener],
                CreateEvent(listener.Identity.Id),
                [],
                TimeoutDeadline))
        {
            Fail("Participation-round task could not start its timeout scenario.");
            return;
        }

        stage = 3;
    }

    private void ObserveTimeout()
    {
        if (controller.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        if (controller.Status != CharacterTurnParticipationRoundTaskStatus.TimedOut
            || controller.Result is not null
            || !controller.TryReset())
        {
            Fail("Participation-round task did not distinguish its deadline timeout.");
            return;
        }

        Character listener = CreateCharacter("failed-listener");
        if (!controller.TryStart(
                new FailingParticipationModel(),
                [listener],
                CreateEvent(listener.Identity.Id),
                [],
                OrdinaryDeadline))
        {
            Fail("Participation-round task could not start its safe failure scenario.");
            return;
        }

        stage = 4;
    }

    private void ObserveFailure()
    {
        if (controller.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        if (controller.Status != CharacterTurnParticipationRoundTaskStatus.Failed
            || controller.Result is not null
            || !controller.TryReset())
        {
            Fail("Participation-round task exposed the wrong safe failure outcome.");
            return;
        }

        Character heard = CreateCharacter("heard-listener");
        Character absent = CreateCharacter("absent-listener");
        RecordingParticipationModel model = new(request => Decision(
            request,
            CharacterTurnParticipationKind.RemainSilent));
        if (!controller.TryStart(
                model,
                [heard, absent],
                CreateEvent(heard.Identity.Id),
                [],
                OrdinaryDeadline))
        {
            Fail("Participation-round task did not own its preflight failure task.");
            return;
        }

        completingModel = model;
        stage = 5;
    }

    private void ObservePreflightFailure()
    {
        if (controller.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        if (controller.Status != CharacterTurnParticipationRoundTaskStatus.Failed
            || controller.Result is not null
            || completingModel!.Requests.Count != 0
            || !controller.TryReset()
            || !SceneStateIsUnchanged())
        {
            Fail("Participation-round task failed complete preflight or state preservation.");
            return;
        }

        GD.Print(
            "Participation-round task controller smoke passed: exact successful round order, "
            + "main-thread installation, cancellation, timeout, safe failure, reset, complete "
            + "preflight, overlap rejection, closed surface, and unchanged surrounding state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs(DialogueEvent heardEvent)
    {
        ICharacterTurnParticipationModel model = new FailingParticipationModel();
        return Throws<ArgumentNullException>(
                () => controller.TryStart(null!, [], heardEvent, [], OrdinaryDeadline))
            && Throws<ArgumentNullException>(
                () => controller.TryStart(model, null!, heardEvent, [], OrdinaryDeadline))
            && Throws<ArgumentNullException>(
                () => controller.TryStart(model, [], null!, [], OrdinaryDeadline))
            && Throws<ArgumentNullException>(
                () => controller.TryStart(model, [], heardEvent, null!, OrdinaryDeadline))
            && Throws<ArgumentOutOfRangeException>(
                () => controller.TryStart(model, [], heardEvent, [], TimeSpan.Zero))
            && Throws<ArgumentOutOfRangeException>(
                () => controller.TryStart(model, [], heardEvent, [], Timeout.InfiniteTimeSpan));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTurnParticipationRoundTaskController);
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        bool isClosed = properties.SequenceEqual(["Result", "Status"])
            && methods.SequenceEqual(["TryCancel", "TryReset", "TryStart", "_ExitTree", "_Process"])
            && Enum.GetValues<CharacterTurnParticipationRoundTaskStatus>().SequenceEqual(
                [
                    CharacterTurnParticipationRoundTaskStatus.Idle,
                    CharacterTurnParticipationRoundTaskStatus.Running,
                    CharacterTurnParticipationRoundTaskStatus.Completed,
                    CharacterTurnParticipationRoundTaskStatus.Cancelled,
                    CharacterTurnParticipationRoundTaskStatus.TimedOut,
                    CharacterTurnParticipationRoundTaskStatus.Failed,
                ]);
        return isClosed;
    }

    private SceneState CaptureSceneState()
    {
        TemporaryCharacterModelGenerationStatusLabel status = GetNode<TemporaryCharacterModelGenerationStatusLabel>(
            "../DialogueOverlay/Panel/ModelStatus");
        TemporaryCharacterModelStreamTextLabel text = GetNode<TemporaryCharacterModelStreamTextLabel>(
            "../DialogueOverlay/Panel/ModelStreamText");
        WorldEventController world = GetNode<WorldEventController>("../WorldEventController");
        DialogueInputController input = GetNode<DialogueInputController>("../DialogueInputController");
        return new SceneState(
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity,
            GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController").Budget,
            input.LastSubmittedUtterance,
            GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript,
            world.WorldEvents,
            world.EventLog,
            world.WorldTime,
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController").Snapshot,
            status.Presentation,
            text.Presentation,
            status.Visible,
            text.Visible,
            status.Text,
            text.Text);
    }

    private bool SceneStateIsUnchanged()
    {
        SceneState current = CaptureSceneState();
        return ReferenceEquals(current.Turn, originalState.Turn)
            && ReferenceEquals(current.Budget, originalState.Budget)
            && ReferenceEquals(current.SubmittedUtterance, originalState.SubmittedUtterance)
            && ReferenceEquals(current.DialogueEvent, originalState.DialogueEvent)
            && ReferenceEquals(current.Transcript, originalState.Transcript)
            && ReferenceEquals(current.WorldEvents, originalState.WorldEvents)
            && ReferenceEquals(current.EventLog, originalState.EventLog)
            && ReferenceEquals(current.WorldTime, originalState.WorldTime)
            && ReferenceEquals(current.WorldObjects, originalState.WorldObjects)
            && ReferenceEquals(current.ModelStream, originalState.ModelStream)
            && ReferenceEquals(current.ModelStatus, originalState.ModelStatus)
            && ReferenceEquals(current.ModelText, originalState.ModelText)
            && current.StatusVisible == originalState.StatusVisible
            && current.TextVisible == originalState.TextVisible
            && current.StatusText == originalState.StatusText
            && current.Text == originalState.Text;
    }

    private static CharacterTurnParticipationDecision Decision(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationKind kind)
    {
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : request.ScheduledCharacter.Goals.Entries[0];
        return new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            kind,
            motive);
    }

    private static DialogueEvent CreateEvent(params CharacterId[] listeners)
    {
        CharacterId speaker = new("seeker");
        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speaker, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speaker, listeners));
    }

    private static Character CreateCharacter(string id)
    {
        CharacterId characterId = new(id);
        return new Character(
            new CharacterIdentity(characterId, id.ToUpperInvariant()),
            new PublicCharacterBackground("A scheduled participant."),
            new PrivateCharacterBackground("A private scheduled participant."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [new CharacterGoal($"goal-{id}", "Participate for an authored reason.")]));
    }

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
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

    private sealed record SceneState(
        object? Turn,
        object? Budget,
        object? SubmittedUtterance,
        object? DialogueEvent,
        object? Transcript,
        object? WorldEvents,
        object? EventLog,
        object? WorldTime,
        object? WorldObjects,
        object? ModelStream,
        object? ModelStatus,
        object? ModelText,
        bool StatusVisible,
        bool TextVisible,
        string StatusText,
        string Text);
}
