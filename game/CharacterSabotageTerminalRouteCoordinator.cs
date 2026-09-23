using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact completed generic sabotage route without scheduling another.</summary>
public partial class CharacterSabotageTerminalRouteCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
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

    /// <summary>Gets the exact selected generic author request retained at terminal state.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind retained with the terminal route.</summary>
    public CharacterSabotageActionKind? ActionKind { get; private set; }

    /// <summary>Gets the exact author request whose retained terminal route was reset.</summary>
    public CharacterSabotageActionConsiderationRequest? ResetAuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind whose retained terminal route was reset.</summary>
    public CharacterSabotageActionKind? ResetActionKind { get; private set; }

    /// <summary>Gets the exact recovered concealment retained for the Conceal route.</summary>
    public WorldObjectConcealmentSnapshot? RecoveredConcealment { get; private set; }

    /// <summary>Gets the exact recovered placements retained for the Move route.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <summary>Gets the exact recovered substitutions retained for the Substitute route.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot? RecoveredSubstitutions
    {
        get;
        private set;
    }

    /// <summary>Gets the exact recovered contamination retained for the Contaminate route.</summary>
    public WorldObjectContaminationSnapshot? RecoveredContamination { get; private set; }

    /// <summary>Gets the exact presentation retained for the Misquote route.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSnapshot? MisquotationPresentation
    {
        get;
        private set;
    }

    /// <summary>Gets the exact applied recovery retained for the Delay route.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? DelayRecovery { get; private set; }

    /// <summary>Gets the exact applied recovery retained for the Influence route.</summary>
    public CharacterTargetSabotageInfluenceRecoveryResult? InfluenceRecovery { get; private set; }

    /// <summary>Gets the number of exact terminal branch outputs retained by this one-shot seam.</summary>
    public int TerminalOutputCount =>
        (RecoveredConcealment is null ? 0 : 1)
        + (RecoveredPlacements is null ? 0 : 1)
        + (RecoveredSubstitutions is null ? 0 : 1)
        + (RecoveredContamination is null ? 0 : 1)
        + (MisquotationPresentation is null ? 0 : 1)
        + (DelayRecovery is null ? 0 : 1)
        + (InfluenceRecovery is null ? 0 : 1);

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
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

    /// <summary>Retains only the exact current output of one completed canonical route.</summary>
    public bool TryRetain(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind)
    {
        if (expectedAuthorRequest is null
            || expectedActionKind is null
            || AuthorRequest is not null
            || ActionKind is not null
            || ResetAuthorRequest is not null
            || ResetActionKind is not null
            || TerminalOutputCount != 0
            || CurrentRouteOutputCount != 1
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                expectedActionKind)
            || binding.SelectedCandidateBinding is not { } selectedBinding
            || !ReferenceEquals(selectedBinding.Request, expectedAuthorRequest)
            || !ReferenceEquals(selectedBinding.ActionKind, expectedActionKind))
        {
            return false;
        }

        bool retained = TryRetainConcealment(expectedAuthorRequest, expectedActionKind)
            || TryRetainMove(expectedAuthorRequest, expectedActionKind)
            || TryRetainSubstitution(expectedAuthorRequest, expectedActionKind)
            || TryRetainContamination(expectedAuthorRequest, expectedActionKind)
            || TryRetainMisquotation(expectedAuthorRequest, expectedActionKind)
            || TryRetainDelay(expectedAuthorRequest, expectedActionKind)
            || TryRetainInfluence(expectedAuthorRequest, expectedActionKind);
        if (retained)
        {
            AuthorRequest = expectedAuthorRequest;
            ActionKind = expectedActionKind;
        }

        return retained;
    }

    /// <summary>Clears only this seam's exact retained terminal route once.</summary>
    public bool TryReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind)
    {
        if (expectedAuthorRequest is null
            || expectedActionKind is null
            || !ReferenceEquals(AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(ActionKind, expectedActionKind)
            || ResetAuthorRequest is not null
            || ResetActionKind is not null
            || TerminalOutputCount != 1)
        {
            return false;
        }

        AuthorRequest = null;
        ActionKind = null;
        RecoveredConcealment = null;
        RecoveredPlacements = null;
        RecoveredSubstitutions = null;
        RecoveredContamination = null;
        MisquotationPresentation = null;
        DelayRecovery = null;
        InfluenceRecovery = null;
        ResetAuthorRequest = expectedAuthorRequest;
        ResetActionKind = expectedActionKind;
        return true;
    }

    private int CurrentRouteOutputCount =>
        (objectRecovery.RecoveredConcealment is null ? 0 : 1)
        + (objectRecovery.RecoveredPlacements is null ? 0 : 1)
        + (objectRecovery.RecoveredSubstitutions is null ? 0 : 1)
        + (objectRecovery.RecoveredContamination is null ? 0 : 1)
        + (objectRecovery.MisquotationPresentation is null ? 0 : 1)
        + (delayRecovery.RecoveredResult is null ? 0 : 1)
        + (influenceRecovery.RecoveredResult is null ? 0 : 1);

    private bool TryRetainConcealment(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Conceal)
            || !ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
            || objectRecovery.RecoveredConcealment is not { } recovered
            || !ReferenceEquals(concealmentState.Concealment, recovered))
        {
            return false;
        }

        RecoveredConcealment = recovered;
        return true;
    }

    private bool TryRetainMove(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
            || objectRecovery.RecoveredPlacements is not { } recovered
            || !ReferenceEquals(moveState.Placements, recovered))
        {
            return false;
        }

        RecoveredPlacements = recovered;
        return true;
    }

    private bool TryRetainSubstitution(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Substitute)
            || !ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
            || objectRecovery.RecoveredSubstitutions is not { } recovered
            || !ReferenceEquals(substitutionState.Substitutions, recovered))
        {
            return false;
        }

        RecoveredSubstitutions = recovered;
        return true;
    }

    private bool TryRetainContamination(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Contaminate)
            || !ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
            || objectRecovery.RecoveredContamination is not { } recovered
            || !ReferenceEquals(contaminationState.Contamination, recovered))
        {
            return false;
        }

        RecoveredContamination = recovered;
        return true;
    }

    private bool TryRetainMisquotation(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Misquote)
            || !ReferenceEquals(objectRecovery.AuthorRequest, expectedAuthorRequest)
            || objectRecovery.MisquotationPresentation is not { } presentation
            || !ReferenceEquals(misquotationPresentations.Presentation, presentation))
        {
            return false;
        }

        MisquotationPresentation = presentation;
        return true;
    }

    private bool TryRetainDelay(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(delayRecovery.AuthorRequest, expectedAuthorRequest)
            || delayRecovery.RecoveredResult is not { } recovered
            || !ReferenceEquals(delayRecovery.RecoveredEventLog, recovered.EventLog)
            || !ReferenceEquals(delayRecovery.RecoveredWorldTime, recovered.WorldTime)
            || !ReferenceEquals(delayState.LatestAppliedResult, recovered)
            || !ReferenceEquals(delayState.EventLog, recovered.EventLog)
            || !ReferenceEquals(delayState.WorldTime, recovered.WorldTime))
        {
            return false;
        }

        DelayRecovery = recovered;
        return true;
    }

    private bool TryRetainInfluence(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionKind expectedActionKind)
    {
        if (!ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Influence)
            || !ReferenceEquals(influenceRecovery.AuthorRequest, expectedAuthorRequest)
            || influenceRecovery.RecoveredResult is not { } recovered
            || !ReferenceEquals(influenceRecovery.RecoveredRelationship, recovered.Relationship)
            || !ReferenceEquals(influenceState.LatestAppliedResult, recovered)
            || !ReferenceEquals(influenceState.Relationship, recovered.Relationship))
        {
            return false;
        }

        InfluenceRecovery = recovered;
        return true;
    }
}
