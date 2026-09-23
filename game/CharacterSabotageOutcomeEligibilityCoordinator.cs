using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Publishes one retained exact sabotage outcome for its existing recovery route.</summary>
public partial class CharacterSabotageOutcomeEligibilityCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator concealmentResults = null!;
    private TemporaryCharacterObjectSabotageMoveSource moveSource = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterObjectSabotageMovePresentationCoordinator moveResults = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterObjectSabotageSubstitutionPresentationCoordinator substitutionResults = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterObjectSabotageContaminationPresentationCoordinator contaminationResults = null!;
    private CharacterDialogueMisquotationEvidenceExposureController misquotationState = null!;
    private CharacterDialogueMisquotationEvidenceExposureCoordinator misquotationResults = null!;
    private CharacterDialogueMisquotationEvidenceViewController misquotationView = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator delayResults = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController delayRecovery = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator influenceResults = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateController influenceRecovery = null!;

    /// <summary>Gets the exact generic author request whose outcome became eligible.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact evidence view that admitted an object-recovery branch.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <summary>Gets the exact concealment candidate eligible for recovery.</summary>
    public CharacterObjectSabotageEvidence? ConcealmentCandidate { get; private set; }

    /// <summary>Gets the exact move-recovery candidate eligible for recovery.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? MoveCandidate { get; private set; }

    /// <summary>Gets the exact substitution candidate eligible for recovery.</summary>
    public CharacterObjectSabotageSubstitutionCandidate? SubstitutionCandidate { get; private set; }

    /// <summary>Gets the exact contamination candidate eligible for recovery.</summary>
    public CharacterObjectSabotageEvidence? ContaminationCandidate { get; private set; }

    /// <summary>Gets the exact exposed misquotation-evidence outcome eligible for its view route.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot? MisquotationExposure
    {
        get;
        private set;
    }

    /// <summary>Gets the exact current view paired with the misquotation-evidence outcome.</summary>
    public CharacterDialogueMisquotationEvidenceViewSnapshot? MisquotationView { get; private set; }

    /// <summary>Gets the existing exact delay-recovery candidate, when eligible.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? DelayCandidate { get; private set; }

    /// <summary>Gets the existing exact influence-recovery candidate, when eligible.</summary>
    public CharacterTargetSabotageInfluenceRecoveryCandidate? InfluenceCandidate { get; private set; }

    /// <summary>Gets the number of typed eligibility routes published by this one-shot seam.</summary>
    public int EligibilityCount =>
        (ConcealmentCandidate is null ? 0 : 1)
        + (MoveCandidate is null ? 0 : 1)
        + (SubstitutionCandidate is null ? 0 : 1)
        + (ContaminationCandidate is null ? 0 : 1)
        + (MisquotationExposure is null ? 0 : 1)
        + (DelayCandidate is null ? 0 : 1)
        + (InfluenceCandidate is null ? 0 : 1);

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        outcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        concealmentResults = GetNode<CharacterObjectSabotageConcealmentPresentationCoordinator>(
            "../CharacterObjectSabotageConcealmentPresentationCoordinator");
        moveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        moveResults = GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
            "../CharacterObjectSabotageMovePresentationCoordinator");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        substitutionResults = GetNode<CharacterObjectSabotageSubstitutionPresentationCoordinator>(
            "../CharacterObjectSabotageSubstitutionPresentationCoordinator");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        contaminationResults = GetNode<CharacterObjectSabotageContaminationPresentationCoordinator>(
            "../CharacterObjectSabotageContaminationPresentationCoordinator");
        misquotationState = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        misquotationResults = GetNode<CharacterDialogueMisquotationEvidenceExposureCoordinator>(
            "../CharacterDialogueMisquotationEvidenceExposureCoordinator");
        misquotationView = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        delayResults = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        delayRecovery = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        influenceResults = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
        influenceRecovery = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
    }

    /// <summary>Publishes one exact currently eligible branch without starting its route.</summary>
    public bool TryPublish(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (expectedAuthorRequest is null
            || AuthorRequest is not null
            || EligibilityCount != 0
            || outcomes.OutcomeCount != 1
            || results.ResultCount != 1
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || binding.AcknowledgedResult?.SelectedActionKind is not { } selectedKind
            || binding.SelectedCandidateBinding is not { } selectedBinding
            || !ReferenceEquals(selectedBinding.Request, expectedAuthorRequest)
            || !ReferenceEquals(selectedBinding.ActionKind, selectedKind))
        {
            return false;
        }

        bool published = TryPublishConcealment(evidenceView)
            || TryPublishMove(evidenceView)
            || TryPublishSubstitution(evidenceView)
            || TryPublishContamination(evidenceView)
            || TryPublishMisquotation(evidenceView)
            || TryPublishDelay(evidenceView)
            || TryPublishInfluence(evidenceView);
        if (published)
        {
            AuthorRequest = expectedAuthorRequest;
        }

        return published;
    }

    private bool TryPublishConcealment(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Conceal)
            || results.ConcealmentResult is not { SelectedCandidate: { } candidate } result
            || requests.ConcealmentRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(concealmentResults.AcknowledgedResult, result)
            || outcomes.Concealment is not { } concealment
            || outcomes.ConcealmentEvidence is not { } evidence
            || !ReferenceEquals(concealmentState.Concealment, concealment)
            || !ReferenceEquals(concealmentState.Evidence, evidence)
            || !ReferenceEquals(concealmentResults.AcknowledgedConcealment, concealment)
            || !ReferenceEquals(concealmentResults.AcknowledgedEvidence, evidence)
            || !CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                concealment,
                evidence,
                evidenceView,
                candidate))
        {
            return false;
        }

        EvidenceView = evidenceView;
        ConcealmentCandidate = candidate;
        return true;
    }

    private bool TryPublishMove(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Move)
            || results.MoveResult is not { SelectedCandidate: { } candidate } result
            || requests.MoveRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(moveResults.AcknowledgedResult, result)
            || outcomes.Placements is not { } placements
            || outcomes.MoveEvidence is not { } evidence
            || !ReferenceEquals(moveState.Placements, placements)
            || !ReferenceEquals(moveState.Evidence, evidence)
            || !ReferenceEquals(moveResults.AcknowledgedPlacements, placements)
            || !ReferenceEquals(moveResults.AcknowledgedEvidence, evidence)
            || moveSource.RecoveryCandidate is not { } recovery
            || !ReferenceEquals(recovery.Candidate, candidate)
            || !CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                placements,
                evidence,
                evidenceView,
                recovery))
        {
            return false;
        }

        EvidenceView = evidenceView;
        MoveCandidate = recovery;
        return true;
    }

    private bool TryPublishSubstitution(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Substitute)
            || results.SubstitutionResult is not { SelectedCandidate: { } candidate } result
            || requests.SubstitutionRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(substitutionResults.AcknowledgedResult, result)
            || outcomes.Substitutions is not { } substitutions
            || outcomes.SubstitutionEvidence is not { } evidence
            || !ReferenceEquals(substitutionState.Substitutions, substitutions)
            || !ReferenceEquals(substitutionState.Evidence, evidence)
            || !ReferenceEquals(substitutionResults.AcknowledgedSubstitutions, substitutions)
            || !ReferenceEquals(substitutionResults.AcknowledgedEvidence, evidence)
            || !CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                substitutions,
                evidence,
                evidenceView,
                candidate))
        {
            return false;
        }

        EvidenceView = evidenceView;
        SubstitutionCandidate = candidate;
        return true;
    }

    private bool TryPublishContamination(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Contaminate)
            || results.ContaminationResult is not { SelectedCandidate: { } candidate } result
            || requests.ContaminationRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(contaminationResults.AcknowledgedResult, result)
            || outcomes.Contamination is not { } contamination
            || outcomes.ContaminationEvidence is not { } evidence
            || !ReferenceEquals(contaminationState.Contamination, contamination)
            || !ReferenceEquals(contaminationState.Evidence, evidence)
            || !ReferenceEquals(contaminationResults.AcknowledgedContamination, contamination)
            || !ReferenceEquals(contaminationResults.AcknowledgedEvidence, evidence)
            || !CharacterObjectSabotageContaminationRecoveryPolicy.CanRecover(
                contamination,
                evidence,
                evidenceView,
                candidate))
        {
            return false;
        }

        EvidenceView = evidenceView;
        ContaminationCandidate = candidate;
        return true;
    }

    private bool TryPublishMisquotation(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is not null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Misquote)
            || results.MisquotationResult is not { SelectedEvidence: { } candidate } result
            || requests.MisquotationRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(misquotationResults.AcknowledgedResult, result)
            || outcomes.MisquotationExposure is not { } exposure
            || !ReferenceEquals(misquotationState.Exposure, exposure)
            || !ReferenceEquals(misquotationResults.AcknowledgedExposure, exposure)
            || !ReferenceEquals(misquotationView.SourceExposure, exposure)
            || misquotationView.View is not { } view
            || !view.Entries.Any(entry => ReferenceEquals(entry, candidate)))
        {
            return false;
        }

        MisquotationExposure = exposure;
        MisquotationView = view;
        return true;
    }

    private bool TryPublishDelay(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is not null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Delay)
            || results.DelayResult is not { SelectedCandidate: { } candidate } result
            || requests.DelayRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(delayResults.AcknowledgedResult, result)
            || outcomes.Delay is not { } outcome
            || !ReferenceEquals(delayState.LatestAppliedResult, outcome)
            || !ReferenceEquals(delayState.EventLog, outcome.EventLog)
            || !ReferenceEquals(delayState.WorldTime, outcome.WorldTime)
            || !ReferenceEquals(delayResults.AcknowledgedAppliedResult, outcome)
            || delayRecovery.Candidate is not { } recovery
            || !ReferenceEquals(recovery.DelayResult, outcome))
        {
            return false;
        }

        DelayCandidate = recovery;
        return true;
    }

    private bool TryPublishInfluence(CharacterEvidenceViewSnapshot? evidenceView)
    {
        if (evidenceView is not null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Influence)
            || results.InfluenceResult is not { SelectedCandidate: { } candidate } result
            || requests.InfluenceRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> selected
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(influenceResults.AcknowledgedResult, result)
            || outcomes.Influence is not { } outcome
            || !ReferenceEquals(influenceState.LatestAppliedResult, outcome)
            || !ReferenceEquals(influenceState.Relationship, outcome.Relationship)
            || !ReferenceEquals(influenceResults.AcknowledgedAppliedResult, outcome)
            || influenceRecovery.Candidate is not { } recovery
            || !ReferenceEquals(recovery.InfluenceResult, outcome)
            || !CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                influenceState.Relationship,
                recovery))
        {
            return false;
        }

        InfluenceCandidate = recovery;
        return true;
    }
}
