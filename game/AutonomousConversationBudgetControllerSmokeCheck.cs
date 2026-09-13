using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free autonomous-conversation budget ownership checks.</summary>
public partial class AutonomousConversationBudgetControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_AUTONOMOUS_CONVERSATION_BUDGET_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        AutonomousConversationBudgetController controller =
            GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController");
        AutonomousConversationBudget original = new(0, 2, false);

        if (!ExercisesAutonomousTurnCoordination()
            || !ExercisesFreshPlayerRootedChains()
            || controller.Budget is not null
            || !RejectsNullInitialization(controller)
            || !controller.TryInitialize(original)
            || !ReferenceEquals(controller.Budget, original)
            || controller.TryInitialize(new AutonomousConversationBudget(0, 1, false))
            || !controller.TryRecordCompletedTurn())
        {
            Fail("Budget controller rejected exact one-time initialization or first completion.");
            return;
        }

        AutonomousConversationBudget afterCompletion = controller.Budget!;
        if (ReferenceEquals(afterCompletion, original)
            || original.CompletedTurnCount != 0
            || afterCompletion.CompletedTurnCount != 1
            || afterCompletion.MaximumTurnCount != 2
            || afterCompletion.HasPlayerInterrupted
            || !controller.TryInterrupt())
        {
            Fail("Budget controller failed immutable completion or interruption.");
            return;
        }

        AutonomousConversationBudget interrupted = controller.Budget!;
        if (ReferenceEquals(interrupted, afterCompletion)
            || !interrupted.HasPlayerInterrupted
            || interrupted.CompletedTurnCount != 1
            || controller.TryInterrupt()
            || controller.TryRecordCompletedTurn()
            || !ReferenceEquals(controller.Budget, interrupted)
            || !HasClosedSurface())
        {
            Fail("Budget controller failed monotonic or fail-closed ownership checks.");
            return;
        }

        GD.Print(
            "Autonomous-conversation budget controller smoke passed: exact one-time ownership, "
            + "exact follow-up creation/start, atomic completion, monotonic interruption, and "
            + "fail-closed repeated operations.");
        GetTree().Quit();
    }

    private static bool ExercisesAutonomousTurnCoordination()
    {
        AutonomousConversationBudgetController controller = new();
        CharacterTurnActivityController turnController = new();
        AutonomousConversationBudget originalBudget = new(0, 2, false);
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterId responder = sage.Identity.Id;
        CharacterTurnParticipationDecision decision = new(
            responder,
            CharacterTurnParticipationKind.Respond,
            null);
        CharacterTurnRequest request = CreateRequest(responder);

        if (!ExercisesExistingSelectionSeam(sage, decision, request.DialogueEvent)
            || !controller.TryInitialize(originalBudget)
            || !controller.TryStartFollowUp(turnController, decision, request)
            || turnController.Activity is not { } awaiting
            || !ReferenceEquals(awaiting.Request, request)
            || !ReferenceEquals(awaiting.State, CharacterTurnActivityState.AwaitingResponse)
            || !ReferenceEquals(controller.Budget, originalBudget)
            || controller.TryStartFollowUp(turnController, decision, request))
        {
            controller.Free();
            turnController.Free();
            return false;
        }

        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(responder, "The evidence supports another question."),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        if (!turnController.TryReceiveResponse(response)
            || !controller.TryCompleteTurn(turnController)
            || turnController.Activity is not { } completed
            || !ReferenceEquals(completed.Request, request)
            || !ReferenceEquals(completed.Response, response)
            || !ReferenceEquals(completed.State, CharacterTurnActivityState.Completed)
            || controller.Budget is not { CompletedTurnCount: 1 } advanced
            || ReferenceEquals(advanced, originalBudget)
            || !StartsOnlyAnEligibleFollowUp(
                controller,
                turnController,
                sage,
                decision,
                completed,
                advanced)
            || !controller.TryInterrupt()
            || controller.TryCompleteTurn(turnController)
            || controller.TryStartFollowUp(turnController, decision, request))
        {
            controller.Free();
            turnController.Free();
            return false;
        }

        bool result = RejectsCoordinationNulls(controller, turnController, decision, request);
        controller.Free();
        turnController.Free();
        return result;
    }

    private static bool ExercisesFreshPlayerRootedChains()
    {
        AutonomousConversationBudgetController controller = new();
        AutonomousConversationBudget first = new(0, 2, false);
        AutonomousConversationBudget second = new(0, 2, false);
        bool result = controller.TryInstallNewChain(null, first)
            && ReferenceEquals(controller.Budget, first)
            && !controller.TryInstallNewChain(null, second)
            && !controller.TryInstallNewChain(first, first)
            && !controller.TryInstallNewChain(
                first,
                new AutonomousConversationBudget(1, 2, false))
            && !controller.TryInstallNewChain(
                first,
                new AutonomousConversationBudget(0, 2, true))
            && ThrowsArgumentNull(() => controller.TryInstallNewChain(first, null!))
            && controller.TryInstallNewChain(first, second)
            && ReferenceEquals(controller.Budget, second);
        controller.Free();
        return result;
    }

    private static bool ExercisesExistingSelectionSeam(
        Character character,
        CharacterTurnParticipationDecision decision,
        DialogueEvent heardEvent)
    {
        AutonomousConversationBudgetController controller = new();
        CharacterTurnActivityController turnController = new();
        bool started = controller.TryInitialize(new AutonomousConversationBudget(0, 1, false))
            && controller.TrySelectAndStartFollowUp(
                turnController,
                [character],
                [decision],
                [new CharacterId(character.Identity.Id.Value)],
                heardEvent,
                DialogueActType.Question)
            && turnController.Activity is { } activity
            && ReferenceEquals(activity.Request.DialogueEvent, heardEvent)
            && ReferenceEquals(activity.Request.RespondingCharacterId, decision.ScheduledCharacterId);

        controller.Free();
        turnController.Free();
        return started;
    }

    private static bool StartsOnlyAnEligibleFollowUp(
        AutonomousConversationBudgetController controller,
        CharacterTurnActivityController turnController,
        Character scheduledCharacter,
        CharacterTurnParticipationDecision decision,
        CharacterTurnActivity completed,
        AutonomousConversationBudget advancedBudget)
    {
        CharacterId responder = decision.ScheduledCharacterId;
        DialogueUtterance utterance = new(
            new CharacterId("travelling-healer"),
            "The observed leaves support another question.");
        DialogueEvent heardEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                utterance.SpeakerCharacterId,
                [new CharacterId(responder.Value)]));
        CharacterTurnParticipationDecision silence = new(
            responder,
            CharacterTurnParticipationKind.RemainSilent,
            new Allrpcs.Domain.Characters.CharacterGoal(
                "compare-evidence",
                "Compare the available evidence."));
        DialogueEvent unheardEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                utterance.SpeakerCharacterId,
                [new CharacterId("woodland-hermit")]));
        CharacterTurnParticipationConsiderationRequest considerationRequest = new(
            scheduledCharacter,
            heardEvent,
            true);
        CharacterTurnParticipationConsiderationResult considerationResult = new(
            considerationRequest,
            decision);
        CharacterTurnParticipationConsiderationRoundResult considerationRound = new(
            [considerationRequest],
            [considerationResult]);

        if (controller.TryCreateAndStartFollowUp(
                turnController,
                silence,
                heardEvent,
                DialogueActType.ReportedClaim)
            || controller.TryCreateAndStartFollowUp(
                turnController,
                decision,
                unheardEvent,
                DialogueActType.ReportedClaim)
            || !ReferenceEquals(turnController.Activity, completed)
            || !controller.TrySelectAndStartFollowUp(
                turnController,
                considerationRound,
                DialogueActType.ReportedClaim)
            || turnController.Activity is not { } awaitingFollowUp)
        {
            return false;
        }

        return ReferenceEquals(awaitingFollowUp.State, CharacterTurnActivityState.AwaitingResponse)
            && ReferenceEquals(awaitingFollowUp.Request.RespondingCharacterId, responder)
            && ReferenceEquals(awaitingFollowUp.Request.DialogueEvent, heardEvent)
            && ReferenceEquals(
                awaitingFollowUp.Request.DialogueAct.Utterance,
                heardEvent.Utterance)
            && ReferenceEquals(
                awaitingFollowUp.Request.DialogueAct.Type,
                DialogueActType.ReportedClaim)
            && ReferenceEquals(scheduledCharacter.Identity.Id, responder)
            && ReferenceEquals(considerationResult, considerationRound.Results[0])
            && ReferenceEquals(controller.Budget, advancedBudget)
            && advancedBudget.CompletedTurnCount == 1;
    }

    private static bool RejectsCoordinationNulls(
        AutonomousConversationBudgetController controller,
        CharacterTurnActivityController turnController,
        CharacterTurnParticipationDecision decision,
        CharacterTurnRequest request)
    {
        return ThrowsArgumentNull(() => controller.TryStartFollowUp(null!, decision, request))
            && ThrowsArgumentNull(
                () => controller.TryStartFollowUp(turnController, null!, request))
            && ThrowsArgumentNull(
                () => controller.TryStartFollowUp(turnController, decision, null!))
            && ThrowsArgumentNull(
                () => controller.TryCreateAndStartFollowUp(
                    null!,
                    decision,
                    request.DialogueEvent,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TryCreateAndStartFollowUp(
                    turnController,
                    null!,
                    request.DialogueEvent,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TryCreateAndStartFollowUp(
                    turnController,
                    decision,
                    null!,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TryCreateAndStartFollowUp(
                    turnController,
                    decision,
                    request.DialogueEvent,
                    null!))
            && ThrowsArgumentNull(
                () => controller.TrySelectAndStartFollowUp(
                    null!,
                    [],
                    [],
                    [],
                    request.DialogueEvent,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TrySelectAndStartFollowUp(
                    turnController,
                    null!,
                    [],
                    [],
                    request.DialogueEvent,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TrySelectAndStartFollowUp(
                    turnController,
                    null!,
                    DialogueActType.Question))
            && ThrowsArgumentNull(
                () => controller.TrySelectAndStartFollowUp(
                    turnController,
                    new CharacterTurnParticipationConsiderationRoundResult([], []),
                    null!))
            && ThrowsArgumentNull(() => controller.TryCompleteTurn(null!));
    }

    private static CharacterTurnRequest CreateRequest(CharacterId responder)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "What follows?");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [responder]));
        return new CharacterTurnRequest(
            responder,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Question));
    }

    private static bool RejectsNullInitialization(
        AutonomousConversationBudgetController controller)
    {
        try
        {
            _ = controller.TryInitialize(null!);
        }
        catch (ArgumentNullException)
        {
            return controller.Budget is null;
        }

        return false;
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

    private static bool HasClosedSurface()
    {
        Type type = typeof(AutonomousConversationBudgetController);
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        return property.Name == "Budget"
            && property.PropertyType == typeof(AutonomousConversationBudget)
            && property.SetMethod?.IsPrivate == true
            && methods.Select(method => method.Name).SequenceEqual(
                [
                    "TryCompleteTurn",
                    "TryInitialize",
                    "TryInterrupt",
                    "TryRecordCompletedTurn",
                    "TrySelectAndStartFollowUp",
                    "TrySelectAndStartFollowUp",
                ]);
    }

    private static T AssertSingle<T>(IReadOnlyList<T> entries) => entries.Count == 1
        ? entries[0]
        : throw new InvalidOperationException("Expected exactly one controller property.");

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
