using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class ConversationTurnCoordinatorTests
{
    [Fact]
    public void AcceptsSubmissionOnlyWhenEveryOwnerIsIdle()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        Assert.True(coordinator.CanAcceptSubmission());

        ports.ParticipationBusy = true;
        Assert.False(coordinator.CanAcceptSubmission());
        ports.ParticipationBusy = false;
        ports.IsGenerating = true;
        Assert.False(coordinator.CanAcceptSubmission());
        ports.IsGenerating = false;
        ports.ParticipationRoundIdle = false;
        Assert.False(coordinator.CanAcceptSubmission());
        ports.ParticipationRoundIdle = true;
        ports.TurnActivity = ports.AwaitingHealerTurn();
        Assert.False(coordinator.CanAcceptSubmission());
        ports.TurnActivity = null;

        ports.LastSubmittedDelivery = ports.PlayerDelivery("hello");
        Assert.False(coordinator.CanAcceptSubmission());
        coordinator.ProcessDialogueSubmission();
        Assert.True(coordinator.CanAcceptSubmission());
    }

    [Fact]
    public void OccursPlayerDialogueOnceWithHearingCandidatesAndHealerDirectAddress()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        DialogueDelivery delivery = ports.PlayerDelivery("Can you help?");
        ports.LastSubmittedDelivery = delivery;

        coordinator.ProcessDialogueSubmission();
        coordinator.ProcessDialogueSubmission();

        Assert.Equal(["occur", "append", "chain"], ports.Calls);
        Assert.Same(delivery, ports.OccurredDelivery);
        Assert.Equal(ports.SeekerPosition, ports.OccurredSpeakerPosition);
        Assert.Equal((192.0f, 48.0f), ports.OccurredRanges);
        Assert.Equal(
            [
                new DialogueHearingCandidate(ports.SeekerId, ports.SeekerPosition),
                new DialogueHearingCandidate(ports.Healer.Identity.Id, ports.HealerPosition),
                new DialogueHearingCandidate(ports.Sage.Identity.Id, ports.SageHearingPosition),
            ],
            ports.OccurredCandidates);
        Assert.Same(delivery, Assert.Single(ports.AppendedDeliveries));
        Assert.Equal([ports.Healer, ports.Sage], ports.ChainScheduled);
        CharacterId address = Assert.Single(ports.ChainAddresses!);
        Assert.Equal(ports.Healer.Identity.Id, address);
        Assert.NotSame(ports.Healer.Identity.Id, address);
        Assert.Equal(
            (0, 2, false),
            (ports.ChainBudget!.CompletedTurnCount, ports.ChainBudget.MaximumTurnCount, ports.ChainBudget.HasPlayerInterrupted));
        Assert.Equal(TimeSpan.FromSeconds(30), ports.ChainDeadline);
    }

    [Fact]
    public void AbsentHealerNeitherHearsNorIsAddressed()
    {
        FakePorts ports = new() { HealerPresent = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.LastSubmittedDelivery = ports.PlayerDelivery("Anyone?");

        coordinator.ProcessDialogueSubmission();

        Assert.Equal(
            [ports.SeekerId, ports.Sage.Identity.Id],
            ports.OccurredCandidates!.Select(candidate => candidate.CharacterId));
        Assert.Equal([ports.Sage], ports.ChainScheduled);
        Assert.Empty(ports.ChainAddresses!);
    }

    [Fact]
    public void ReportsWhenNoListenerCanRespond()
    {
        FakePorts ports = new() { ChainResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.LastSubmittedDelivery = ports.PlayerDelivery("Hello?");

        coordinator.ProcessDialogueSubmission();

        Assert.Equal("No available listener can respond. Please retry.", ports.DialogueStatus);
    }

    [Fact]
    public void FailedDeliverySideEffectIsNeverReplayed()
    {
        FakePorts ports = new() { AppendResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.LastSubmittedDelivery = ports.PlayerDelivery("fails once");

        Assert.Throws<InvalidOperationException>(coordinator.ProcessDialogueSubmission);
        ports.AppendResult = true;
        coordinator.ProcessDialogueSubmission();

        Assert.Equal(["occur", "append"], ports.Calls);
    }

    [Fact]
    public void FailedOccurrenceThrowsBeforeTranscriptOrChain()
    {
        FakePorts ports = new() { OccurResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.LastSubmittedDelivery = ports.PlayerDelivery("cannot occur");

        Assert.Throws<InvalidOperationException>(coordinator.ProcessDialogueSubmission);
        Assert.Equal(["occur"], ports.Calls);
    }

    [Fact]
    public void StartsBoundedGenerationForTheExactAwaitingTurn()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();

        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["create-input", "start-generation"], ports.Calls);
        Assert.Same(ports.TurnActivity.Request, ports.StartedInput!.GenerationRequest.TurnRequest);
        Assert.Equal(TimeSpan.FromSeconds(30), ports.StartedDeadline);
    }

    [Fact]
    public void ResetsARetainedResultBeforeTheNextGeneration()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.GenerationResult = Terminal(new(CharacterModelStreamTerminationStatus.Cancelled, null, null));
        ports.GenerationHasStarted = true;
        ports.InputCurrent = false;

        coordinator.ProcessCharacterTurnResponse();
        Assert.Equal(["cancel-turn"], ports.Calls);
        ports.Calls.Clear();
        ports.TurnActivity = ports.AwaitingHealerTurn();
        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["reset-generation", "create-input", "start-generation"], ports.Calls);
    }

    [Fact]
    public void FailedResetOrStartIsAnInvariantViolation()
    {
        FakePorts ports = new() { ResetResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.GenerationResult = Terminal(new(CharacterModelStreamTerminationStatus.Cancelled, null, null));
        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);

        ports = new() { StartResult = false };
        coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);

        ports = new() { InputForOtherRequest = true };
        coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);
        Assert.DoesNotContain("start-generation", ports.Calls);
    }

    [Fact]
    public void ContextBudgetRefusalCancelsTheTurnWithVisibleStatus()
    {
        FakePorts ports = new()
        {
            InputResult = false,
            LastGenerationAssemblyStatus = CharacterModelContextAssemblyStatus.RequiredContextBudgetExceeded,
        };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();

        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["create-input", "cancel-turn"], ports.Calls);
        Assert.Equal(
            "Required dialogue context exceeds the budget. Reduce context and retry.",
            ports.DialogueStatus);
    }

    [Fact]
    public void MissingInputForAnotherReasonIsAnInvariantViolation()
    {
        FakePorts ports = new() { InputResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();

        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);
    }

    [Fact]
    public void ReceivesACompletedResponseOnceAndOffersTheExamination()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        CharacterTurnActivity awaiting = ports.AwaitingHealerTurn();
        ports.TurnActivity = awaiting;
        CharacterTurnResponse response = Response(awaiting.Request);
        ports.GenerationHasStarted = true;
        ports.GenerationResult = Terminal(new(CharacterModelStreamTerminationStatus.Completed, response, null));

        coordinator.ProcessCharacterTurnResponse();
        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["receive", "offer-examination"], ports.Calls);
        Assert.Same(response, ports.ReceivedResponse);
        Assert.Same(ports.TurnActivity, ports.OfferedActivity);
        Assert.NotSame(awaiting, ports.OfferedActivity);
    }

    [Fact]
    public void ConditionalCollectionScaffoldingKeepsTheExaminationUnoffered()
    {
        FakePorts ports = new() { ConditionalCollectionScaffolding = true };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        CharacterTurnActivity awaiting = ports.AwaitingHealerTurn();
        ports.TurnActivity = awaiting;
        ports.GenerationHasStarted = true;
        ports.GenerationResult = Terminal(
            new(CharacterModelStreamTerminationStatus.Completed, Response(awaiting.Request), null));

        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["receive"], ports.Calls);
    }

    [Fact]
    public void ResponseForAnotherRequestIsIgnored()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.GenerationHasStarted = true;
        ports.IsGenerating = true;
        ports.GenerationResult = Terminal(new(
            CharacterModelStreamTerminationStatus.Completed,
            Response(ports.AwaitingHealerTurn().Request),
            null));

        coordinator.ProcessCharacterTurnResponse();

        Assert.Empty(ports.Calls);
    }

    [Fact]
    public void StaleInputCancelsTheTurnWithoutReceiving()
    {
        FakePorts ports = new() { InputCurrent = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        CharacterTurnActivity awaiting = ports.AwaitingHealerTurn();
        ports.TurnActivity = awaiting;
        ports.GenerationHasStarted = true;
        ports.IsGenerating = true;
        ports.GenerationResult = Terminal(
            new(CharacterModelStreamTerminationStatus.Completed, Response(awaiting.Request), null));

        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(["cancel-turn"], ports.Calls);
    }

    [Theory]
    [InlineData(CharacterModelStreamTerminationStatus.TimedOut, "Dialogue response timed out. Please retry.")]
    [InlineData(CharacterModelStreamTerminationStatus.Cancelled, "Dialogue response cancelled. Please retry.")]
    [InlineData(
        CharacterModelStreamTerminationStatus.Failed,
        "Dialogue provider unavailable or response failed. Check provider settings and retry.")]
    public void UnsuccessfulResultShowsStatusAndCancelsThePendingTurn(
        CharacterModelStreamTerminationStatus status,
        string expected)
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.GenerationHasStarted = true;
        ports.IsGenerating = true;
        ports.GenerationResult = Terminal(new(
            status,
            null,
            status == CharacterModelStreamTerminationStatus.Failed
                ? new CharacterModelFailure(CharacterModelFailureCode.Unavailable)
                : null));

        coordinator.ProcessCharacterTurnResponse();

        Assert.Equal(expected, ports.DialogueStatus);
        Assert.Equal(["cancel-turn"], ports.Calls);
    }

    [Fact]
    public void UnsuccessfulResultThatCannotCancelIsAnInvariantViolation()
    {
        FakePorts ports = new() { CancelTurnResult = false };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.GenerationHasStarted = true;
        ports.GenerationResult = Terminal(new(CharacterModelStreamTerminationStatus.Cancelled, null, null));

        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);
    }

    [Fact]
    public void NonTerminalResultIsAnInvariantViolation()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.GenerationHasStarted = true;
        ports.GenerationResult = new CharacterModelStreamSnapshot([new CharacterModelTextDelta("partial")]);

        Assert.Throws<InvalidOperationException>(coordinator.ProcessCharacterTurnResponse);
    }

    [Fact]
    public void CompletedTurnWhoseSpeechOccurredContinuesParticipationOnce()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        CharacterTurnActivity awaiting = ports.AwaitingHealerTurn();
        CharacterTurnResponse response = Response(awaiting.Request);
        ports.TurnActivity = new(awaiting.Request, response, CharacterTurnActivityState.Completed);
        ports.LastOccurredDialogueEvent = new(
            response.Delivery!,
            new DialogueAudienceSnapshot(ports.Healer.Identity.Id, [ports.SeekerId, ports.Sage.Identity.Id]));

        coordinator.Process();
        coordinator.Process();

        Assert.Equal(["continue"], ports.Calls);
        Assert.Equal([ports.Sage], ports.ContinueScheduled);
        Assert.Empty(ports.ContinueAddresses!);
        Assert.Equal(TimeSpan.FromSeconds(30), ports.ContinueDeadline);
    }

    [Fact]
    public void CompletedTurnWithoutOccurredSpeechDoesNotContinue()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        CharacterTurnActivity awaiting = ports.AwaitingHealerTurn();
        ports.TurnActivity = new(awaiting.Request, Response(awaiting.Request), CharacterTurnActivityState.Completed);
        ports.LastOccurredDialogueEvent = awaiting.Request.DialogueEvent;

        coordinator.Process();

        Assert.Empty(ports.Calls);
    }

    [Fact]
    public void ProcessRestoresReadyStatusOnlyFromBusyWhenAcceptable()
    {
        FakePorts ports = new() { DialogueStatus = "Dialogue submission: BUSY" };
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.IsGenerating = true;
        coordinator.Process();
        Assert.Equal("Dialogue submission: BUSY", ports.DialogueStatus);

        ports.IsGenerating = false;
        coordinator.Process();
        Assert.Equal("Dialogue submission: READY", ports.DialogueStatus);

        ports.DialogueStatus = "Dialogue response timed out. Please retry.";
        coordinator.Process();
        Assert.Equal("Dialogue response timed out. Please retry.", ports.DialogueStatus);
    }

    [Fact]
    public void ProcessConsumesTheResponseBeforeTheNewSubmission()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.TurnActivity = ports.AwaitingHealerTurn();
        ports.LastSubmittedDelivery = ports.PlayerDelivery("same frame");

        coordinator.Process();

        Assert.Equal(["create-input", "start-generation", "occur", "append", "chain"], ports.Calls);
    }

    [Fact]
    public void CancelStopsEveryOwnerInOrder()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);

        coordinator.Cancel();

        Assert.Equal(
            ["cancel-examination", "cancel-generation", "cancel-round", "cancel-turn", "interrupt-budget"],
            ports.Calls);
    }

    [Fact]
    public void ResetSessionForgetsEveryConsumedFence()
    {
        FakePorts ports = new();
        ConversationTurnCoordinator<FakeInput> coordinator = new(ports);
        ports.LastSubmittedDelivery = ports.PlayerDelivery("before restore");
        coordinator.ProcessDialogueSubmission();
        Assert.True(coordinator.CanAcceptSubmission());

        coordinator.ResetSession();

        Assert.False(coordinator.CanAcceptSubmission());
        coordinator.ProcessDialogueSubmission();
        Assert.Equal(["occur", "append", "chain", "occur", "append", "chain"], ports.Calls);
    }

    private static CharacterModelStreamSnapshot Terminal(CharacterModelStreamTermination termination) =>
        new([termination]);

    private static CharacterTurnResponse Response(CharacterTurnRequest request) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(request.RespondingCharacterId, "I will look."),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);

    private static Character LlmCharacter(string id)
    {
        CharacterId characterId = new(id);
        return new Character(
            new CharacterIdentity(characterId, id.ToUpperInvariant()),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private sealed record FakeInput(CharacterModelGenerationRequest GenerationRequest)
        : ICharacterModelGenerationInput;

    private sealed class FakePorts : IConversationTurnPorts<FakeInput>
    {
        public List<string> Calls { get; } = [];

        public CharacterId SeekerId { get; } = new("seeker");

        public Character Healer { get; } = LlmCharacter("healer");

        public Character Sage { get; } = LlmCharacter("sage");

        public bool GenerationHasStarted { get; set; }

        public bool IsGenerating { get; set; }

        public CharacterModelStreamSnapshot? GenerationResult { get; set; }

        public CharacterModelContextAssemblyStatus? LastGenerationAssemblyStatus { get; set; }

        public CharacterTurnActivity? TurnActivity { get; set; }

        public IReadOnlyList<Character> OrdinaryParticipants => [Healer, Sage];

        public bool HealerPresent { get; set; } = true;

        public NumericsVector2 SeekerPosition { get; } = new(10, 20);

        public NumericsVector2 HealerPosition { get; } = new(30, 40);

        public NumericsVector2 SageHearingPosition { get; } = new(50, 60);

        public DialogueDelivery? LastSubmittedDelivery { get; set; }

        public string DialogueStatus { get; set; } = "Dialogue submission: READY";

        public DialogueEvent? LastOccurredDialogueEvent { get; set; }

        public bool ParticipationBusy { get; set; }

        public bool ParticipationRoundIdle { get; set; } = true;

        public bool ConditionalCollectionScaffolding { get; set; }

        public bool ResetResult { get; set; } = true;

        public bool StartResult { get; set; } = true;

        public bool InputResult { get; set; } = true;

        public bool InputForOtherRequest { get; set; }

        public bool InputCurrent { get; set; } = true;

        public bool CancelTurnResult { get; set; } = true;

        public bool OccurResult { get; set; } = true;

        public bool AppendResult { get; set; } = true;

        public bool ChainResult { get; set; } = true;

        public FakeInput? StartedInput { get; private set; }

        public TimeSpan? StartedDeadline { get; private set; }

        public CharacterTurnResponse? ReceivedResponse { get; private set; }

        public CharacterTurnActivity? OfferedActivity { get; private set; }

        public DialogueDelivery? OccurredDelivery { get; private set; }

        public NumericsVector2? OccurredSpeakerPosition { get; private set; }

        public (float, float)? OccurredRanges { get; private set; }

        public IReadOnlyList<DialogueHearingCandidate>? OccurredCandidates { get; private set; }

        public List<DialogueDelivery> AppendedDeliveries { get; } = [];

        public IReadOnlyList<Character>? ChainScheduled { get; private set; }

        public IReadOnlyList<CharacterId>? ChainAddresses { get; private set; }

        public AutonomousConversationBudget? ChainBudget { get; private set; }

        public TimeSpan? ChainDeadline { get; private set; }

        public IReadOnlyList<Character>? ContinueScheduled { get; private set; }

        public IReadOnlyList<CharacterId>? ContinueAddresses { get; private set; }

        public TimeSpan? ContinueDeadline { get; private set; }

        public DialogueDelivery PlayerDelivery(string text) =>
            new(new DialogueUtterance(SeekerId, text), DialogueDeliveryMode.OrdinarySpeech);

        public CharacterTurnActivity AwaitingHealerTurn()
        {
            DialogueUtterance utterance = new(SeekerId, "Please help.");
            CharacterTurnRequest request = new(
                Healer.Identity.Id,
                new DialogueEvent(
                    new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                    new DialogueAudienceSnapshot(SeekerId, [Healer.Identity.Id])),
                new DialogueAct(utterance, DialogueActType.Request));
            return new CharacterTurnActivity(request, null, CharacterTurnActivityState.AwaitingResponse);
        }

        public bool TryResetGeneration()
        {
            Calls.Add("reset-generation");
            GenerationResult = null;
            GenerationHasStarted = false;
            return ResetResult;
        }

        public bool TryCancelGeneration()
        {
            Calls.Add("cancel-generation");
            return true;
        }

        public FakeInput? CreateGenerationInput(CharacterTurnActivity activity)
        {
            Calls.Add("create-input");
            if (!InputResult)
            {
                return null;
            }

            CharacterTurnRequest request = InputForOtherRequest ? AwaitingHealerTurn().Request : activity.Request;
            return new FakeInput(new CharacterModelGenerationRequest(
                request,
                new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        request.DialogueEvent.Utterance.Text,
                        request.DialogueEvent.Delivery.Mode),
                ])));
        }

        public bool IsGenerationInputCurrent(CharacterTurnRequest request) => InputCurrent;

        public bool TryStartGeneration(FakeInput input, TimeSpan deadline)
        {
            Calls.Add("start-generation");
            StartedInput = input;
            StartedDeadline = deadline;
            return StartResult;
        }

        public bool TryCancelTurn()
        {
            Calls.Add("cancel-turn");
            if (CancelTurnResult && TurnActivity is { } activity)
            {
                TurnActivity = new(activity.Request, activity.Response, CharacterTurnActivityState.Cancelled);
            }

            return CancelTurnResult;
        }

        public bool TryReceiveTurnResponse(CharacterTurnResponse response)
        {
            Calls.Add("receive");
            ReceivedResponse = response;
            TurnActivity = new(response.Request, response, CharacterTurnActivityState.Speaking);
            return true;
        }

        public void SetDialogueStatus(string status) => DialogueStatus = status;

        public bool TryOccurDialogue(
            DialogueDelivery delivery,
            NumericsVector2 speakerPosition,
            float ordinaryHearingRangePixels,
            float whisperHearingRangePixels,
            IReadOnlyList<DialogueHearingCandidate> candidates)
        {
            Calls.Add("occur");
            OccurredDelivery = delivery;
            OccurredSpeakerPosition = speakerPosition;
            OccurredRanges = (ordinaryHearingRangePixels, whisperHearingRangePixels);
            OccurredCandidates = candidates.ToArray();
            if (OccurResult)
            {
                LastOccurredDialogueEvent = new(
                    delivery,
                    new DialogueAudienceSnapshot(
                        delivery.Utterance.SpeakerCharacterId,
                        candidates.Skip(1).Select(candidate => candidate.CharacterId)));
            }

            return OccurResult;
        }

        public bool TryAppendSpokenStatement(DialogueDelivery delivery)
        {
            Calls.Add("append");
            AppendedDeliveries.Add(delivery);
            return AppendResult;
        }

        public bool TryContinueParticipation(
            DialogueEvent heardDialogueEvent,
            IReadOnlyList<Character> scheduledCharacters,
            IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
            TimeSpan deadline)
        {
            Calls.Add("continue");
            ContinueScheduled = scheduledCharacters;
            ContinueAddresses = directlyAddressedCharacterIds;
            ContinueDeadline = deadline;
            return true;
        }

        public bool TryStartParticipationChain(
            DialogueEvent heardDialogueEvent,
            IReadOnlyList<Character> scheduledCharacters,
            IReadOnlyList<CharacterId> directlyAddressedCharacterIds,
            AutonomousConversationBudget initialBudget,
            TimeSpan deadline)
        {
            Calls.Add("chain");
            ChainScheduled = scheduledCharacters;
            ChainAddresses = directlyAddressedCharacterIds;
            ChainBudget = initialBudget;
            ChainDeadline = deadline;
            return ChainResult;
        }

        public bool TryCancelParticipationRound()
        {
            Calls.Add("cancel-round");
            return true;
        }

        public bool TryInterruptConversationBudget()
        {
            Calls.Add("interrupt-budget");
            return true;
        }

        public void OfferExamination(CharacterTurnActivity activity)
        {
            Calls.Add("offer-examination");
            OfferedActivity = activity;
        }

        public void CancelExamination() => Calls.Add("cancel-examination");
    }
}
