using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks completion of the exact reserved Delay recovery.</summary>
public partial class CharacterSabotageTurnOpportunityControllerSmokeCheck
{
    private void VerifyCompletedRecovery()
    {
        CharacterTurnActivityController turns =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterSabotageTurnReservation? reservation = turns.SabotageReservation;
        CharacterTargetSabotageDelayRecoveryTaskController task =
            GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
                "../CharacterTargetSabotageDelayRecoveryTaskController");
        if (task.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageOrdinaryTurnResultBindingCoordinator binding =
            GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
                "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator recoveryRequest =
            GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult =
            GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator");
        if (reservation is null
            || binding.SelectedAuthorRequest is not { } authorRequest
            || recoveryRequest.Request is not { } request
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || recoveryModel?.CallCount != 1
            || !ReferenceEquals(recoveryModel.Request, request))
        {
            Fail("The exact reserved Delay recovery did not complete once.");
            return;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        bool nullRejected = !recoveryResult.TryRetain(null, authorRequest, request);
        bool rebuiltReservationRejected = !recoveryResult.TryRetain(
            new CharacterSabotageTurnReservation(reservation.Opportunity),
            authorRequest,
            request);
        bool rebuiltRequestRejected = !recoveryResult.TryRetain(
            reservation,
            authorRequest,
            rebuiltRequest);
        bool retained = recoveryResult.TryRetain(reservation, authorRequest, request);
        bool repeatedRejected = !recoveryResult.TryRetain(reservation, authorRequest, request);
        if (!nullRejected
            || !rebuiltReservationRejected
            || !rebuiltRequestRejected
            || !retained
            || !ReferenceEquals(recoveryResult.Reservation, reservation)
            || !ReferenceEquals(recoveryResult.AuthorRequest, authorRequest)
            || !ReferenceEquals(recoveryResult.Request, request)
            || !ReferenceEquals(recoveryResult.Result, result)
            || recoveryResult.AppliedResult is not { } appliedResult
            || !ReferenceEquals(recoveryResult.EventLog, appliedResult.EventLog)
            || !ReferenceEquals(recoveryResult.WorldTime, appliedResult.WorldTime)
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || task.Result is not null
            || !repeatedRejected
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(turns.SabotageReservation, reservation))
        {
            Fail("The reserved Delay recovery was not retained and acknowledged exactly once.");
            return;
        }

        CharacterSabotageOrdinaryTurnTerminalRouteCoordinator terminalRoute =
            GetNode<CharacterSabotageOrdinaryTurnTerminalRouteCoordinator>(
                "../CharacterSabotageOrdinaryTurnTerminalRouteCoordinator");
        bool nullRouteRejected = !terminalRoute.TryRetain(
            null,
            authorRequest,
            request,
            result,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime,
            CharacterSabotageActionKind.Delay);
        bool rebuiltRouteReservationRejected = !terminalRoute.TryRetain(
            new CharacterSabotageTurnReservation(reservation.Opportunity),
            authorRequest,
            request,
            result,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime,
            CharacterSabotageActionKind.Delay);
        bool routeRetained = terminalRoute.TryRetain(
            reservation,
            authorRequest,
            request,
            result,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime,
            CharacterSabotageActionKind.Delay);
        bool repeatedRouteRejected = !terminalRoute.TryRetain(
            reservation,
            authorRequest,
            request,
            result,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime,
            CharacterSabotageActionKind.Delay);
        if (!nullRouteRejected
            || !rebuiltRouteReservationRejected
            || !routeRetained
            || !ReferenceEquals(terminalRoute.Reservation, reservation)
            || !ReferenceEquals(terminalRoute.AuthorRequest, authorRequest)
            || !ReferenceEquals(terminalRoute.ActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(terminalRoute.DelayRecovery, appliedResult)
            || !repeatedRouteRejected
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(turns.SabotageReservation, reservation))
        {
            Fail(
                "The reserved Delay terminal route was not retained exactly once. "
                + $"null={nullRouteRejected}; rebuilt={rebuiltRouteReservationRejected}; "
                + $"retained={routeRetained}; repeated={repeatedRouteRejected}; "
                + $"reservation={ReferenceEquals(terminalRoute.Reservation, reservation)}; "
                + $"author={ReferenceEquals(terminalRoute.AuthorRequest, authorRequest)}; "
                + $"kind={ReferenceEquals(terminalRoute.ActionKind, CharacterSabotageActionKind.Delay)}; "
                + $"recovery={ReferenceEquals(terminalRoute.DelayRecovery, appliedResult)}.");
            return;
        }

        CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset =
            GetNode<CharacterSabotageOrdinaryTurnTerminalResetCoordinator>(
                "../CharacterSabotageOrdinaryTurnTerminalResetCoordinator");
        bool nullResetRejected = !terminalReset.TryReset(
            null,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool rebuiltResetReservationRejected = !terminalReset.TryReset(
            new CharacterSabotageTurnReservation(reservation.Opportunity),
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool routeReset = terminalReset.TryReset(
            reservation,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool repeatedResetRejected = !terminalReset.TryReset(
            reservation,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        if (!nullResetRejected
            || !rebuiltResetReservationRejected
            || !routeReset
            || !ReferenceEquals(terminalReset.Reservation, reservation)
            || !ReferenceEquals(terminalReset.ResetAuthorRequest, authorRequest)
            || !ReferenceEquals(terminalReset.ResetActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(terminalReset.DelayRecovery, appliedResult)
            || !ReferenceEquals(recoveryResult.EventLog, appliedResult.EventLog)
            || !ReferenceEquals(recoveryResult.WorldTime, appliedResult.WorldTime)
            || !repeatedResetRejected
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(turns.SabotageReservation, reservation))
        {
            Fail("The reserved Delay terminal route was not reset exactly once.");
            return;
        }

        CharacterSabotageOrdinaryTurnCompletionCoordinator completion =
            GetNode<CharacterSabotageOrdinaryTurnCompletionCoordinator>(
                "../CharacterSabotageOrdinaryTurnCompletionCoordinator");
        bool nullCompletionRejected = !completion.TryComplete(
            null,
            reservation.Opportunity.Actor,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult);
        bool rebuiltCompletionReservationRejected = !completion.TryComplete(
            new CharacterSabotageTurnReservation(reservation.Opportunity),
            reservation.Opportunity.Actor,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult);
        bool completed = completion.TryComplete(
            reservation,
            reservation.Opportunity.Actor,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult);
        bool repeatedCompletionRejected = !completion.TryComplete(
            reservation,
            reservation.Opportunity.Actor,
            authorRequest,
            CharacterSabotageActionKind.Delay,
            appliedResult);
        if (!nullCompletionRejected
            || !rebuiltCompletionReservationRejected
            || !completed
            || !ReferenceEquals(completion.Reservation, reservation)
            || completion.Completion is not { } completedTurn
            || !ReferenceEquals(completedTurn.Actor, reservation.Opportunity.Actor)
            || !ReferenceEquals(completedTurn.AuthorRequest, authorRequest)
            || !ReferenceEquals(completedTurn.ActionKind, CharacterSabotageActionKind.Delay)
            || !repeatedCompletionRejected
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(turns.SabotageReservation, reservation))
        {
            Fail("The reserved Delay turn did not expose one exact completion token.");
            return;
        }

        CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator admission =
            GetNode<CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator>(
                "../CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator");
        bool nullAdmissionRejected = !admission.TryAdmit(
            null,
            completedTurn,
            appliedResult);
        bool rebuiltAdmissionReservationRejected = !admission.TryAdmit(
            new CharacterSabotageTurnReservation(reservation.Opportunity),
            completedTurn,
            appliedResult);
        bool admitted = admission.TryAdmit(reservation, completedTurn, appliedResult);
        bool repeatedAdmissionRejected = !admission.TryAdmit(
            reservation,
            completedTurn,
            appliedResult);
        if (!nullAdmissionRejected
            || !rebuiltAdmissionReservationRejected
            || !admitted
            || !ReferenceEquals(admission.Reservation, reservation)
            || !ReferenceEquals(admission.Admission, completedTurn)
            || !repeatedAdmissionRejected
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(turns.SabotageReservation, reservation))
        {
            Fail("The reserved Delay completion was not admitted exactly once.");
            return;
        }

        CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator resolution =
            GetNode<CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator>(
                "../CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator");
        CharacterSabotageOrdinaryTurnBoundaryCoordinator boundary =
            GetNode<CharacterSabotageOrdinaryTurnBoundaryCoordinator>(
                "../CharacterSabotageOrdinaryTurnBoundaryCoordinator");
        CharacterTurnActivity awaitingTurn = CreateReservedTurn(reservation.Opportunity.Actor);
        CharacterTurnActivity rebuiltTurn = new(
            awaitingTurn.Request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completedTurn.Actor,
            completedTurn.AuthorRequest,
            completedTurn.ActionKind);
        CharacterTargetSabotageDelayRecoveryResult rebuiltRecovery = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool activityStarted = !turns.TryStartReservedSabotageTurn(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                awaitingTurn)
            && turns.TryStartReservedSabotageTurn(reservation, awaitingTurn)
            && turns.Activity is null
            && ReferenceEquals(turns.ReservedSabotageTurn, awaitingTurn)
            && !turns.TryStartReservedSabotageTurn(reservation, rebuiltTurn);
        bool activeBoundaryRejected = activityStarted
            && !boundary.TryBindAndRelease(
                reservation,
                completedTurn,
                awaitingTurn);
        bool invalidResolutionRejected = !resolution.TryResolve(
                null,
                awaitingTurn,
                completedTurn,
                appliedResult)
            && !resolution.TryResolve(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                awaitingTurn,
                completedTurn,
                appliedResult)
            && !resolution.TryResolve(
                reservation,
                rebuiltTurn,
                completedTurn,
                appliedResult)
            && !resolution.TryResolve(
                reservation,
                awaitingTurn,
                rebuiltCompletion,
                appliedResult)
            && !resolution.TryResolve(
                reservation,
                awaitingTurn,
                completedTurn,
                rebuiltRecovery);
        bool resolved = resolution.TryResolve(
            reservation,
            awaitingTurn,
            completedTurn,
            appliedResult);
        CharacterTurnActivity? resolvedTurn = resolution.ResolvedTurn;
        bool exactResolution = resolvedTurn is not null
            && ReferenceEquals(resolution.Reservation, reservation)
            && ReferenceEquals(resolution.OriginalTurn, awaitingTurn)
            && ReferenceEquals(resolution.Completion, completedTurn)
            && ReferenceEquals(resolution.DelayRecovery, appliedResult)
            && ReferenceEquals(turns.Activity, resolvedTurn)
            && ReferenceEquals(resolvedTurn.Request, awaitingTurn.Request)
            && resolvedTurn.Response is null
            && ReferenceEquals(resolvedTurn.State, CharacterTurnActivityState.Resolved)
            && !resolution.TryResolve(
                reservation,
                awaitingTurn,
                completedTurn,
                appliedResult);
        bool released = resolvedTurn is not null
            && !boundary.TryBindAndRelease(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                completedTurn,
                resolvedTurn)
            && !boundary.TryBindAndRelease(reservation, rebuiltCompletion, resolvedTurn)
            && boundary.TryBindAndRelease(reservation, completedTurn, resolvedTurn)
            && ReferenceEquals(boundary.Reservation, reservation)
            && ReferenceEquals(boundary.Admission, completedTurn)
            && ReferenceEquals(boundary.TurnActivity, resolvedTurn)
            && turns.SabotageReservation is null
            && !boundary.TryBindAndRelease(reservation, completedTurn, resolvedTurn);
        TemporaryCharacterTargetSabotageInvestigationSource investigationSource =
            GetNode<TemporaryCharacterTargetSabotageInvestigationSource>(
                "../TemporaryCharacterTargetSabotageInvestigationSource");
        CharacterSabotageOrdinaryTurnDelayInvestigationCoordinator investigation =
            GetNode<CharacterSabotageOrdinaryTurnDelayInvestigationCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayInvestigationCoordinator");
        CharacterInteractionAccessSnapshot rebuiltAccess = new(
            investigationSource.Investigator.Identity.Id,
            investigationSource.InteractionAccess.ReachableTargetCharacterIds);
        bool investigated = !investigation.TryInvestigate(
                null,
                appliedResult,
                investigationSource.Investigator,
                investigationSource.InteractionAccess,
                investigationSource.DelayEvidenceId)
            && !investigation.TryInvestigate(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                appliedResult,
                investigationSource.Investigator,
                investigationSource.InteractionAccess,
                investigationSource.DelayEvidenceId)
            && !investigation.TryInvestigate(
                reservation,
                rebuiltRecovery,
                investigationSource.Investigator,
                investigationSource.InteractionAccess,
                investigationSource.DelayEvidenceId)
            && !investigation.TryInvestigate(
                reservation,
                appliedResult,
                investigationSource.Investigator,
                rebuiltAccess,
                investigationSource.DelayEvidenceId)
            && investigation.TryInvestigate(
                reservation,
                appliedResult,
                investigationSource.Investigator,
                investigationSource.InteractionAccess,
                investigationSource.DelayEvidenceId)
            && investigation.Candidate is { } investigationCandidate
            && ReferenceEquals(investigationCandidate.RecoveryResult, appliedResult)
            && ReferenceEquals(
                investigationCandidate.EvidenceId,
                investigationSource.DelayEvidenceId)
            && investigation.EvidenceView is { } evidenceView
            && ReferenceEquals(
                evidenceView.CharacterId,
                investigationSource.Investigator.Identity.Id)
            && evidenceView.EvidenceIds.Count == 1
            && ReferenceEquals(
                evidenceView.EvidenceIds[0],
                investigationSource.DelayEvidenceId)
            && !investigation.TryInvestigate(
                reservation,
                appliedResult,
                investigationSource.Investigator,
                investigationSource.InteractionAccess,
                investigationSource.DelayEvidenceId);
        if (!activeBoundaryRejected
            || !invalidResolutionRejected
            || !resolved
            || !exactResolution
            || !released
            || !investigated
            || recoveryModel.CallCount != 1)
        {
            Fail("The reserved Delay turn did not resolve, bind, and release exactly once.");
            return;
        }

        GD.Print(
            "Sabotage-turn opportunity controller smoke passed: ordinary startup stayed inert; "
            + "one exact reserved Delay route completed its generic and typed decisions, applied "
            + "and recovered once, and retained fresh authoritative recovery snapshots while "
            + "resetting only the completed recovery task, retaining then resetting one exact "
            + "terminal route, exposing and admitting one exact completed-turn token, resolving "
            + "its response-free activity, binding its exact terminal successor, and releasing "
            + "only its reservation before one exact interaction-access investigation exposed "
            + "authored evidence without scheduling another action.");
        GetTree().Quit();
    }

    private static CharacterTurnActivity CreateReservedTurn(Character actor)
    {
        CharacterId speakerId = new("reserved-sabotage-turn-smoke-speaker");
        DialogueUtterance utterance = new(speakerId, "The road remains open.");
        CharacterTurnRequest request = new(
            actor.Identity.Id,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(speakerId, [actor.Identity.Id])),
            new DialogueAct(utterance, DialogueActType.ReportedClaim));
        return new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
    }
}
