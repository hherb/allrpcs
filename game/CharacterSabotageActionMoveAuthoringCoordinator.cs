using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Guards one existing Move author invocation with exact cycle admission.</summary>
public partial class CharacterSabotageActionMoveAuthoringCoordinator : Node
{
    private CharacterSabotageActionAuthoringAdmissionCoordinator admissions = null!;
    private CharacterSabotageCycleBoundaryCoordinator cycleBoundary = null!;
    private CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator authorization = null!;
    private CharacterSabotageTurnAdmissionCoordinator turnAdmission = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact admission that guarded the successful author invocation.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <summary>Gets the exact request produced by the guarded existing author.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admissions = GetNode<CharacterSabotageActionAuthoringAdmissionCoordinator>(
            "../CharacterSabotageActionAuthoringAdmissionCoordinator");
        cycleBoundary = GetNode<CharacterSabotageCycleBoundaryCoordinator>(
            "../CharacterSabotageCycleBoundaryCoordinator");
        authorization =
            GetNode<CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator");
        turnAdmission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../ScheduledCharacterSabotageActionMoveRequestController");
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Invokes the existing Move author once for the exact current admission.</summary>
    public bool TryAuthor(CharacterSabotageActionAuthoringAdmission? expectedAdmission)
    {
        CharacterSabotageActionAuthoringAdmission? currentAdmission = admissions.Admission;
        CharacterSabotageCycleBoundary? currentBoundary = cycleBoundary.Boundary;
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? currentAuthorization =
            authorization.Authorization;
        if (Admission is not null
            || AuthorRequest is not null
            || currentAdmission is null
            || currentBoundary is null
            || !ReferenceEquals(currentAdmission, expectedAdmission)
            || !ReferenceEquals(currentAdmission.Boundary, currentBoundary)
            || !ReferenceEquals(currentAdmission.Actor, source.Character)
            || !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                currentBoundary,
                currentBoundary,
                currentAuthorization,
                turnAdmission.Admission,
                turns.Activity)
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence
            || !moveAuthor.TryCreate(
                currentAdmission.Actor,
                source.PhysicalAccess,
                source.WorldObjects,
                placements,
                evidence,
                source.Destination,
                source.FollowUpCandidate))
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest request = moveAuthor.Request
            ?? throw new InvalidOperationException("Successful Move authoring must retain a request.");
        if (moveAuthor.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    binding
            || !ReferenceEquals(binding.Request, request)
            || !ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(binding.Candidate, source.FollowUpCandidate))
        {
            throw new InvalidOperationException(
                "Successful Move authoring must retain its exact typed candidate binding.");
        }

        Admission = currentAdmission;
        AuthorRequest = request;
        return true;
    }
}
