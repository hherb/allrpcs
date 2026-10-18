using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Sequences one frame of the ordinary conversation: LLM turn generation and its single terminal
/// result, participation after a completed turn, player-dialogue occurrence, player cancel and
/// session reset. Each delivery, completed turn and model result is consumed at most once.
/// </summary>
/// <typeparam name="TInput">The exact prepared generation input the input source fences.</typeparam>
public sealed class ConversationTurnCoordinator<TInput>
    where TInput : class, ICharacterModelGenerationInput
{
    /// <summary>The fixed bounded deadline for one ordinary character-model generation.</summary>
    public static readonly TimeSpan CharacterModelGenerationDeadline = TimeSpan.FromSeconds(30);

    /// <summary>The fixed bounded deadline for one participation round.</summary>
    public static readonly TimeSpan ParticipationRoundDeadline = TimeSpan.FromSeconds(30);

    /// <summary>The autonomous turns one player utterance may start.</summary>
    public const int AutonomousConversationMaximumTurnCount = 2;

    /// <summary>The ordinary-speech hearing range.</summary>
    public const float DialogueHearingRangePixels = 192.0f;

    /// <summary>The whisper hearing range.</summary>
    public const float WhisperHearingRangePixels = 48.0f;

    private readonly IConversationTurnPorts<TInput> ports;
    private CharacterModelStreamSnapshot? lastProcessedModelResult;
    private CharacterTurnActivity? lastProcessedCompletedCharacterTurn;
    private DialogueDelivery? lastProcessedDialogueDelivery;

    /// <summary>Creates a coordinator over the exact native owners.</summary>
    public ConversationTurnCoordinator(IConversationTurnPorts<TInput> ports)
    {
        ArgumentNullException.ThrowIfNull(ports);
        this.ports = ports;
    }

    /// <summary>Gets whether a new player submission may be accepted now.</summary>
    public bool CanAcceptSubmission() =>
        (ports.LastSubmittedDelivery is null
            || ReferenceEquals(ports.LastSubmittedDelivery, lastProcessedDialogueDelivery))
        && !ports.ParticipationBusy
        && !ports.IsGenerating
        && ports.ParticipationRoundIdle
        && CharacterTurnStartPolicy.CanStart(ports.TurnActivity);

    /// <summary>Runs one frame's conversation steps in their required order.</summary>
    public void Process()
    {
        ProcessCharacterTurnResponse();
        ProcessCompletedCharacterTurnParticipation();
        ProcessDialogueSubmission();
        if (CanAcceptSubmission() && ports.DialogueStatus.Contains("BUSY", StringComparison.Ordinal))
        {
            ports.SetDialogueStatus("Dialogue submission: READY");
        }
    }

    /// <summary>Consumes a terminal model result, then starts generation for an awaiting turn.</summary>
    public void ProcessCharacterTurnResponse()
    {
        if (ports.GenerationHasStarted)
        {
            ProcessCharacterModelTurnResponse();
            if (ports.IsGenerating)
            {
                return;
            }
        }

        CharacterTurnActivity? activity = ports.TurnActivity;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        if (ports.GenerationResult is not null && !ports.TryResetGeneration())
        {
            throw new InvalidOperationException(
                "The completed character-model flow could not reset for the next exact turn.");
        }

        Character character = CharacterTurnParticipantLookup.Find(
            ports.OrdinaryParticipants,
            activity)
            ?? throw new InvalidOperationException(
                "The awaiting ordinary character turn had no exact LLM participant.");
        CharacterControllerAssignment assignment = character.ControllerAssignment;
        TInput? input = ports.CreateGenerationInput(activity);
        if (input is null
            && ports.LastGenerationAssemblyStatus
                == CharacterModelContextAssemblyStatus.RequiredContextBudgetExceeded)
        {
            _ = ports.TryCancelTurn();
            ports.SetDialogueStatus(
                "Required dialogue context exceeds the budget. Reduce context and retry.");
            return;
        }

        if (!ReferenceEquals(assignment.CharacterId, activity.Request.RespondingCharacterId)
            || !ReferenceEquals(assignment.ControllerType, ControllerType.Llm)
            || input is null
            || !ReferenceEquals(input.GenerationRequest.TurnRequest, activity.Request)
            || !ports.TryStartGeneration(input, CharacterModelGenerationDeadline))
        {
            throw new InvalidOperationException(
                "The ordinary LLM healer turn could not start its bounded generation.");
        }
    }

    /// <summary>Occurs the last accepted player delivery once and starts its participation.</summary>
    public void ProcessDialogueSubmission()
    {
        DialogueDelivery? delivery = ports.LastSubmittedDelivery;
        if (delivery is null || ReferenceEquals(delivery, lastProcessedDialogueDelivery))
        {
            return;
        }

        // Fenced before any side effect so a failing step is never replayed.
        lastProcessedDialogueDelivery = delivery;
        IReadOnlyList<Character> participants = ports.OrdinaryParticipants;
        List<DialogueHearingCandidate> candidates =
        [
            new(delivery.Utterance.SpeakerCharacterId, ports.SeekerPosition),
        ];
        if (ports.HealerPresent)
        {
            candidates.Add(new(participants[0].Identity.Id, ports.HealerPosition));
        }

        candidates.Add(new(participants[1].Identity.Id, ports.SageHearingPosition));
        if (!ports.TryOccurDialogue(
                delivery,
                candidates[0].Position,
                DialogueHearingRangePixels,
                WhisperHearingRangePixels,
                candidates))
        {
            throw new InvalidOperationException(
                "The accepted player dialogue delivery could not occur.");
        }

        DialogueEvent occurredEvent = ports.LastOccurredDialogueEvent
            ?? throw new InvalidOperationException("The occurred player dialogue event was absent.");
        if (!ReferenceEquals(occurredEvent.Delivery, delivery)
            || !ports.TryAppendSpokenStatement(occurredEvent.Delivery))
        {
            throw new InvalidOperationException(
                "The occurred player dialogue delivery could not append to the transcript.");
        }

        StartParticipationRoundIfEligible(occurredEvent);
    }

    /// <summary>Cancels examination, generation, round, turn and budget, in that order.</summary>
    public void Cancel()
    {
        ports.CancelExamination();
        _ = ports.TryCancelGeneration();
        _ = ports.TryCancelParticipationRound();
        _ = ports.TryCancelTurn();
        _ = ports.TryInterruptConversationBudget();
    }

    /// <summary>Forgets every consumed result, turn and delivery for a restored session.</summary>
    public void ResetSession()
    {
        lastProcessedModelResult = null;
        lastProcessedCompletedCharacterTurn = null;
        lastProcessedDialogueDelivery = null;
    }

    private void ProcessCharacterModelTurnResponse()
    {
        CharacterModelStreamSnapshot? result = ports.GenerationResult;
        if (result is null || ReferenceEquals(result, lastProcessedModelResult))
        {
            return;
        }

        lastProcessedModelResult = result;
        if (result.Updates[^1] is not CharacterModelStreamTermination termination)
        {
            throw new InvalidOperationException("The completed model flow was not terminal.");
        }

        CharacterTurnActivity? activity = ports.TurnActivity;
        if (activity is not null && !ports.IsGenerationInputCurrent(activity.Request))
        {
            if (CharacterTurnCancellationPolicy.CanCancel(activity))
            {
                _ = ports.TryCancelTurn();
            }

            return;
        }

        if (termination is
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response: not null,
            })
        {
            if (activity is not null
                && CharacterTurnResponseReceptionPolicy.CanReceive(activity, termination.Response))
            {
                if (!ports.TryReceiveTurnResponse(termination.Response))
                {
                    throw new InvalidOperationException(
                        "The validated model response could not enter its exact awaiting turn.");
                }

                if (!ports.ConditionalCollectionScaffolding)
                {
                    ports.OfferExamination(ports.TurnActivity!);
                }
            }

            return;
        }

        ports.SetDialogueStatus(termination.Status switch
        {
            CharacterModelStreamTerminationStatus.TimedOut => "Dialogue response timed out. Please retry.",
            CharacterModelStreamTerminationStatus.Cancelled => "Dialogue response cancelled. Please retry.",
            _ => "Dialogue provider unavailable or response failed. Check provider settings and retry.",
        });
        if (activity is not null
            && CharacterTurnCancellationPolicy.CanCancel(activity)
            && !ports.TryCancelTurn())
        {
            throw new InvalidOperationException(
                "The unsuccessful model response could not cancel its exact pending turn.");
        }
    }

    private void ProcessCompletedCharacterTurnParticipation()
    {
        CharacterTurnActivity? activity = ports.TurnActivity;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || ReferenceEquals(activity, lastProcessedCompletedCharacterTurn))
        {
            return;
        }

        lastProcessedCompletedCharacterTurn = activity;
        DialogueUtterance? speech = activity.Response?.Speech;
        DialogueEvent? occurredEvent = ports.LastOccurredDialogueEvent;
        if (speech is null || !ReferenceEquals(occurredEvent?.Utterance, speech))
        {
            return;
        }

        Character[] scheduledCharacters = GetHeardOrdinaryParticipants(occurredEvent);
        if (scheduledCharacters.Length == 0)
        {
            return;
        }

        _ = ports.TryContinueParticipation(
            occurredEvent,
            scheduledCharacters,
            [],
            ParticipationRoundDeadline);
    }

    private void StartParticipationRoundIfEligible(DialogueEvent occurredEvent)
    {
        Character[] heardParticipants = GetHeardOrdinaryParticipants(occurredEvent);
        Character healerCharacter = ports.OrdinaryParticipants[0];
        CharacterId[] directAddresses = heardParticipants.Any(
            character => ReferenceEquals(character, healerCharacter))
            ? [new CharacterId(healerCharacter.Identity.Id.Value)]
            : [];
        bool started = ports.TryStartParticipationChain(
            occurredEvent,
            heardParticipants,
            directAddresses,
            new AutonomousConversationBudget(
                0,
                AutonomousConversationMaximumTurnCount,
                false),
            ParticipationRoundDeadline);
        if (!started)
        {
            ports.SetDialogueStatus("No available listener can respond. Please retry.");
        }
    }

    private Character[] GetHeardOrdinaryParticipants(DialogueEvent occurredEvent)
    {
        HashSet<CharacterId> heardCharacterIds =
            occurredEvent.Audience.ListenerCharacterIds.ToHashSet();
        IReadOnlyList<Character> participants = ports.OrdinaryParticipants;
        return participants
            .Where(character => heardCharacterIds.Contains(character.Identity.Id)
                && (!ReferenceEquals(character, participants[0]) || ports.HealerPresent))
            .ToArray();
    }
}
