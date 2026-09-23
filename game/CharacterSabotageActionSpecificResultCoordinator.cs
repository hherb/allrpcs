using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes and acknowledges one exact typed sabotage-action result.</summary>
public partial class CharacterSabotageActionSpecificResultCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator start = null!;
    private CharacterObjectSabotageConcealmentTaskController concealmentTask = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator concealmentResults = null!;
    private CharacterObjectSabotageMoveTaskController moveTask = null!;
    private CharacterObjectSabotageMovePresentationCoordinator moveResults = null!;
    private CharacterObjectSabotageSubstitutionTaskController substitutionTask = null!;
    private CharacterObjectSabotageSubstitutionPresentationCoordinator substitutionResults = null!;
    private CharacterObjectSabotageContaminationTaskController contaminationTask = null!;
    private CharacterObjectSabotageContaminationPresentationCoordinator contaminationResults = null!;
    private CharacterDialogueMisquotationEvidenceTaskController misquotationTask = null!;
    private CharacterDialogueMisquotationEvidenceExposureCoordinator misquotationResults = null!;
    private CharacterTargetSabotageDelayTaskController delayTask = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator delayResults = null!;
    private CharacterTargetSabotageInfluenceTaskController influenceTask = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator influenceResults = null!;

    /// <summary>Gets the exact observed concealment result, if that branch completed.</summary>
    public CharacterObjectSabotageConcealmentConsiderationResult? ConcealmentResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact observed move result, if that branch completed.</summary>
    public CharacterObjectSabotageMoveConsiderationResult? MoveResult { get; private set; }

    /// <summary>Gets the exact observed substitution result, if that branch completed.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationResult? SubstitutionResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact observed contamination result, if that branch completed.</summary>
    public CharacterObjectSabotageContaminationConsiderationResult? ContaminationResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact observed misquotation result, if that branch completed.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationResult? MisquotationResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact observed delay result, if that branch completed.</summary>
    public CharacterTargetSabotageDelayConsiderationResult? DelayResult { get; private set; }

    /// <summary>Gets the exact observed influence result, if that branch completed.</summary>
    public CharacterTargetSabotageInfluenceConsiderationResult? InfluenceResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact generic author request for the acknowledged typed result.</summary>
    public CharacterSabotageActionConsiderationRequest? AcknowledgedAuthorRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the number of exact typed results retained by this one-shot seam.</summary>
    public int ResultCount =>
        (ConcealmentResult is null ? 0 : 1)
        + (MoveResult is null ? 0 : 1)
        + (SubstitutionResult is null ? 0 : 1)
        + (ContaminationResult is null ? 0 : 1)
        + (MisquotationResult is null ? 0 : 1)
        + (DelayResult is null ? 0 : 1)
        + (InfluenceResult is null ? 0 : 1);

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        start = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        concealmentTask = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        concealmentResults = GetNode<CharacterObjectSabotageConcealmentPresentationCoordinator>(
            "../CharacterObjectSabotageConcealmentPresentationCoordinator");
        moveTask = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
        moveResults = GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
            "../CharacterObjectSabotageMovePresentationCoordinator");
        substitutionTask = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
        substitutionResults = GetNode<CharacterObjectSabotageSubstitutionPresentationCoordinator>(
            "../CharacterObjectSabotageSubstitutionPresentationCoordinator");
        contaminationTask = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");
        contaminationResults = GetNode<CharacterObjectSabotageContaminationPresentationCoordinator>(
            "../CharacterObjectSabotageContaminationPresentationCoordinator");
        misquotationTask = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        misquotationResults = GetNode<CharacterDialogueMisquotationEvidenceExposureCoordinator>(
            "../CharacterDialogueMisquotationEvidenceExposureCoordinator");
        delayTask = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        delayResults = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        influenceTask = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        influenceResults = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
    }

    /// <summary>Observes the exact terminal result for the one started typed request.</summary>
    public bool TryObserveCompletedResult()
    {
        CharacterSabotageActionConsiderationRequest? authorRequest =
            start.StartedAuthorRequest;
        if (authorRequest is null
            || ResultCount != 0
            || AcknowledgedAuthorRequest is not null
            || !ReferenceEquals(binding.SelectedAuthorRequest, authorRequest))
        {
            return false;
        }

        return TryObserveConcealment()
            || TryObserveMove()
            || TryObserveSubstitution()
            || TryObserveContamination()
            || TryObserveMisquotation()
            || TryObserveDelay()
            || TryObserveInfluence();
    }

    /// <summary>Acknowledges only the same exact observed result through its existing owner.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterSabotageActionConsiderationRequest? authorRequest =
            start.StartedAuthorRequest;
        if (authorRequest is null
            || ResultCount != 1
            || AcknowledgedAuthorRequest is not null
            || !ReferenceEquals(binding.SelectedAuthorRequest, authorRequest))
        {
            return false;
        }

        bool acknowledged = ConcealmentResult is { } concealment
            && ReferenceEquals(concealmentTask.Result, concealment)
            && concealmentResults.TryAcknowledgeCompletedResult()
            || MoveResult is { } move
            && ReferenceEquals(moveTask.Result, move)
            && moveResults.TryAcknowledgeCompletedResult()
            || SubstitutionResult is { } substitution
            && ReferenceEquals(substitutionTask.Result, substitution)
            && substitutionResults.TryAcknowledgeCompletedResult()
            || ContaminationResult is { } contamination
            && ReferenceEquals(contaminationTask.Result, contamination)
            && contaminationResults.TryAcknowledgeCompletedResult()
            || MisquotationResult is { } misquotation
            && ReferenceEquals(misquotationTask.Result, misquotation)
            && misquotationResults.TryAcknowledgeCompletedResult()
            || DelayResult is { } delay
            && ReferenceEquals(delayTask.Result, delay)
            && delayResults.TryAcknowledgeCompletedResult()
            || InfluenceResult is { } influence
            && ReferenceEquals(influenceTask.Result, influence)
            && influenceResults.TryAcknowledgeCompletedResult();
        if (acknowledged)
        {
            AcknowledgedAuthorRequest = authorRequest;
        }

        return acknowledged;
    }

    private bool TryObserveConcealment()
    {
        CharacterObjectSabotageConcealmentConsiderationResult? result =
            concealmentTask.Result;
        if (requests.ConcealmentRequest is not { } request
            || concealmentTask.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        ConcealmentResult = result;
        return true;
    }

    private bool TryObserveMove()
    {
        CharacterObjectSabotageMoveConsiderationResult? result = moveTask.Result;
        if (requests.MoveRequest is not { } request
            || moveTask.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageMoveCandidate> selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        MoveResult = result;
        return true;
    }

    private bool TryObserveSubstitution()
    {
        CharacterObjectSabotageSubstitutionConsiderationResult? result =
            substitutionTask.Result;
        if (requests.SubstitutionRequest is not { } request
            || substitutionTask.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        SubstitutionResult = result;
        return true;
    }

    private bool TryObserveContamination()
    {
        CharacterObjectSabotageContaminationConsiderationResult? result =
            contaminationTask.Result;
        if (requests.ContaminationRequest is not { } request
            || contaminationTask.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        ContaminationResult = result;
        return true;
    }

    private bool TryObserveMisquotation()
    {
        CharacterDialogueMisquotationEvidenceConsiderationResult? result =
            misquotationTask.Result;
        if (requests.MisquotationRequest is not { } request
            || misquotationTask.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> selected
            || result.SelectedEvidence is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        MisquotationResult = result;
        return true;
    }

    private bool TryObserveDelay()
    {
        CharacterTargetSabotageDelayConsiderationResult? result = delayTask.Result;
        if (requests.DelayRequest is not { } request
            || delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageDelayCandidate> selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        DelayResult = result;
        return true;
    }

    private bool TryObserveInfluence()
    {
        CharacterTargetSabotageInfluenceConsiderationResult? result = influenceTask.Result;
        if (requests.InfluenceRequest is not { } request
            || influenceTask.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || result is null
            || !ReferenceEquals(result.Request, request)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> selected
            || result.SelectedCandidate is { } candidate
                && !ReferenceEquals(candidate, selected.Candidate))
        {
            return false;
        }

        InfluenceResult = result;
        return true;
    }
}
