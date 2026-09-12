using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks ordinary participation preflight and creates isolated smoke scenarios.</summary>
internal static class OrdinaryPlayerDialogueParticipationPreflightSmoke
{
    private static readonly TimeSpan OrdinaryDeadline = TimeSpan.FromSeconds(5);

    public static bool Passes()
    {
        return RejectsIneligibleStarts() && RejectsNonLlmCharacter() && RejectsInvalidInputs()
            && HasClosedSurface();
    }

    public static OrdinaryPlayerDialogueParticipationSmokeScenario CreateScenario(
        CharacterTurnParticipationKind? kind,
        ICharacterTurnParticipationModel? suppliedModel = null,
        AutonomousConversationBudget? budget = null)
    {
        Character character = CreateLlmCharacter();
        CharacterId directAddress = new(character.Identity.Id.Value);
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        OrdinaryParticipationRecordingModel model = new(
            request => new CharacterTurnParticipationDecision(
            request.ScheduledCharacter.Identity.Id,
            kind ?? CharacterTurnParticipationKind.Respond,
            kind == CharacterTurnParticipationKind.RemainSilent
                ? request.ScheduledCharacter.Goals.Entries[0]
                : null));
        TemporaryCharacterTurnParticipationModelSource modelSource = new();
        if (kind is not null && !modelSource.TrySupplyModel(suppliedModel ?? model))
        {
            throw new InvalidOperationException("The smoke one-shot model could not be supplied.");
        }

        AutonomousConversationBudget initialBudget = budget
            ?? new AutonomousConversationBudget(0, 2, false);
        AutonomousConversationBudgetController budgetController = new();
        if (!budgetController.TryInitialize(initialBudget))
        {
            throw new InvalidOperationException("The smoke budget could not initialize.");
        }

        return new OrdinaryPlayerDialogueParticipationSmokeScenario(
            new OrdinaryPlayerDialogueParticipationCoordinator(),
            new CharacterTurnParticipationRoundTaskController(),
            modelSource,
            budgetController,
            new CharacterTurnActivityController(),
            character,
            directAddress,
            heardEvent,
            initialBudget,
            model);
    }

    private static bool RejectsIneligibleStarts()
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario exhausted = CreateScenario(
            CharacterTurnParticipationKind.Respond,
            budget: new AutonomousConversationBudget(1, 1, false));
        ICharacterTurnParticipationModel retainedExhaustedModel = exhausted.ModelSource.TakeModel();
        _ = exhausted.ModelSource.TrySupplyModel(retainedExhaustedModel);
        bool exhaustedRejected = !TryStart(exhausted)
            && ReferenceEquals(exhausted.ModelSource.TakeModel(), retainedExhaustedModel);
        exhausted.Dispose();

        OrdinaryPlayerDialogueParticipationSmokeScenario unheard =
            CreateScenario(CharacterTurnParticipationKind.Respond);
        DialogueEvent otherEvent = CreateEvent(new CharacterId("woodland-hermit"));
        AutonomousConversationBudget initialUnheardBudget = unheard.BudgetController.Budget!;
        ICharacterTurnParticipationModel retainedUnheardModel = unheard.ModelSource.TakeModel();
        _ = unheard.ModelSource.TrySupplyModel(retainedUnheardModel);
        bool unheardRejected = !unheard.Coordinator.TryStartNewChain(
                otherEvent,
                unheard.Character,
                unheard.DirectAddress,
                unheard.ModelSource,
                unheard.RoundController,
                unheard.BudgetController,
                unheard.TurnController,
                new AutonomousConversationBudget(0, 2, false),
                OrdinaryDeadline)
            && ReferenceEquals(unheard.ModelSource.TakeModel(), retainedUnheardModel)
            && ReferenceEquals(unheard.BudgetController.Budget, initialUnheardBudget);
        unheard.Dispose();

        OrdinaryPlayerDialogueParticipationSmokeScenario activeTurn =
            CreateScenario(CharacterTurnParticipationKind.Respond);
        CharacterTurnRequest activeRequest = new(
            activeTurn.Character.Identity.Id,
            activeTurn.Event,
            new DialogueAct(activeTurn.Event.Utterance, DialogueActType.Request));
        _ = activeTurn.TurnController.TryStart(activeRequest);
        ICharacterTurnParticipationModel retainedActiveModel = activeTurn.ModelSource.TakeModel();
        _ = activeTurn.ModelSource.TrySupplyModel(retainedActiveModel);
        bool activeTurnRejected = !TryStart(activeTurn)
            && ReferenceEquals(activeTurn.ModelSource.TakeModel(), retainedActiveModel);
        activeTurn.Dispose();

        return exhaustedRejected && unheardRejected && activeTurnRejected;
    }

    private static bool RejectsNonLlmCharacter()
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario scenario =
            CreateScenario(CharacterTurnParticipationKind.Respond);
        CharacterId id = scenario.Character.Identity.Id;
        Character human = new(
            scenario.Character.Identity,
            scenario.Character.PublicBackground,
            scenario.Character.PrivateBackground,
            scenario.Character.Capabilities,
            scenario.Character.Limitations,
            new CharacterControllerAssignment(id, ControllerType.Human),
            scenario.Character.Conditions,
            scenario.Character.Inventory,
            scenario.Character.Knowledge,
            scenario.Character.Beliefs,
            scenario.Character.Relationships,
            scenario.Character.Goals);
        bool rejected = !scenario.Coordinator.TryStart(
            scenario.Event,
            human,
            scenario.DirectAddress,
            scenario.ModelSource,
            scenario.RoundController,
            scenario.BudgetController,
            scenario.TurnController,
            OrdinaryDeadline);
        scenario.Dispose();
        return rejected;
    }

    private static bool RejectsInvalidInputs()
    {
        OrdinaryPlayerDialogueParticipationSmokeScenario scenario =
            CreateScenario(CharacterTurnParticipationKind.Respond);
        object?[] invalidArguments =
        [
            null,
            scenario.Event,
            scenario.Character,
            scenario.DirectAddress,
            scenario.ModelSource,
            scenario.RoundController,
            scenario.BudgetController,
            scenario.TurnController,
        ];
        bool rejectedNulls = Enumerable.Range(0, 7).All(index => ThrowsArgumentNull(() =>
            InvokeWithNullAt(scenario, invalidArguments, index)));
        bool rejectedDeadline = ThrowsArgumentOutOfRange(() => scenario.Coordinator.TryStart(
            scenario.Event,
            scenario.Character,
            scenario.DirectAddress,
            scenario.ModelSource,
            scenario.RoundController,
            scenario.BudgetController,
            scenario.TurnController,
            TimeSpan.Zero));
        bool rejectedNullInitialBudget = ThrowsArgumentNull(
            () => scenario.Coordinator.TryStartNewChain(
                scenario.Event,
                scenario.Character,
                scenario.DirectAddress,
                scenario.ModelSource,
                scenario.RoundController,
                scenario.BudgetController,
                scenario.TurnController,
                null!,
                OrdinaryDeadline));
        scenario.Dispose();
        return rejectedNulls && rejectedDeadline && rejectedNullInitialBudget;
    }

    private static void InvokeWithNullAt(
        OrdinaryPlayerDialogueParticipationSmokeScenario scenario,
        object?[] arguments,
        int nullIndex)
    {
        object?[] values = arguments.Skip(1).ToArray();
        values[nullIndex] = null;
        _ = scenario.Coordinator.TryStart(
            (DialogueEvent)values[0]!,
            (Character)values[1]!,
            (CharacterId)values[2]!,
            (TemporaryCharacterTurnParticipationModelSource)values[3]!,
            (CharacterTurnParticipationRoundTaskController)values[4]!,
            (AutonomousConversationBudgetController)values[5]!,
            (CharacterTurnActivityController)values[6]!,
            OrdinaryDeadline);
    }

    private static bool TryStart(OrdinaryPlayerDialogueParticipationSmokeScenario scenario)
    {
        return scenario.Coordinator.TryStart(
            scenario.Event,
            scenario.Character,
            scenario.DirectAddress,
            scenario.ModelSource,
            scenario.RoundController,
            scenario.BudgetController,
            scenario.TurnController,
            OrdinaryDeadline);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(OrdinaryPlayerDialogueParticipationCoordinator);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName && method.Name != nameof(Node._Process))
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();
        return properties.Length == 0
            && methods.Length == 2
            && methods[0].Name == "TryStart"
            && methods[1].Name == "TryStartNewChain"
            && methods.All(method => method.ReturnType == typeof(bool));
    }

    private static Character CreateLlmCharacter()
    {
        CharacterId id = new("travelling-healer");
        return new Character(
            new CharacterIdentity(id, "HEALER"),
            new PublicCharacterBackground("A cautious travelling healer."),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal("listen", "Listen before choosing.")]));
    }

    private static DialogueEvent CreateEvent(CharacterId listener)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "Can you help me?");
        return new DialogueEvent(
            utterance,
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [listener]));
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

    private static bool ThrowsArgumentOutOfRange(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }

        return false;
    }
}
