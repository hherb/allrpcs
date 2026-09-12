using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks complete stable ordinary participation scheduling with two listeners.</summary>
public partial class OrdinaryPlayerDialogueParticipationSchedulingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_ORDINARY_PARTICIPATION_SCHEDULING_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 300;
    private static readonly TimeSpan OrdinaryDeadline = TimeSpan.FromSeconds(5);

    private SchedulingScenario? scenario;
    private int processFrames;
    private int stage;

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
            Fail("Ordinary participation scheduling exceeded its bounded smoke duration.");
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
                StartMotivatedInterjection();
                break;
            case 3:
                ObserveMotivatedInterjection();
                break;
            case 4:
                StartAllSilent();
                break;
            case 5:
                ObserveAllSilent();
                break;
            default:
                Fail("Ordinary participation scheduling entered an unknown smoke stage.");
                break;
        }
    }

    private void StartDirectResponse()
    {
        SchedulingScenario current = CreateScenario((request, index) =>
            index == 0
                ? new CharacterTurnParticipationDecision(
                    request.ScheduledCharacter.Identity.Id,
                    CharacterTurnParticipationKind.Respond,
                    null)
                : MotivatedDecision(request, CharacterTurnParticipationKind.Interject));
        SceneState before = CaptureSceneState();
        if (!StartNewChain(current)
            || !ReferenceEquals(current.BudgetController.Budget, current.InitialBudget)
            || !current.ModelSource.TrySupplyModel(current.OverlapModel)
            || current.Coordinator.TryStart(
                current.Event,
                current.Characters,
                current.DirectAddresses,
                current.ModelSource,
                current.RoundController,
                current.BudgetController,
                current.TurnController,
                OrdinaryDeadline)
            || !ReferenceEquals(current.ModelSource.TakeModel(), current.OverlapModel)
            || !SceneStateIsUnchanged(before))
        {
            current.Dispose();
            Fail("The complete direct-response schedule did not start once without mutation.");
            return;
        }

        scenario = current;
        stage = 1;
    }

    private void ObserveDirectResponse()
    {
        SchedulingScenario current = scenario!;
        if (!ProcessToTerminal(current))
        {
            return;
        }

        CharacterTurnActivity? activity = current.TurnController.Activity;
        if (!HasExactStableRequests(current)
            || activity is null
            || !ReferenceEquals(
                activity.Request.RespondingCharacterId,
                current.Characters[0].Identity.Id)
            || !ReferenceEquals(activity.Request.DialogueEvent, current.Event)
            || current.Model.Decisions[0].Motive is not null)
        {
            current.Dispose();
            Fail("The exact scheduled direct response was not selected first.");
            return;
        }

        CompleteScenario(current);
        stage = 2;
    }

    private void StartMotivatedInterjection()
    {
        scenario = CreateScenario((request, index) => MotivatedDecision(
            request,
            index == 0
                ? CharacterTurnParticipationKind.RemainSilent
                : CharacterTurnParticipationKind.Interject));
        StartScenarioOrFail("motivated interjection");
        stage = 3;
    }

    private void ObserveMotivatedInterjection()
    {
        SchedulingScenario current = scenario!;
        if (!ProcessToTerminal(current))
        {
            return;
        }

        CharacterTurnActivity? activity = current.TurnController.Activity;
        CharacterGoal expectedMotive = current.Characters[1].Goals.Entries[0];
        if (!HasExactStableRequests(current)
            || activity is null
            || !ReferenceEquals(
                activity.Request.RespondingCharacterId,
                current.Characters[1].Identity.Id)
            || !ReferenceEquals(activity.Request.DialogueEvent, current.Event)
            || !ReferenceEquals(current.Model.Decisions[1].Motive, expectedMotive))
        {
            current.Dispose();
            Fail("Meaningful direct-listener silence did not select the exact motivated interjector.");
            return;
        }

        CompleteScenario(current);
        stage = 4;
    }

    private void StartAllSilent()
    {
        scenario = CreateScenario((request, _) =>
            MotivatedDecision(request, CharacterTurnParticipationKind.RemainSilent));
        StartScenarioOrFail("all-silent round");
        stage = 5;
    }

    private void ObserveAllSilent()
    {
        SchedulingScenario current = scenario!;
        if (!ProcessToTerminal(current))
        {
            return;
        }

        SetProcess(false);
        if (!HasExactStableRequests(current)
            || current.TurnController.Activity is not null
            || !SceneStateIsUnchanged(current.SceneStateBefore))
        {
            current.Dispose();
            Fail("The complete all-silent round changed turn or surrounding scene state.");
            return;
        }

        current.Dispose();
        scenario = null;
        GD.Print(
            "Ordinary participation scheduling smoke passed: two exact heard LLM listeners were "
            + "considered in stable order with value-equal direct addressing; direct response, "
            + "motivated interjection after meaningful silence, and all-silent outcomes selected "
            + "or stayed silent exactly, while overlap preserved its queued model and surrounding "
            + "scene state remained unchanged.");
        GetTree().Quit();
    }

    private void StartScenarioOrFail(string description)
    {
        SchedulingScenario current = scenario!;
        if (!StartNewChain(current))
        {
            current.Dispose();
            Fail($"The complete {description} schedule could not start.");
        }
    }

    private static bool StartNewChain(SchedulingScenario current)
    {
        return current.Coordinator.TryStartNewChain(
            current.Event,
            current.Characters,
            current.DirectAddresses,
            current.ModelSource,
            current.RoundController,
            current.BudgetController,
            current.TurnController,
            current.InitialBudget,
            OrdinaryDeadline);
    }

    private static bool ProcessToTerminal(SchedulingScenario current)
    {
        current.RoundController._Process(0);
        current.Coordinator._Process(0);
        return current.RoundController.Status != CharacterTurnParticipationRoundTaskStatus.Running;
    }

    private void CompleteScenario(SchedulingScenario current)
    {
        if (current.RoundController.Status != CharacterTurnParticipationRoundTaskStatus.Idle
            || current.RoundController.Result is not null
            || !SceneStateIsUnchanged(current.SceneStateBefore))
        {
            current.Dispose();
            Fail("A complete scheduled round did not reset without surrounding mutation.");
            return;
        }

        current.Dispose();
        scenario = null;
    }

    private static bool HasExactStableRequests(SchedulingScenario current)
    {
        return current.Model.Requests.Count == 2
            && current.Model.Decisions.Count == 2
            && ReferenceEquals(current.Model.Requests[0].ScheduledCharacter, current.Characters[0])
            && ReferenceEquals(current.Model.Requests[1].ScheduledCharacter, current.Characters[1])
            && ReferenceEquals(current.Model.Requests[0].HeardDialogueEvent, current.Event)
            && ReferenceEquals(current.Model.Requests[1].HeardDialogueEvent, current.Event)
            && current.Model.Requests[0].IsDirectlyAddressed
            && !current.Model.Requests[1].IsDirectlyAddressed;
    }

    private static CharacterTurnParticipationDecision MotivatedDecision(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationKind kind)
    {
        return new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            kind,
            request.ScheduledCharacter.Goals.Entries[0]);
    }

    private SchedulingScenario CreateScenario(
        Func<CharacterTurnParticipationConsiderationRequest, int,
            CharacterTurnParticipationDecision> decide)
    {
        Character directListener = CreateCharacter("travelling-healer");
        Character interjector = CreateCharacter("village-sage");
        Character[] characters = [directListener, interjector];
        DialogueEvent heardEvent = CreateEvent(characters);
        CharacterId[] directAddresses = [new CharacterId(directListener.Identity.Id.Value)];
        SchedulingModel model = new(decide);
        TemporaryCharacterTurnParticipationModelSource modelSource = new();
        if (!modelSource.TrySupplyModel(model))
        {
            throw new InvalidOperationException("The scheduling smoke model could not be supplied.");
        }

        AutonomousConversationBudgetController budgetController = new();
        AutonomousConversationBudget initialBudget = new(0, 2, false);
        return new SchedulingScenario(
            new OrdinaryPlayerDialogueParticipationCoordinator(),
            new CharacterTurnParticipationRoundTaskController(),
            modelSource,
            budgetController,
            new CharacterTurnActivityController(),
            characters,
            directAddresses,
            heardEvent,
            initialBudget,
            model,
            new SchedulingModel(decide),
            CaptureSceneState());
    }

    private static Character CreateCharacter(string id)
    {
        CharacterId characterId = new(id);
        return new Character(
            new CharacterIdentity(characterId, id.ToUpperInvariant()),
            new PublicCharacterBackground("A scheduled listener."),
            new PrivateCharacterBackground("A private scheduled listener."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal($"goal-{id}", "Speak for an authored reason.")]));
    }

    private static DialogueEvent CreateEvent(IReadOnlyList<Character> characters)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "What do you both think?");
        return new DialogueEvent(
            utterance,
            new DialogueAudienceSnapshot(
                utterance.SpeakerCharacterId,
                characters.Select(character => new CharacterId(character.Identity.Id.Value))));
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

    private sealed record SchedulingScenario(
        OrdinaryPlayerDialogueParticipationCoordinator Coordinator,
        CharacterTurnParticipationRoundTaskController RoundController,
        TemporaryCharacterTurnParticipationModelSource ModelSource,
        AutonomousConversationBudgetController BudgetController,
        CharacterTurnActivityController TurnController,
        IReadOnlyList<Character> Characters,
        IReadOnlyList<CharacterId> DirectAddresses,
        DialogueEvent Event,
        AutonomousConversationBudget InitialBudget,
        SchedulingModel Model,
        SchedulingModel OverlapModel,
        SceneState SceneStateBefore) : IDisposable
    {
        public void Dispose()
        {
            Coordinator.Free();
            RoundController.Free();
            ModelSource.Free();
            BudgetController.Free();
            TurnController.Free();
        }
    }

    private sealed class SchedulingModel(
        Func<CharacterTurnParticipationConsiderationRequest, int,
            CharacterTurnParticipationDecision> decide) : ICharacterTurnParticipationModel
    {
        public List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

        public List<CharacterTurnParticipationDecision> Decisions { get; } = [];

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            CharacterTurnParticipationDecision decision = decide(request, Requests.Count - 1);
            Decisions.Add(decision);
            return ValueTask.FromResult(decision);
        }
    }

    private sealed record SceneState(
        object? DialogueEvent,
        object? Transcript,
        object? EventLog,
        object? WorldTime,
        object? WorldObjects,
        object? Perception);
}
