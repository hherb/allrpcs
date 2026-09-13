using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact configured, one-shot, and offline participation-model supply.</summary>
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
        ICharacterTurnParticipationModel offline = source.TakeModel();
        bool stableOffline = ReferenceEquals(source.TakeModel(), offline);
        CharacterModelSettings openAiSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot selection = new(
            [openAiSettings, anthropicSettings],
            CharacterModelProvider.OpenAi);
        RecordingParticipationModel openAi = new(request => new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null));
        RecordingParticipationModel anthropic = new(request => new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null));
        CharacterTurnParticipationModelBinding openAiBinding = new(
            CharacterModelProvider.OpenAi,
            openAiSettings,
            openAi);
        CharacterTurnParticipationModelBinding anthropicBinding = new(
            CharacterModelProvider.Anthropic,
            anthropicSettings,
            anthropic);
        CharacterTurnParticipationModelBindingSnapshot bindings = new(
            [openAiBinding, anthropicBinding]);
        bool initialized = source.TryInitializeBindings(selection, bindings);
        bool rejectedRepeatedInitialization = !source.TryInitializeBindings(selection, bindings);
        bool selectedInitial = ReferenceEquals(source.TakeModel(), openAi)
            && ReferenceEquals(source.TakeModel(), openAi);
        bool selectedAnthropic = source.TrySelectProvider(CharacterModelProvider.Anthropic)
            && ReferenceEquals(source.TakeModel(), anthropic);
        bool rejectedUnavailable = !source.TrySelectProvider(CharacterModelProvider.Ollama)
            && ReferenceEquals(source.TakeModel(), anthropic);

        CharacterModelSettings replacementSettings = new(
            CharacterModelProvider.OpenAi,
            "replacement-test");
        CharacterModelSettingsSelectionSnapshot replacementSelection = new(
            [replacementSettings],
            CharacterModelProvider.OpenAi);
        RecordingParticipationModel replacement = new(request =>
            new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                CharacterTurnParticipationKind.Respond,
                null));
        CharacterTurnParticipationModelBinding mismatchedReplacement = new(
            CharacterModelProvider.OpenAi,
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "replacement-test"),
            replacement);
        CharacterTurnParticipationModelBinding exactReplacement = new(
            CharacterModelProvider.OpenAi,
            replacementSettings,
            replacement);
        bool rejectedMismatch = !source.TryReplaceBindings(
                replacementSelection,
                new CharacterTurnParticipationModelBindingSnapshot([mismatchedReplacement]))
            && ReferenceEquals(source.TakeModel(), anthropic);

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
        bool replaced = source.TryReplaceBindings(
            replacementSelection,
            new CharacterTurnParticipationModelBindingSnapshot([exactReplacement]));
        bool replacementIsStable = ReferenceEquals(source.TakeModel(), replacement)
            && ReferenceEquals(source.TakeModel(), replacement);
        bool resupplied = source.TrySupplyModel(first);
        bool cleared = source.TryClearBindings();
        bool rejectedRepeatedClear = !source.TryClearBindings();
        bool clearPreservedOneShot = ReferenceEquals(source.TakeModel(), first)
            && ReferenceEquals(source.TakeModel(), offline);

        Character listener = CreateCharacter();
        if (!HasClosedSurface()
            || !stableOffline
            || !initialized
            || !rejectedRepeatedInitialization
            || !selectedInitial
            || !selectedAnthropic
            || !rejectedUnavailable
            || !rejectedMismatch
            || !rejectsNull
            || !supplied
            || !rejectedOverlap
            || !ReferenceEquals(taken, first)
            || !replaced
            || !replacementIsStable
            || !resupplied
            || !cleared
            || !rejectedRepeatedClear
            || !clearPreservedOneShot
            || openAi.CallCount != 0
            || anthropic.CallCount != 0
            || replacement.CallCount != 0
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
            "Temporary participation-model source smoke passed: exact initialized, selected, and "
            + "replaced bindings, one exact one-shot model, one stable fail-closed offline model, "
            + "bounded task failure, closed surface, and unchanged surrounding scene state.");
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
            && methods.SequenceEqual(
                [
                    "TakeModel",
                    "TryClearBindings",
                    "TryInitializeBindings",
                    "TryReplaceBindings",
                    "TrySelectProvider",
                    "TrySupplyModel",
                ]);
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
            new DialogueDelivery(
                new DialogueUtterance(speaker, "What follows from the evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
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

    private sealed class RecordingParticipationModel(
        Func<CharacterTurnParticipationConsiderationRequest, CharacterTurnParticipationDecision>
            decide)
        : ICharacterTurnParticipationModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return ValueTask.FromResult(decide(request));
        }
    }
}
