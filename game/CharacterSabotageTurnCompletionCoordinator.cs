using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains exact provenance for one completed generic sabotage turn.</summary>
public sealed class CharacterSabotageTurnCompletion
{
    /// <summary>Creates one exact scene-owned completion provenance token.</summary>
    internal CharacterSabotageTurnCompletion(
        Character actor,
        CharacterSabotageActionConsiderationRequest authorRequest,
        CharacterSabotageActionKind actionKind)
    {
        Actor = actor;
        AuthorRequest = authorRequest;
        ActionKind = actionKind;
    }

    /// <summary>Gets the exact actor whose completed sabotage route was reset.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact generic author request whose route completed.</summary>
    public CharacterSabotageActionConsiderationRequest AuthorRequest { get; }

    /// <summary>Gets the exact canonical kind whose route completed.</summary>
    public CharacterSabotageActionKind ActionKind { get; }
}

/// <summary>Exposes one exact completed sabotage turn after terminal-route reset.</summary>
public partial class CharacterSabotageTurnCompletionCoordinator : Node
{
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator objectRecovery = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator delayRecovery = null!;
    private CharacterSabotageOutcomeInfluenceRecoveryCoordinator influenceRecovery = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterDialogueMisquotationEvidencePresentationController
        misquotationPresentations = null!;
    private CharacterTargetSabotageDelayRecoveryController delayState = null!;
    private CharacterTargetSabotageInfluenceRecoveryController influenceState = null!;

    /// <summary>Gets the one exact completed-turn token exposed by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Completion { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        objectRecovery = GetNode<CharacterSabotageOutcomeRecoveryCoordinator>(
            "../CharacterSabotageOutcomeRecoveryCoordinator");
        delayRecovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
        influenceRecovery = GetNode<CharacterSabotageOutcomeInfluenceRecoveryCoordinator>(
            "../CharacterSabotageOutcomeInfluenceRecoveryCoordinator");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        misquotationPresentations =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        delayState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
    }

    /// <summary>Exposes completion only for the exact reset route and its current owner.</summary>
    public bool TryComplete(
        Character? expectedActor,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind)
    {
        if (expectedActor is null
            || expectedAuthorRequest is null
            || expectedActionKind is null
            || Completion is not null
            || terminalRoute.AuthorRequest is not null
            || terminalRoute.ActionKind is not null
            || terminalRoute.TerminalOutputCount != 0
            || !ReferenceEquals(terminalRoute.ResetAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalRoute.ResetActionKind, expectedActionKind)
            || !ReferenceEquals(expectedAuthorRequest.Actor, expectedActor)
            || expectedAuthorRequest.ActionKinds.Count != 1
            || !ReferenceEquals(expectedAuthorRequest.ActionKinds[0], expectedActionKind)
            || !IsCanonicalActionKind(expectedActionKind)
            || CurrentRouteOwnerCount != 1
            || !HasCurrentRouteOwner(expectedAuthorRequest, expectedActionKind))
        {
            return false;
        }

        Completion = new CharacterSabotageTurnCompletion(
            expectedActor,
            expectedAuthorRequest,
            expectedActionKind);
        return true;
    }

    /// <summary>Gets the number of existing branch owners retaining terminal output.</summary>
    private int CurrentRouteOwnerCount =>
        (objectRecovery.RecoveredConcealment is null ? 0 : 1)
        + (objectRecovery.RecoveredPlacements is null ? 0 : 1)
        + (objectRecovery.RecoveredSubstitutions is null ? 0 : 1)
        + (objectRecovery.RecoveredContamination is null ? 0 : 1)
        + (objectRecovery.MisquotationPresentation is null ? 0 : 1)
        + (delayRecovery.RecoveredResult is null ? 0 : 1)
        + (influenceRecovery.RecoveredResult is null ? 0 : 1);

    /// <summary>Checks that the canonical branch alone retains its current terminal output.</summary>
    private bool HasCurrentRouteOwner(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        HasCurrentConcealment(expectedAuthorRequest, expectedActionKind)
        || HasCurrentMove(expectedAuthorRequest, expectedActionKind)
        || HasCurrentSubstitution(expectedAuthorRequest, expectedActionKind)
        || HasCurrentContamination(expectedAuthorRequest, expectedActionKind)
        || HasCurrentMisquotation(expectedAuthorRequest, expectedActionKind)
        || HasCurrentDelay(expectedAuthorRequest, expectedActionKind)
        || HasCurrentInfluence(expectedAuthorRequest, expectedActionKind);

    /// <summary>Checks the exact current Conceal route owner.</summary>
    private bool HasCurrentConcealment(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
        && objectRecovery.RecoveredConcealment is { } recovered
        && ReferenceEquals(concealmentState.Concealment, recovered);

    /// <summary>Checks the exact current Move route owner.</summary>
    private bool HasCurrentMove(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
        && objectRecovery.RecoveredPlacements is { } recovered
        && ReferenceEquals(moveState.Placements, recovered);

    /// <summary>Checks the exact current Substitute route owner.</summary>
    private bool HasCurrentSubstitution(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Substitute)
        && ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
        && objectRecovery.RecoveredSubstitutions is { } recovered
        && ReferenceEquals(substitutionState.Substitutions, recovered);

    /// <summary>Checks the exact current Contaminate route owner.</summary>
    private bool HasCurrentContamination(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Contaminate)
        && ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
        && objectRecovery.RecoveredContamination is { } recovered
        && ReferenceEquals(contaminationState.Contamination, recovered);

    /// <summary>Checks the exact current Misquote route owner.</summary>
    private bool HasCurrentMisquotation(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Misquote)
        && ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
        && objectRecovery.MisquotationPresentation is { } presentation
        && ReferenceEquals(misquotationPresentations.Presentation, presentation);

    /// <summary>Checks the exact current Delay route owner.</summary>
    private bool HasCurrentDelay(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Delay)
        && ReferenceEquals(delayRecovery.AuthorRequest, expectedAuthorRequest)
        && delayRecovery.RecoveredResult is { } recovered
        && ReferenceEquals(delayRecovery.RecoveredEventLog, recovered.EventLog)
        && ReferenceEquals(delayRecovery.RecoveredWorldTime, recovered.WorldTime)
        && ReferenceEquals(delayState.LatestAppliedResult, recovered)
        && ReferenceEquals(delayState.EventLog, recovered.EventLog)
        && ReferenceEquals(delayState.WorldTime, recovered.WorldTime);

    /// <summary>Checks the exact current Influence route owner.</summary>
    private bool HasCurrentInfluence(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind) =>
        ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Influence)
        && ReferenceEquals(influenceRecovery.AuthorRequest, expectedAuthorRequest)
        && influenceRecovery.RecoveredResult is { } recovered
        && ReferenceEquals(influenceRecovery.RecoveredRelationship, recovered.Relationship)
        && ReferenceEquals(influenceState.LatestAppliedResult, recovered)
        && ReferenceEquals(influenceState.Relationship, recovered.Relationship);

    /// <summary>Checks that a kind is one exact canonical singleton.</summary>
    private static bool IsCanonicalActionKind(CharacterSabotageActionKind actionKind)
    {
        foreach (CharacterSabotageActionKind canonicalKind in CharacterSabotageActionKind.All)
        {
            if (ReferenceEquals(canonicalKind, actionKind))
            {
                return true;
            }
        }

        return false;
    }
}
