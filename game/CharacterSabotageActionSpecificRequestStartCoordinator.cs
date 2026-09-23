using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts only the exact typed request composed from one acknowledged action.</summary>
public partial class CharacterSabotageActionSpecificRequestStartCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource concealmentModels = null!;
    private CharacterObjectSabotageConcealmentTaskController concealmentTask = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource moveModels = null!;
    private CharacterObjectSabotageMoveTaskController moveTask = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource substitutionModels = null!;
    private CharacterObjectSabotageSubstitutionTaskController substitutionTask = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource contaminationModels = null!;
    private CharacterObjectSabotageContaminationTaskController contaminationTask = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource misquotationModels = null!;
    private CharacterDialogueMisquotationEvidenceTaskController misquotationTask = null!;
    private TemporaryCharacterTargetSabotageDelayModelSource delayModels = null!;
    private CharacterTargetSabotageDelayTaskController delayTask = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private TemporaryCharacterTargetSabotageInfluenceModelSource influenceModels = null!;
    private CharacterTargetSabotageInfluenceTaskController influenceTask = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;

    /// <summary>Gets the exact generic author request whose typed request was started.</summary>
    public CharacterSabotageActionConsiderationRequest? StartedAuthorRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        concealmentModels = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        concealmentTask = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        moveModels = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../TemporaryCharacterObjectSabotageMoveModelSource");
        moveTask = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        substitutionModels = GetNode<TemporaryCharacterObjectSabotageSubstitutionModelSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionModelSource");
        substitutionTask = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        contaminationModels = GetNode<TemporaryCharacterObjectSabotageContaminationModelSource>(
            "../TemporaryCharacterObjectSabotageContaminationModelSource");
        contaminationTask = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        misquotationModels = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        misquotationTask = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        delayModels = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        delayTask = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        influenceModels = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        influenceTask = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
    }

    /// <summary>Starts the one exact current typed request at most once.</summary>
    public bool TryStart()
    {
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding? candidateBinding =
            binding.SelectedCandidateBinding;
        CharacterSabotageActionKind? selectedKind =
            binding.AcknowledgedResult?.SelectedActionKind;
        if (authorRequest is null
            || candidateBinding is null
            || selectedKind is null
            || StartedAuthorRequest is not null
            || requests.RequestCount != 1
            || !ReferenceEquals(candidateBinding.Request, authorRequest)
            || !ReferenceEquals(candidateBinding.ActionKind, selectedKind))
        {
            return false;
        }

        bool started = false;
        if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Conceal)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    concealment
            && requests.ConcealmentRequest is { } concealmentRequest)
        {
            started = TryStartConcealment(concealmentRequest, concealment.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Move)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    move
            && requests.MoveRequest is { } moveRequest)
        {
            started = TryStartMove(moveRequest, move.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Substitute)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> substitution
            && requests.SubstitutionRequest is { } substitutionRequest)
        {
            started = TryStartSubstitution(substitutionRequest, substitution.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Contaminate)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    contamination
            && requests.ContaminationRequest is { } contaminationRequest)
        {
            started = TryStartContamination(contaminationRequest, contamination.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Misquote)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> misquotation
            && requests.MisquotationRequest is { } misquotationRequest)
        {
            started = TryStartMisquotation(misquotationRequest, misquotation.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Delay)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    delay
            && requests.DelayRequest is { } delayRequest)
        {
            started = TryStartDelay(delayRequest, delay.Candidate);
        }
        else if (ReferenceEquals(selectedKind, CharacterSabotageActionKind.Influence)
            && candidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> influence
            && requests.InfluenceRequest is { } influenceRequest)
        {
            started = TryStartInfluence(influenceRequest, influence.Candidate);
        }

        if (started)
        {
            StartedAuthorRequest = authorRequest;
        }

        return started;
    }

    private bool TryStartConcealment(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CharacterObjectSabotageEvidence candidate)
    {
        if (concealmentTask.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(concealmentState.Concealment, request.Concealment)
            || !ReferenceEquals(concealmentState.Evidence, request.Evidence)
            || !CharacterObjectSabotageConcealmentPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Concealment,
                request.Evidence,
                candidate))
        {
            return false;
        }

        ICharacterObjectSabotageConcealmentDecisionModel model = concealmentModels.TakeModel();
        return concealmentTask.TryStart(model, request, concealmentState);
    }

    private bool TryStartMove(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveCandidate candidate)
    {
        if (moveTask.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(moveState.Placements, request.Placements)
            || !ReferenceEquals(moveState.Evidence, request.Evidence)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Placements,
                request.Evidence,
                candidate))
        {
            return false;
        }

        ICharacterObjectSabotageMoveDecisionModel model = moveModels.TakeModel();
        return moveTask.TryStart(model, request, moveState);
    }

    private bool TryStartSubstitution(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        if (substitutionTask.Status != CharacterObjectSabotageSubstitutionTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(substitutionState.Substitutions, request.Substitutions)
            || !ReferenceEquals(substitutionState.Evidence, request.Evidence)
            || !CharacterObjectSabotageSubstitutionPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Substitutions,
                request.Evidence,
                candidate))
        {
            return false;
        }

        ICharacterObjectSabotageSubstitutionDecisionModel model = substitutionModels.TakeModel();
        return substitutionTask.TryStart(model, request, substitutionState);
    }

    private bool TryStartContamination(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        CharacterObjectSabotageEvidence candidate)
    {
        if (contaminationTask.Status != CharacterObjectSabotageContaminationTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(contaminationState.Contamination, request.Contamination)
            || !ReferenceEquals(contaminationState.Evidence, request.Evidence)
            || !CharacterObjectSabotageContaminationPolicy.CanApply(
                request.Character,
                request.PhysicalAccess,
                request.WorldObjects,
                request.Contamination,
                request.Evidence,
                candidate))
        {
            return false;
        }

        ICharacterObjectSabotageContaminationDecisionModel model =
            contaminationModels.TakeModel();
        return contaminationTask.TryStart(model, request, contaminationState);
    }

    private bool TryStartMisquotation(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        CharacterDialogueMisquotationEvidence candidate)
    {
        if (misquotationTask.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !CharacterDialogueMisquotationEvidencePolicy.CanObserve(request.Actor, candidate))
        {
            return false;
        }

        ICharacterDialogueMisquotationEvidenceDecisionModel model =
            misquotationModels.TakeModel();
        return misquotationTask.TryStart(model, request);
    }

    private bool TryStartDelay(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CharacterTargetSabotageDelayCandidate candidate)
    {
        if (delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(delayState.EventLog, request.EventLog)
            || !ReferenceEquals(delayState.WorldTime, request.WorldTime)
            || !CharacterTargetSabotageDelayPolicy.CanApply(
                request.Actor,
                request.InteractionAccess,
                request.WorldEvents,
                request.EventLog,
                request.WorldTime,
                candidate))
        {
            return false;
        }

        ICharacterTargetSabotageDelayDecisionModel model = delayModels.TakeModel();
        return delayTask.TryStart(model, request);
    }

    private bool TryStartInfluence(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        CharacterTargetSabotageInfluenceCandidate candidate)
    {
        if (influenceTask.Status != CharacterTargetSabotageInfluenceTaskStatus.Idle
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(influenceState.Relationship, request.Relationship)
            || !CharacterTargetSabotageInfluencePolicy.CanApply(
                request.Actor,
                request.InteractionAccess,
                request.Relationship,
                candidate))
        {
            return false;
        }

        ICharacterTargetSabotageInfluenceDecisionModel model = influenceModels.TakeModel();
        return influenceTask.TryStart(model, request);
    }
}
