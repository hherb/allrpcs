using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact one-shot and fail-closed offline participation-model supply.</summary>
public partial class TemporaryCharacterTurnParticipationModelSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_PARTICIPATION_MODEL_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(5);

    private CharacterTurnParticipationRoundTaskController taskController = null!;
    private TemporaryCharacterTurnParticipationModelSource source = null!;
    private SceneState originalState = null!;
    private int processFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "../TemporaryCharacterTurnParticipationModelSource");
        taskController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "../CharacterTurnParticipationRoundTaskController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Participation-model source did not finish within its bounded smoke duration.");
            return;
        }

        if (taskController.Status == CharacterTurnParticipationRoundTaskStatus.Idle)
        {
            StartOfflineFailure();
            return;
        }

        if (taskController.Status == CharacterTurnParticipationRoundTaskStatus.Running)
        {
            return;
        }

        ObserveOfflineFailure();
    }

    private void StartOfflineFailure()
    {
        originalState = CaptureSceneState();
        RecordingParticipationModel first = new(request => new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null));
        RecordingParticipationModel rejected = new(request => new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null));
        bool rejectsNull = ThrowsArgumentNull(() => source.TrySupplyModel(null!));
        bool supplied = source.TrySupplyModel(first);
        bool rejectedOverlap = !source.TrySupplyModel(rejected);
        ICharacterTurnParticipationModel taken = source.TakeModel();
        ICharacterTurnParticipationModel offline = source.TakeModel();

        Character listener = CreateCharacter();
        if (!HasClosedSurface()
            || !rejectsNull
            || !supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || ReferenceEquals(offline, first)
            || !ReferenceEquals(source.TakeModel(), offline)
            || !taskController.TryStart(
                offline,
                [listener],
                CreateEvent(listener.Identity.Id),
                [listener.Identity.Id],
                Deadline))
        {
            Fail("Participation-model source failed exact one-shot or stable offline selection.");
        }
    }

    private void ObserveOfflineFailure()
    {
        if (taskController.Status != CharacterTurnParticipationRoundTaskStatus.Failed
            || taskController.Result is not null
            || !taskController.TryReset()
            || !SceneStateIsUnchanged())
        {
            Fail("Offline participation model did not fail closed without scene mutation.");
            return;
        }

        GD.Print(
            "Temporary participation-model source smoke passed: one exact one-shot model, "
            + "overlap and null rejection, one stable fail-closed offline model, bounded task "
            + "failure, closed surface, and unchanged surrounding scene state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterTurnParticipationModelSource);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        return type.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly).Length == 0
            && methods.SequenceEqual(["TakeModel", "TrySupplyModel"]);
    }

    private SceneState CaptureSceneState()
    {
        WorldEventController world = GetNode<WorldEventController>("../WorldEventController");
        return new SceneState(
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity,
            GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController").Budget,
            GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript,
            world.EventLog,
            world.WorldTime,
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController").Snapshot);
    }

    private bool SceneStateIsUnchanged()
    {
        SceneState current = CaptureSceneState();
        return ReferenceEquals(current.Turn, originalState.Turn)
            && ReferenceEquals(current.Budget, originalState.Budget)
            && ReferenceEquals(current.DialogueEvent, originalState.DialogueEvent)
            && ReferenceEquals(current.Transcript, originalState.Transcript)
            && ReferenceEquals(current.EventLog, originalState.EventLog)
            && ReferenceEquals(current.WorldTime, originalState.WorldTime)
            && ReferenceEquals(current.WorldObjects, originalState.WorldObjects)
            && ReferenceEquals(current.ModelStream, originalState.ModelStream);
    }

    private static Character CreateCharacter()
    {
        CharacterId id = new("village-sage");
        return new Character(
            new CharacterIdentity(id, "Village Sage"),
            new PublicCharacterBackground("A scheduled participant."),
            new PrivateCharacterBackground("A private scheduled participant."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal("compare-evidence", "Compare the evidence.")]));
    }

    private static DialogueEvent CreateEvent(CharacterId listener)
    {
        CharacterId speaker = new("seeker");
        return new DialogueEvent(
            new DialogueUtterance(speaker, "What follows from the evidence?"),
            new DialogueAudienceSnapshot(speaker, [listener]));
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
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

    private sealed record SceneState(
        object? Turn,
        object? Budget,
        object? DialogueEvent,
        object? Transcript,
        object? EventLog,
        object? WorldTime,
        object? WorldObjects,
        object? ModelStream);
}
