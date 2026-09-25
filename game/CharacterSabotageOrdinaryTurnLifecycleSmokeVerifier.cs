using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Verifies exact ordinary-turn action resolution and terminal boundary binding.</summary>
internal static class CharacterSabotageOrdinaryTurnLifecycleSmokeVerifier
{
    /// <summary>Rejects active and substitute inputs, then binds only the exact terminal successor.</summary>
    internal static bool TryVerify(
        CharacterTurnActivity turn,
        CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult,
        CharacterSabotageOrdinaryTurnCompletionCoordinator completion,
        CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator admission,
        CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator resolution,
        CharacterSabotageOrdinaryTurnBoundaryCoordinator boundary,
        CharacterTurnActivityController turns,
        int actionModelCallCount,
        int delayModelCallCount,
        int recoveryModelCallCount)
    {
        if (completion.Completion is not { } completed
            || admission.Admission is not { } admitted
            || recoveryResult.AppliedResult is not { } recovered
            || !ReferenceEquals(completed, admitted))
        {
            return false;
        }

        CharacterTurnActivity rebuiltTurn = new(
            turn.Request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
        CharacterTurnActivity alreadyResolved = new(
            turn.Request,
            null,
            CharacterTurnActivityState.Resolved);
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completed.Actor,
            completed.AuthorRequest,
            completed.ActionKind);
        CharacterTargetSabotageDelayRecoveryResult rebuiltRecovery = new(
            recovered.Candidate,
            recovered.EventLog,
            recovered.WorldTime);
        bool activeRejected = !boundary.TryBind(completed, turn)
            && boundary.Admission is null
            && boundary.TurnActivity is null;
        bool invalidRejected = !resolution.TryResolve(null, completed, recovered)
            && !resolution.TryResolve(rebuiltTurn, completed, recovered)
            && !resolution.TryResolve(alreadyResolved, completed, recovered)
            && !resolution.TryResolve(turn, rebuiltCompletion, recovered)
            && !resolution.TryResolve(turn, completed, rebuiltRecovery);
        bool resolved = resolution.TryResolve(turn, completed, recovered);
        CharacterTurnActivity? successor = resolution.ResolvedTurn;
        bool exactResolution = successor is not null
            && ReferenceEquals(resolution.OriginalTurn, turn)
            && ReferenceEquals(resolution.Completion, completed)
            && ReferenceEquals(resolution.DelayRecovery, recovered)
            && ReferenceEquals(turns.Activity, successor)
            && !ReferenceEquals(successor, turn)
            && ReferenceEquals(successor.Request, turn.Request)
            && successor.Response is null
            && ReferenceEquals(successor.State, CharacterTurnActivityState.Resolved)
            && CharacterTurnStartPolicy.CanStart(successor)
            && !resolution.TryResolve(turn, completed, recovered);
        bool boundaryBound = successor is not null
            && !boundary.TryBind(completed, turn)
            && boundary.TryBind(completed, successor)
            && ReferenceEquals(boundary.Admission, completed)
            && ReferenceEquals(boundary.TurnActivity, successor)
            && !boundary.TryBind(completed, successor);

        return activeRejected
            && invalidRejected
            && resolved
            && exactResolution
            && boundaryBound
            && actionModelCallCount == 1
            && delayModelCallCount == 1
            && recoveryModelCallCount == 1;
    }
}
