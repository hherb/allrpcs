using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

internal enum CharacterModelLiveConformanceOutcome
{
    Conforming,
    StreamViolation,
    TerminationFailure,
    ResponseViolation,
}

internal sealed record CharacterModelLiveConformanceSummary
{
    internal const int RequiredAttemptCount = 3;

    private CharacterModelLiveConformanceSummary(
        int conformingCount,
        int streamViolationCount,
        int terminationFailureCount,
        int responseViolationCount)
    {
        ConformingCount = conformingCount;
        StreamViolationCount = streamViolationCount;
        TerminationFailureCount = terminationFailureCount;
        ResponseViolationCount = responseViolationCount;
    }

    internal int ConformingCount { get; }

    internal int StreamViolationCount { get; }

    internal int TerminationFailureCount { get; }

    internal int ResponseViolationCount { get; }

    internal bool IsConforming => ConformingCount == RequiredAttemptCount;

    internal static CharacterModelLiveConformanceSummary Create(
        IEnumerable<CharacterModelLiveConformanceOutcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        int conformingCount = 0;
        int streamViolationCount = 0;
        int terminationFailureCount = 0;
        int responseViolationCount = 0;
        int totalCount = 0;
        foreach (CharacterModelLiveConformanceOutcome outcome in outcomes)
        {
            if (!Enum.IsDefined(outcome))
            {
                throw new ArgumentOutOfRangeException(nameof(outcomes));
            }

            totalCount++;
            if (totalCount > RequiredAttemptCount)
            {
                throw new ArgumentException(
                    $"Live conformance requires exactly {RequiredAttemptCount} outcomes.",
                    nameof(outcomes));
            }

            switch (outcome)
            {
                case CharacterModelLiveConformanceOutcome.Conforming:
                    conformingCount++;
                    break;
                case CharacterModelLiveConformanceOutcome.StreamViolation:
                    streamViolationCount++;
                    break;
                case CharacterModelLiveConformanceOutcome.TerminationFailure:
                    terminationFailureCount++;
                    break;
                case CharacterModelLiveConformanceOutcome.ResponseViolation:
                    responseViolationCount++;
                    break;
            }
        }

        if (totalCount != RequiredAttemptCount)
        {
            throw new ArgumentException(
                $"Live conformance requires exactly {RequiredAttemptCount} outcomes.",
                nameof(outcomes));
        }

        return new CharacterModelLiveConformanceSummary(
            conformingCount,
            streamViolationCount,
            terminationFailureCount,
            responseViolationCount);
    }

    public override string ToString() =>
        $"Live conformance aggregate: completed={ConformingCount}; "
        + $"stream-violations={StreamViolationCount}; "
        + $"termination-failures={TerminationFailureCount}; "
        + $"response-violations={ResponseViolationCount}.";
}

internal static class CharacterModelLiveConformance
{
    internal static CharacterModelLiveConformanceOutcome Evaluate(
        CharacterModelGenerationRequest request,
        int updateCount,
        CharacterModelStreamTermination? termination)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (updateCount != 1 || termination is null)
        {
            return CharacterModelLiveConformanceOutcome.StreamViolation;
        }

        if (termination.Status != CharacterModelStreamTerminationStatus.Completed)
        {
            return CharacterModelLiveConformanceOutcome.TerminationFailure;
        }

        CharacterTurnResponse response = termination.Response!;
        if (!HasSafeResponseCore(request, response)
            || !ReferenceEquals(response.ImmediateIntention.Type, CharacterIntentionType.Wait)
            || response.ImmediateIntention.TargetObjectId is not null)
        {
            return CharacterModelLiveConformanceOutcome.ResponseViolation;
        }

        return CharacterModelLiveConformanceOutcome.Conforming;
    }

    internal static CharacterModelLiveConformanceOutcome EvaluateTargetedAction(
        CharacterModelGenerationRequest request,
        int updateCount,
        CharacterModelStreamTermination? termination,
        CharacterIntentionType expectedType,
        ObjectId expectedTargetObjectId,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(expectedType);
        ArgumentNullException.ThrowIfNull(expectedTargetObjectId);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(affordances);

        if (updateCount != 1 || termination is null)
        {
            return CharacterModelLiveConformanceOutcome.StreamViolation;
        }

        if (termination.Status != CharacterModelStreamTerminationStatus.Completed)
        {
            return CharacterModelLiveConformanceOutcome.TerminationFailure;
        }

        CharacterTurnResponse response = termination.Response!;
        CharacterIntention intention = response.ImmediateIntention;
        CharacterTurnResponseValidationResult validation =
            CharacterTurnResponseValidationPolicy.Validate(
                request.TurnRequest,
                response,
                worldObjects,
                affordances);
        if (!HasSafeResponseCore(request, response)
            || !ReferenceEquals(intention.Type, expectedType)
            || intention.TargetObjectId != expectedTargetObjectId
            || !validation.IsValid)
        {
            return CharacterModelLiveConformanceOutcome.ResponseViolation;
        }

        return CharacterModelLiveConformanceOutcome.Conforming;
    }

    private static bool HasSafeResponseCore(
        CharacterModelGenerationRequest request,
        CharacterTurnResponse response)
    {
        DialogueUtterance? speech = response.Speech;
        return ReferenceEquals(response.Request, request.TurnRequest)
            && speech is not null
            && ReferenceEquals(
                speech.SpeakerCharacterId,
                request.TurnRequest.RespondingCharacterId)
            && response.Commitments.Count == 0
            && response.StateProposals.Count == 0;
    }
}
