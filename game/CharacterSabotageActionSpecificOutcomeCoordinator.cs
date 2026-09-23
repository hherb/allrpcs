using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one acknowledged selected sabotage result to its exact current outcome.</summary>
public partial class CharacterSabotageActionSpecificOutcomeCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator start = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator concealmentResults = null!;
    private CharacterObjectSabotageConcealmentPresentationController concealmentPresentation = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterObjectSabotageMovePresentationCoordinator moveResults = null!;
    private CharacterObjectSabotageMovePresentationController movePresentation = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterObjectSabotageSubstitutionPresentationCoordinator substitutionResults = null!;
    private CharacterObjectSabotageSubstitutionPresentationController substitutionPresentation = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterObjectSabotageContaminationPresentationCoordinator contaminationResults = null!;
    private CharacterObjectSabotageContaminationPresentationController contaminationPresentation = null!;
    private CharacterDialogueMisquotationEvidenceExposureController misquotationExposure = null!;
    private CharacterDialogueMisquotationEvidenceExposureCoordinator misquotationResults = null!;
    private CharacterDialogueMisquotationEvidenceViewController misquotationView = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator delayResults = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator influenceResults = null!;

    /// <summary>Gets the exact generic author request bound to the selected outcome.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact current concealment outcome, when that branch was selected.</summary>
    public WorldObjectConcealmentSnapshot? Concealment { get; private set; }

    /// <summary>Gets the exact current evidence paired with the concealment outcome.</summary>
    public DiscoveryEvidenceSnapshot? ConcealmentEvidence { get; private set; }

    /// <summary>Gets the exact current placement outcome, when that branch was selected.</summary>
    public WorldObjectPlacementSnapshot? Placements { get; private set; }

    /// <summary>Gets the exact current evidence paired with the placement outcome.</summary>
    public DiscoveryEvidenceSnapshot? MoveEvidence { get; private set; }

    /// <summary>Gets the exact current substitution outcome, when that branch was selected.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot? Substitutions { get; private set; }

    /// <summary>Gets the exact current evidence paired with the substitution outcome.</summary>
    public DiscoveryEvidenceSnapshot? SubstitutionEvidence { get; private set; }

    /// <summary>Gets the exact current contamination outcome, when that branch was selected.</summary>
    public WorldObjectContaminationSnapshot? Contamination { get; private set; }

    /// <summary>Gets the exact current evidence paired with the contamination outcome.</summary>
    public DiscoveryEvidenceSnapshot? ContaminationEvidence { get; private set; }

    /// <summary>Gets the exact current misquotation-exposure outcome, when selected.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot? MisquotationExposure
    {
        get;
        private set;
    }

    /// <summary>Gets the exact applied target-delay outcome, when that branch was selected.</summary>
    public CharacterTargetSabotageDelayResult? Delay { get; private set; }

    /// <summary>Gets the exact applied influence outcome, when that branch was selected.</summary>
    public CharacterTargetSabotageInfluenceResult? Influence { get; private set; }

    /// <summary>Gets the number of selected typed outcomes retained by this one-shot seam.</summary>
    public int OutcomeCount =>
        (Concealment is null ? 0 : 1)
        + (Placements is null ? 0 : 1)
        + (Substitutions is null ? 0 : 1)
        + (Contamination is null ? 0 : 1)
        + (MisquotationExposure is null ? 0 : 1)
        + (Delay is null ? 0 : 1)
        + (Influence is null ? 0 : 1);

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        start = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        concealmentResults = GetNode<CharacterObjectSabotageConcealmentPresentationCoordinator>(
            "../CharacterObjectSabotageConcealmentPresentationCoordinator");
        concealmentPresentation = GetNode<CharacterObjectSabotageConcealmentPresentationController>(
            "../CharacterObjectSabotageConcealmentPresentationController");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        moveResults = GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
            "../CharacterObjectSabotageMovePresentationCoordinator");
        movePresentation = GetNode<CharacterObjectSabotageMovePresentationController>(
            "../CharacterObjectSabotageMovePresentationController");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        substitutionResults = GetNode<CharacterObjectSabotageSubstitutionPresentationCoordinator>(
            "../CharacterObjectSabotageSubstitutionPresentationCoordinator");
        substitutionPresentation = GetNode<CharacterObjectSabotageSubstitutionPresentationController>(
            "../CharacterObjectSabotageSubstitutionPresentationController");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        contaminationResults = GetNode<CharacterObjectSabotageContaminationPresentationCoordinator>(
            "../CharacterObjectSabotageContaminationPresentationCoordinator");
        contaminationPresentation = GetNode<CharacterObjectSabotageContaminationPresentationController>(
            "../CharacterObjectSabotageContaminationPresentationController");
        misquotationExposure = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        misquotationResults = GetNode<CharacterDialogueMisquotationEvidenceExposureCoordinator>(
            "../CharacterDialogueMisquotationEvidenceExposureCoordinator");
        misquotationView = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        delayResults = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        influenceResults = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
    }

    /// <summary>Binds one selected result only while its exact post-action outcome is current.</summary>
    public bool TryBindAcknowledgedOutcome()
    {
        CharacterSabotageActionConsiderationRequest? authorRequest =
            results.AcknowledgedAuthorRequest;
        if (authorRequest is null
            || binding.AcknowledgedResult is not { SelectedActionKind: { } selectedKind }
            || binding.SelectedCandidateBinding is not { } selectedBinding
            || AuthorRequest is not null
            || OutcomeCount != 0
            || results.ResultCount != 1
            || !ReferenceEquals(binding.SelectedAuthorRequest, authorRequest)
            || !ReferenceEquals(selectedBinding.Request, authorRequest)
            || !ReferenceEquals(selectedBinding.ActionKind, selectedKind)
            || !ReferenceEquals(start.StartedAuthorRequest, authorRequest))
        {
            return false;
        }

        bool bound = TryBindConcealment(authorRequest)
            || TryBindMove(authorRequest)
            || TryBindSubstitution(authorRequest)
            || TryBindContamination(authorRequest)
            || TryBindMisquotation(authorRequest)
            || TryBindDelay(authorRequest)
            || TryBindInfluence(authorRequest);
        if (bound)
        {
            AuthorRequest = authorRequest;
        }

        return bound;
    }

    private bool TryBindConcealment(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.ConcealmentResult is not { SelectedCandidate: { } candidate } result
            || requests.ConcealmentRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(concealmentResults.AcknowledgedResult, result)
            || concealmentState.Concealment is not { } concealment
            || concealmentState.Evidence is not { } evidence
            || !ReferenceEquals(concealmentResults.AcknowledgedConcealment, concealment)
            || !ReferenceEquals(concealmentResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(concealmentPresentation.LastPresentedConcealment, concealment)
            || ReferenceEquals(concealment, request.Concealment)
            || ReferenceEquals(evidence, request.Evidence))
        {
            return false;
        }

        Concealment = concealment;
        ConcealmentEvidence = evidence;
        return true;
    }

    private bool TryBindMove(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.MoveResult is not { SelectedCandidate: { } candidate } result
            || requests.MoveRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(moveResults.AcknowledgedResult, result)
            || moveState.Placements is not { } placements
            || moveState.Evidence is not { } evidence
            || !ReferenceEquals(moveResults.AcknowledgedPlacements, placements)
            || !ReferenceEquals(moveResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(movePresentation.LastPresentedPlacements, placements)
            || ReferenceEquals(placements, request.Placements)
            || ReferenceEquals(evidence, request.Evidence))
        {
            return false;
        }

        Placements = placements;
        MoveEvidence = evidence;
        return true;
    }

    private bool TryBindSubstitution(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.SubstitutionResult is not { SelectedCandidate: { } candidate } result
            || requests.SubstitutionRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(substitutionResults.AcknowledgedResult, result)
            || substitutionState.Substitutions is not { } substitutions
            || substitutionState.Evidence is not { } evidence
            || !ReferenceEquals(substitutionResults.AcknowledgedSubstitutions, substitutions)
            || !ReferenceEquals(substitutionResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(
                substitutionPresentation.LastPresentedSubstitutions,
                substitutions)
            || ReferenceEquals(substitutions, request.Substitutions)
            || ReferenceEquals(evidence, request.Evidence))
        {
            return false;
        }

        Substitutions = substitutions;
        SubstitutionEvidence = evidence;
        return true;
    }

    private bool TryBindContamination(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.ContaminationResult is not { SelectedCandidate: { } candidate } result
            || requests.ContaminationRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(contaminationResults.AcknowledgedResult, result)
            || contaminationState.Contamination is not { } contamination
            || contaminationState.Evidence is not { } evidence
            || !ReferenceEquals(contaminationResults.AcknowledgedContamination, contamination)
            || !ReferenceEquals(contaminationResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(
                contaminationPresentation.LastPresentedContamination,
                contamination)
            || ReferenceEquals(contamination, request.Contamination)
            || ReferenceEquals(evidence, request.Evidence))
        {
            return false;
        }

        Contamination = contamination;
        ContaminationEvidence = evidence;
        return true;
    }

    private bool TryBindMisquotation(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.MisquotationResult is not { SelectedEvidence: { } candidate } result
            || requests.MisquotationRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(misquotationResults.AcknowledgedResult, result)
            || misquotationExposure.Exposure is not { } exposure
            || !ReferenceEquals(misquotationResults.AcknowledgedExposure, exposure)
            || !ReferenceEquals(misquotationView.SourceExposure, exposure))
        {
            return false;
        }

        MisquotationExposure = exposure;
        return true;
    }

    private bool TryBindDelay(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.DelayResult is not { SelectedCandidate: { } candidate } result
            || requests.DelayRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || delayState.LatestAppliedResult is not { } outcome
            || !ReferenceEquals(delayResults.AcknowledgedResult, result)
            || !ReferenceEquals(delayResults.AcknowledgedAppliedResult, outcome)
            || !ReferenceEquals(outcome.Candidate, candidate)
            || !ReferenceEquals(delayState.EventLog, outcome.EventLog)
            || !ReferenceEquals(delayState.WorldTime, outcome.WorldTime))
        {
            return false;
        }

        Delay = outcome;
        return true;
    }

    private bool TryBindInfluence(CharacterSabotageActionConsiderationRequest authorRequest)
    {
        if (results.InfluenceResult is not { SelectedCandidate: { } candidate } result
            || requests.InfluenceRequest is not { } request
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> selected
            || !ExactSelection(authorRequest, selected, request.Candidates, candidate)
            || !ReferenceEquals(result.Request, request)
            || influenceState.LatestAppliedResult is not { } outcome
            || !ReferenceEquals(influenceResults.AcknowledgedResult, result)
            || !ReferenceEquals(influenceResults.AcknowledgedAppliedResult, outcome)
            || !ReferenceEquals(outcome.Candidate, candidate)
            || !ReferenceEquals(influenceState.Relationship, outcome.Relationship))
        {
            return false;
        }

        Influence = outcome;
        return true;
    }

    private static bool ExactSelection<TCandidate>(
        CharacterSabotageActionConsiderationRequest authorRequest,
        CharacterSabotageActionCandidateBinding<TCandidate> selected,
        IReadOnlyList<TCandidate> candidates,
        TCandidate candidate)
        where TCandidate : class
    {
        return ReferenceEquals(selected.Request, authorRequest)
            && ReferenceEquals(selected.Candidate, candidate)
            && candidates.Count == 1
            && ReferenceEquals(candidates[0], candidate);
    }
}
