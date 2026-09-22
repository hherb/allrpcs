using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one acknowledged generic sabotage choice into its typed request.</summary>
public partial class CharacterSabotageActionSpecificRequestCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private TemporaryCharacterObjectSabotageSource concealmentSource = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private TemporaryCharacterObjectSabotageMoveSource moveSource = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private TemporaryCharacterObjectSabotageSubstitutionSource substitutionSource = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private TemporaryCharacterObjectSabotageContaminationSource contaminationSource = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceSource misquotationSource = null!;
    private CharacterSabotageActionDelayRequestController delayAuthor = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterSabotageActionInfluenceRequestController influenceAuthor = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;

    /// <summary>Gets the exact composed concealment request, if concealment was selected.</summary>
    public CharacterObjectSabotageConcealmentConsiderationRequest? ConcealmentRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact composed move request, if movement was selected.</summary>
    public CharacterObjectSabotageMoveConsiderationRequest? MoveRequest { get; private set; }

    /// <summary>Gets the exact composed substitution request, if substitution was selected.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationRequest? SubstitutionRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact composed contamination request, if contamination was selected.</summary>
    public CharacterObjectSabotageContaminationConsiderationRequest? ContaminationRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact composed misquotation request, if misquotation was selected.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationRequest? MisquotationRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact composed delay request, if delay was selected.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest? DelayRequest { get; private set; }

    /// <summary>Gets the exact composed influence request, if influence was selected.</summary>
    public CharacterTargetSabotageInfluenceConsiderationRequest? InfluenceRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the number of action-specific requests composed by this one-shot seam.</summary>
    public int RequestCount =>
        (ConcealmentRequest is null ? 0 : 1)
        + (MoveRequest is null ? 0 : 1)
        + (SubstitutionRequest is null ? 0 : 1)
        + (ContaminationRequest is null ? 0 : 1)
        + (MisquotationRequest is null ? 0 : 1)
        + (DelayRequest is null ? 0 : 1)
        + (InfluenceRequest is null ? 0 : 1);

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        concealmentSource = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        moveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        substitutionSource = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        contaminationSource = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        misquotationSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        delayAuthor = GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        influenceAuthor = GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
    }

    /// <summary>Composes the one exact selected action after its generic result was acknowledged.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationRequest? selectedAuthorRequest,
        CharacterSabotageActionCandidateBinding? selectedCandidateBinding)
    {
        CharacterSabotageActionConsiderationResult? acknowledged = binding.AcknowledgedResult;
        if (selectedAuthorRequest is null
            || selectedCandidateBinding is null
            || acknowledged?.SelectedActionKind is not { } selectedKind
            || RequestCount != 0
            || !ReferenceEquals(binding.SelectedAuthorRequest, selectedAuthorRequest)
            || !ReferenceEquals(binding.SelectedCandidateBinding, selectedCandidateBinding)
            || !ReferenceEquals(selectedCandidateBinding.Request, selectedAuthorRequest)
            || !ReferenceEquals(selectedCandidateBinding.ActionKind, selectedKind))
        {
            return false;
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Conceal)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    concealment)
        {
            return TryComposeConcealment(selectedAuthorRequest, concealment.Candidate);
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Move)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    move)
        {
            return TryComposeMove(selectedAuthorRequest, move.Candidate);
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Substitute)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> substitution)
        {
            return TryComposeSubstitution(selectedAuthorRequest, substitution.Candidate);
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Contaminate)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    contamination)
        {
            return TryComposeContamination(selectedAuthorRequest, contamination.Candidate);
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Misquote)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> misquotation)
        {
            return TryComposeMisquotation(selectedAuthorRequest, misquotation.Candidate);
        }

        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Delay)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    delay)
        {
            return TryComposeDelay(selectedAuthorRequest, delay.Candidate);
        }

        return ReferenceEquals(selectedKind, CharacterSabotageActionKind.Influence)
            && selectedCandidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> influence
            && TryComposeInfluence(selectedAuthorRequest, influence.Candidate);
    }

    private bool TryComposeConcealment(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterObjectSabotageEvidence candidate)
    {
        WorldObjectConcealmentSnapshot? concealment = concealmentState.Concealment;
        DiscoveryEvidenceSnapshot? evidence = concealmentState.Evidence;
        if (!ReferenceEquals(selectedRequest.Actor, concealmentSource.Character)
            || !ReferenceEquals(candidate, concealmentSource.Candidate)
            || !ReferenceEquals(concealment, concealmentSource.Concealment)
            || !ReferenceEquals(evidence, concealmentSource.Evidence)
            || !CharacterObjectSabotageConcealmentPolicy.CanApply(
                concealmentSource.Character,
                concealmentSource.PhysicalAccess,
                concealmentSource.WorldObjects,
                concealment,
                evidence,
                candidate))
        {
            return false;
        }

        ConcealmentRequest = new CharacterObjectSabotageConcealmentConsiderationRequest(
            concealmentSource.Character,
            concealmentSource.PhysicalAccess,
            concealmentSource.WorldObjects,
            concealment,
            evidence,
            [candidate]);
        return true;
    }

    private bool TryComposeMove(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterObjectSabotageMoveCandidate candidate)
    {
        WorldObjectPlacementSnapshot? placements = moveState.Placements;
        DiscoveryEvidenceSnapshot? evidence = moveState.Evidence;
        if (!ReferenceEquals(selectedRequest.Actor, moveSource.Character)
            || !ReferenceEquals(candidate, moveSource.Candidate)
            || !ReferenceEquals(placements, moveSource.Placements)
            || !ReferenceEquals(evidence, moveSource.Evidence)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                moveSource.Character,
                moveSource.PhysicalAccess,
                moveSource.WorldObjects,
                placements,
                evidence,
                candidate))
        {
            return false;
        }

        MoveRequest = new CharacterObjectSabotageMoveConsiderationRequest(
            moveSource.Character,
            moveSource.PhysicalAccess,
            moveSource.WorldObjects,
            placements,
            evidence,
            [candidate]);
        return true;
    }

    private bool TryComposeSubstitution(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        CharacterObjectSabotageSubstitutionSnapshot? substitutions =
            substitutionState.Substitutions;
        DiscoveryEvidenceSnapshot? evidence = substitutionState.Evidence;
        if (!ReferenceEquals(selectedRequest.Actor, substitutionSource.Character)
            || !ReferenceEquals(candidate, substitutionSource.Candidate)
            || !ReferenceEquals(substitutions, substitutionSource.Substitutions)
            || !ReferenceEquals(evidence, substitutionSource.Evidence)
            || !CharacterObjectSabotageSubstitutionPolicy.CanApply(
                substitutionSource.Character,
                substitutionSource.PhysicalAccess,
                substitutionSource.WorldObjects,
                substitutions,
                evidence,
                candidate))
        {
            return false;
        }

        SubstitutionRequest = new CharacterObjectSabotageSubstitutionConsiderationRequest(
            substitutionSource.Character,
            substitutionSource.PhysicalAccess,
            substitutionSource.WorldObjects,
            substitutions,
            evidence,
            [candidate]);
        return true;
    }

    private bool TryComposeContamination(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterObjectSabotageEvidence candidate)
    {
        WorldObjectContaminationSnapshot? contamination = contaminationState.Contamination;
        DiscoveryEvidenceSnapshot? evidence = contaminationState.Evidence;
        if (!ReferenceEquals(selectedRequest.Actor, contaminationSource.Character)
            || !ReferenceEquals(candidate, contaminationSource.Candidate)
            || !ReferenceEquals(contamination, contaminationSource.Contamination)
            || !ReferenceEquals(evidence, contaminationSource.Evidence)
            || !CharacterObjectSabotageContaminationPolicy.CanApply(
                contaminationSource.Character,
                contaminationSource.PhysicalAccess,
                contaminationSource.WorldObjects,
                contamination,
                evidence,
                candidate))
        {
            return false;
        }

        ContaminationRequest = new CharacterObjectSabotageContaminationConsiderationRequest(
            contaminationSource.Character,
            contaminationSource.PhysicalAccess,
            contaminationSource.WorldObjects,
            contamination,
            evidence,
            [candidate]);
        return true;
    }

    private bool TryComposeMisquotation(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterDialogueMisquotationEvidence candidate)
    {
        if (!ReferenceEquals(selectedRequest.Actor, misquotationSource.Actor)
            || !ReferenceEquals(candidate, misquotationSource.Candidate)
            || !ReferenceEquals(candidate.Occurrence, misquotationSource.Occurrence)
            || !ReferenceEquals(
                candidate.Occurrence.Proposal.SourceEvent,
                misquotationSource.SourceEvent)
            || !ReferenceEquals(
                candidate.Occurrence.DeliveredEvent,
                misquotationSource.DeliveredEvent)
            || !ReferenceEquals(candidate.WitnessCharacterId, misquotationSource.WitnessCharacterId)
            || !CharacterDialogueMisquotationEvidencePolicy.CanObserve(
                misquotationSource.Actor,
                candidate))
        {
            return false;
        }

        MisquotationRequest = new CharacterDialogueMisquotationEvidenceConsiderationRequest(
            misquotationSource.Actor,
            [candidate]);
        return true;
    }

    private bool TryComposeDelay(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterTargetSabotageDelayCandidate candidate)
    {
        CharacterTargetSabotageDelayConsiderationRequest? sourceRequest =
            delayAuthor.SourceRequest;
        if (sourceRequest is null
            || !ReferenceEquals(selectedRequest.Actor, sourceRequest.Actor)
            || !sourceRequest.Candidates.Any(value => ReferenceEquals(value, candidate))
            || !ReferenceEquals(delayState.EventLog, sourceRequest.EventLog)
            || !ReferenceEquals(delayState.WorldTime, sourceRequest.WorldTime)
            || !CharacterTargetSabotageDelayPolicy.CanApply(
                sourceRequest.Actor,
                sourceRequest.InteractionAccess,
                sourceRequest.WorldEvents,
                sourceRequest.EventLog,
                sourceRequest.WorldTime,
                candidate))
        {
            return false;
        }

        DelayRequest = new CharacterTargetSabotageDelayConsiderationRequest(
            sourceRequest.Actor,
            sourceRequest.InteractionAccess,
            sourceRequest.WorldEvents,
            sourceRequest.EventLog,
            sourceRequest.WorldTime,
            [candidate]);
        return true;
    }

    private bool TryComposeInfluence(
        CharacterSabotageActionConsiderationRequest selectedRequest,
        CharacterTargetSabotageInfluenceCandidate candidate)
    {
        CharacterTargetSabotageInfluenceConsiderationRequest? sourceRequest =
            influenceAuthor.SourceRequest;
        if (sourceRequest is null
            || !ReferenceEquals(selectedRequest.Actor, sourceRequest.Actor)
            || !sourceRequest.Candidates.Any(value => ReferenceEquals(value, candidate))
            || !ReferenceEquals(influenceState.Relationship, sourceRequest.Relationship)
            || !CharacterTargetSabotageInfluencePolicy.CanApply(
                sourceRequest.Actor,
                sourceRequest.InteractionAccess,
                sourceRequest.Relationship,
                candidate))
        {
            return false;
        }

        InfluenceRequest = new CharacterTargetSabotageInfluenceConsiderationRequest(
            sourceRequest.Actor,
            sourceRequest.InteractionAccess,
            sourceRequest.Relationship,
            [candidate]);
        return true;
    }
}
