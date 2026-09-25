using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Verifies the exact terminal path for the switch-gated ordinary-turn Delay smoke.</summary>
internal static class CharacterSabotageOrdinaryTurnTerminalSmokeVerifier
{
    /// <summary>Runs terminal retention, reset, completion, and admission checks in order.</summary>
    internal static bool TryVerify(
        Character actor,
        CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult,
        CharacterSabotageOrdinaryTurnTerminalRouteCoordinator terminalRoute,
        CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset,
        CharacterSabotageOrdinaryTurnCompletionCoordinator completion,
        CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator admission,
        CharacterSabotageTerminalRouteCoordinator sharedTerminalRoute,
        int actionModelCallCount,
        int delayModelCallCount,
        int recoveryModelCallCount) =>
        Retain(recoveryResult, terminalRoute, sharedTerminalRoute)
        && Reset(recoveryResult, terminalRoute, terminalReset, sharedTerminalRoute)
        && Complete(actor, terminalReset, completion)
        && Admit(terminalReset, completion, admission)
        && actionModelCallCount == 1
        && delayModelCallCount == 1
        && recoveryModelCallCount == 1;

    private static bool Retain(
        CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult,
        CharacterSabotageOrdinaryTurnTerminalRouteCoordinator terminalRoute,
        CharacterSabotageTerminalRouteCoordinator sharedTerminalRoute)
    {
        if (recoveryResult.AuthorRequest is not { } authorRequest
            || recoveryResult.Request is not { } request
            || recoveryResult.Result is not { } result
            || recoveryResult.AppliedResult is not { } appliedResult
            || recoveryResult.EventLog is not { } eventLog
            || recoveryResult.WorldTime is not { } worldTime)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        CharacterTargetSabotageDelayRecoveryConsiderationResult rebuiltResult = new(
            request,
            new CharacterTargetSabotageDelayRecoveryDecision(
                result.Decision.ActorId,
                result.SelectedCandidate));
        CharacterTargetSabotageDelayRecoveryResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool initialState = terminalRoute.AuthorRequest is null
            && terminalRoute.ActionKind is null
            && terminalRoute.DelayRecovery is null
            && sharedTerminalRoute.TerminalOutputCount == 0;
        bool invalidRejected = !terminalRoute.TryRetain(
                null,
                request,
                result,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay)
            && !terminalRoute.TryRetain(
                rebuiltAuthorRequest,
                request,
                result,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay)
            && !terminalRoute.TryRetain(
                authorRequest,
                rebuiltRequest,
                result,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay)
            && !terminalRoute.TryRetain(
                authorRequest,
                request,
                rebuiltResult,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay)
            && !terminalRoute.TryRetain(
                authorRequest,
                request,
                result,
                rebuiltAppliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay)
            && !terminalRoute.TryRetain(
                authorRequest,
                request,
                result,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Influence);
        bool retained = terminalRoute.TryRetain(
            authorRequest,
            request,
            result,
            appliedResult,
            eventLog,
            worldTime,
            CharacterSabotageActionKind.Delay);
        bool exact = ReferenceEquals(terminalRoute.AuthorRequest, authorRequest)
            && ReferenceEquals(terminalRoute.ActionKind, CharacterSabotageActionKind.Delay)
            && ReferenceEquals(terminalRoute.DelayRecovery, appliedResult)
            && ReferenceEquals(sharedTerminalRoute.AuthorRequest, authorRequest)
            && ReferenceEquals(sharedTerminalRoute.ActionKind, CharacterSabotageActionKind.Delay)
            && ReferenceEquals(sharedTerminalRoute.DelayRecovery, appliedResult)
            && sharedTerminalRoute.TerminalOutputCount == 1
            && !terminalRoute.TryRetain(
                authorRequest,
                request,
                result,
                appliedResult,
                eventLog,
                worldTime,
                CharacterSabotageActionKind.Delay);
        return initialState && invalidRejected && retained && exact;
    }

    private static bool Reset(
        CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult,
        CharacterSabotageOrdinaryTurnTerminalRouteCoordinator terminalRoute,
        CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset,
        CharacterSabotageTerminalRouteCoordinator sharedTerminalRoute)
    {
        if (terminalRoute.AuthorRequest is not { } authorRequest
            || terminalRoute.ActionKind is not { } actionKind
            || terminalRoute.DelayRecovery is not { } appliedResult
            || recoveryResult.EventLog is not { } eventLog
            || recoveryResult.WorldTime is not { } worldTime)
        {
            return false;
        }

        CharacterTargetSabotageDelayRecoveryResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool initialState = terminalReset.ResetAuthorRequest is null
            && terminalReset.ResetActionKind is null
            && terminalReset.DelayRecovery is null
            && sharedTerminalRoute.ResetAuthorRequest is null
            && sharedTerminalRoute.ResetActionKind is null;
        bool invalidRejected = !terminalReset.TryReset(
                null,
                actionKind,
                appliedResult,
                eventLog,
                worldTime)
            && !terminalReset.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Influence,
                appliedResult,
                eventLog,
                worldTime)
            && !terminalReset.TryReset(
                authorRequest,
                actionKind,
                rebuiltAppliedResult,
                eventLog,
                worldTime);
        bool reset = terminalReset.TryReset(
            authorRequest,
            actionKind,
            appliedResult,
            eventLog,
            worldTime);
        bool exact = ReferenceEquals(terminalReset.ResetAuthorRequest, authorRequest)
            && ReferenceEquals(terminalReset.ResetActionKind, actionKind)
            && ReferenceEquals(terminalReset.DelayRecovery, appliedResult)
            && sharedTerminalRoute.AuthorRequest is null
            && sharedTerminalRoute.ActionKind is null
            && sharedTerminalRoute.DelayRecovery is null
            && sharedTerminalRoute.TerminalOutputCount == 0
            && ReferenceEquals(sharedTerminalRoute.ResetAuthorRequest, authorRequest)
            && ReferenceEquals(sharedTerminalRoute.ResetActionKind, actionKind)
            && !terminalReset.TryReset(
                authorRequest,
                actionKind,
                appliedResult,
                eventLog,
                worldTime);
        return initialState && invalidRejected && reset && exact;
    }

    private static bool Complete(
        Character actor,
        CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset,
        CharacterSabotageOrdinaryTurnCompletionCoordinator completion)
    {
        if (terminalReset.ResetAuthorRequest is not { } authorRequest
            || terminalReset.ResetActionKind is not { } actionKind
            || terminalReset.DelayRecovery is not { } appliedResult)
        {
            return false;
        }

        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterTargetSabotageDelayRecoveryResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool initialState = completion.Completion is null;
        bool invalidRejected = !completion.TryComplete(null, authorRequest, actionKind, appliedResult)
            && !completion.TryComplete(
                replacementActor,
                authorRequest,
                actionKind,
                appliedResult)
            && !completion.TryComplete(
                actor,
                rebuiltAuthorRequest,
                actionKind,
                appliedResult)
            && !completion.TryComplete(
                actor,
                authorRequest,
                CharacterSabotageActionKind.Influence,
                appliedResult)
            && !completion.TryComplete(
                actor,
                authorRequest,
                actionKind,
                rebuiltAppliedResult);
        bool completed = completion.TryComplete(actor, authorRequest, actionKind, appliedResult);
        bool exact = completion.Completion is { } completedToken
            && ReferenceEquals(completedToken.Actor, actor)
            && ReferenceEquals(completedToken.AuthorRequest, authorRequest)
            && ReferenceEquals(completedToken.ActionKind, actionKind)
            && !completion.TryComplete(actor, authorRequest, actionKind, appliedResult);
        return initialState && invalidRejected && completed && exact;
    }

    private static bool Admit(
        CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset,
        CharacterSabotageOrdinaryTurnCompletionCoordinator completion,
        CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator admission)
    {
        if (completion.Completion is not { } completedToken
            || terminalReset.DelayRecovery is not { } appliedResult)
        {
            return false;
        }

        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completedToken.Actor,
            completedToken.AuthorRequest,
            completedToken.ActionKind);
        CharacterTargetSabotageDelayRecoveryResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool initialState = admission.Admission is null;
        bool invalidRejected = !admission.TryAdmit(null, appliedResult)
            && !admission.TryAdmit(rebuiltCompletion, appliedResult)
            && !admission.TryAdmit(completedToken, rebuiltAppliedResult);
        bool admitted = admission.TryAdmit(completedToken, appliedResult);
        bool exact = ReferenceEquals(admission.Admission, completedToken)
            && !admission.TryAdmit(completedToken, appliedResult);
        return initialState && invalidRejected && admitted && exact;
    }
}
