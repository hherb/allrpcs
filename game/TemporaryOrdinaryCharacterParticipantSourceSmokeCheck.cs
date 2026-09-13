using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the stable ordinary LLM participant roster without changing scene state.</summary>
public partial class TemporaryOrdinaryCharacterParticipantSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_ORDINARY_CHARACTER_PARTICIPANT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

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
        SetProcess(false);

        TemporaryOrdinaryCharacterParticipantSource source =
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "../TemporaryOrdinaryCharacterParticipantSource");
        TemporaryHealerCharacterSource healerSource = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource");
        SceneState before = CaptureSceneState();
        IReadOnlyList<Character> first = source.Participants;
        IReadOnlyList<Character> second = source.Participants;

        if (!HasExpectedSurface()
            || !ReferenceEquals(first, second)
            || first.Count != 2
            || !ReferenceEquals(first[0], healerSource.Character)
            || ReferenceEquals(first[1], healerSource.Character)
            || !string.Equals(first[1].Identity.Id.Value, "village-sage", StringComparison.Ordinal)
            || first[0].Identity.Id.Equals(first[1].Identity.Id)
            || !ReferenceEquals(
                first[0].ControllerAssignment.CharacterId,
                first[0].Identity.Id)
            || !ReferenceEquals(
                first[1].ControllerAssignment.CharacterId,
                first[1].Identity.Id)
            || !ReferenceEquals(first[0].ControllerAssignment.ControllerType, ControllerType.Llm)
            || !ReferenceEquals(first[1].ControllerAssignment.ControllerType, ControllerType.Llm))
        {
            Fail("The ordinary participant source did not retain the exact stable healer-first roster.");
            return;
        }

        try
        {
            ((IList<Character>)first).Add(RepresentativeSageCharacterSource.Create());
            Fail("The ordinary participant source exposed a mutable roster.");
            return;
        }
        catch (NotSupportedException)
        {
        }

        CharacterTurnActivity awaitingSage = CreateActivity(
            new CharacterId(first[1].Identity.Id.Value),
            CharacterTurnActivityState.AwaitingResponse);
        Character? resolved = CharacterTurnParticipantLookup.Find(first, awaitingSage);
        Character? absent = CharacterTurnParticipantLookup.Find(
            first,
            CreateActivity(new CharacterId("absent"), CharacterTurnActivityState.AwaitingResponse));
        Character? nonAwaiting = CharacterTurnParticipantLookup.Find(
            first,
            CreateActivity(first[0].Identity.Id, CharacterTurnActivityState.Cancelled));
        SceneState after = CaptureSceneState();

        if (!ReferenceEquals(resolved, first[1])
            || ReferenceEquals(awaitingSage.Request.RespondingCharacterId, first[1].Identity.Id)
            || !awaitingSage.Request.RespondingCharacterId.Equals(first[1].Identity.Id)
            || absent is not null
            || nonAwaiting is not null)
        {
            Fail("The ordinary participant roster did not resolve exact awaiting turns fail closed.");
            return;
        }

        if (!before.Equals(after))
        {
            Fail("The ordinary participant roster changed dialogue, world, model, or presentation state.");
            return;
        }

        GD.Print(
            "Temporary ordinary participant source smoke passed: one stable read-only healer-first "
            + "LLM roster retained the exact scene healer and one distinct fresh representative sage; "
            + "value-equal awaiting-turn lookup resolved the exact sage; absent and non-awaiting turns "
            + "returned no result; the source surface stayed closed; and dialogue, world, perception, "
            + "model, budget, and presentation state remained unchanged.");
        GetTree().Quit();
    }

    private SceneState CaptureSceneState()
    {
        CharacterTurnActivityController turnController =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterModelGenerationFlowController modelFlow =
            GetNode<CharacterModelGenerationFlowController>("../CharacterModelGenerationFlowController");
        CharacterTurnParticipationRoundTaskController roundController =
            GetNode<CharacterTurnParticipationRoundTaskController>(
                "../CharacterTurnParticipationRoundTaskController");
        FocusedSearchActivityController focusedSearch =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");

        return new SceneState(
            GetNode<DialogueInputController>("../DialogueInputController").LastSubmittedUtterance,
            GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript,
            turnController.Activity,
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            worldEvents.WorldEvents,
            worldEvents.EventLog,
            worldEvents.WorldTime,
            focusedSearch.Activities,
            focusedSearch.Outcomes,
            focusedSearch.Perception,
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController").Snapshot,
            modelFlow.HasStarted,
            modelFlow.Result,
            roundController.Status,
            roundController.Result,
            GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController").Budget,
            GetNode<Label>("../DialogueOverlay/Panel/ModelStatus").Text,
            GetNode<Label>("../DialogueOverlay/Panel/ModelStreamText").Text,
            GetNode<Label>("../CommandOverlay/Panel/ModeStatus").Text,
            GetNode<Label>("../CommandOverlay/Panel/FocusStatus").Text);
    }

    private static CharacterTurnActivity CreateActivity(
        CharacterId respondingCharacterId,
        CharacterTurnActivityState state)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "What should we conclude?");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [respondingCharacterId]));
        CharacterTurnRequest request = new(
            respondingCharacterId,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Question));
        return new CharacterTurnActivity(request, null, state);
    }

    private static bool HasExpectedSurface()
    {
        Type type = typeof(TemporaryOrdinaryCharacterParticipantSource);
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.MethodInfo method = AssertSingle(type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(candidate => !candidate.IsSpecialName));

        return property.Name == nameof(TemporaryOrdinaryCharacterParticipantSource.Participants)
            && property.PropertyType == typeof(IReadOnlyList<Character>)
            && property.CanRead
            && property.SetMethod is { IsPrivate: true }
            && method.Name == nameof(_Ready)
            && method.ReturnType == typeof(void)
            && method.GetParameters().Length == 0;
    }

    private static T AssertSingle<T>(IEnumerable<T> values)
    {
        T[] retained = values.ToArray();
        return retained.Length == 1
            ? retained[0]
            : throw new InvalidOperationException("The source did not expose exactly one member.");
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private readonly record struct SceneState(
        DialogueUtterance? LastSubmittedUtterance,
        DialogueEvent? LastOccurredEvent,
        CharacterTurnTranscriptSnapshot? Transcript,
        CharacterTurnActivity? Turn,
        WorldObjectSnapshot? WorldObjects,
        WorldEventSnapshot? WorldEvents,
        WorldEventLogSnapshot? EventLog,
        WorldTimeSnapshot? WorldTime,
        FocusedSearchActivitySnapshot FocusedSearchActivities,
        FocusedSearchOutcomeSnapshot FocusedSearchOutcomes,
        CharacterPerceptionSnapshot? Perception,
        CharacterModelStreamSnapshot? ModelStream,
        bool HasModelFlowStarted,
        CharacterModelStreamSnapshot? ModelResult,
        CharacterTurnParticipationRoundTaskStatus RoundStatus,
        CharacterTurnParticipationConsiderationRoundResult? RoundResult,
        AutonomousConversationBudget? Budget,
        string ModelStatus,
        string ModelText,
        string HealerMode,
        string FocusStatus);
}
