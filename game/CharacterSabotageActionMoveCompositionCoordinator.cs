using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one exact admission-guarded Move author output without starting it.</summary>
public partial class CharacterSabotageActionMoveCompositionCoordinator : Node
{
    private CharacterSabotageActionMoveAuthoringCoordinator authoring = null!;
    private CharacterSabotageActionAuthoringAdmissionCoordinator admissions = null!;
    private CharacterSabotageCycleBoundaryCoordinator cycleBoundary = null!;
    private CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator authorization = null!;
    private CharacterSabotageTurnAdmissionCoordinator turnAdmission = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;

    /// <summary>Gets the exact admission retained by successful composition.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <summary>Gets the exact guarded Move author output retained by successful composition.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the fresh generic Move request produced by successful composition.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<CharacterSabotageActionMoveAuthoringCoordinator>(
            "../CharacterSabotageActionMoveAuthoringCoordinator");
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
    }

    /// <summary>Composes once only from the exact still-current guarded Move author output.</summary>
    public bool TryCompose(CharacterSabotageActionConsiderationRequest? expectedAuthorRequest)
    {
        if (Admission is not null
            || AuthorRequest is not null
            || Request is not null
            || !HasCurrentAuthorGraph(expectedAuthorRequest))
        {
            return false;
        }

        CharacterSabotageActionAuthoringAdmission currentAdmission = admissions.Admission!;
        CharacterSabotageActionConsiderationRequest currentAuthorRequest =
            authoring.AuthorRequest!;
        CharacterSabotageActionConsiderationRequest request = new(
            currentAdmission.Actor,
            [CharacterSabotageActionKind.Move]);
        Admission = currentAdmission;
        AuthorRequest = currentAuthorRequest;
        Request = request;
        return true;
    }

    /// <summary>Checks the retained fresh request against its complete exact current graph.</summary>
    internal bool IsCurrentRequest(CharacterSabotageActionConsiderationRequest? expectedRequest) =>
        Admission is not null
        && AuthorRequest is not null
        && Request is not null
        && ReferenceEquals(Request, expectedRequest)
        && ReferenceEquals(Admission, admissions.Admission)
        && ReferenceEquals(AuthorRequest, authoring.AuthorRequest)
        && HasCurrentAuthorGraph(AuthorRequest)
        && ReferenceEquals(Request.Actor, Admission.Actor)
        && Request.ActionKinds.Count == 1
        && ReferenceEquals(Request.ActionKinds[0], CharacterSabotageActionKind.Move);

    private bool HasCurrentAuthorGraph(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest)
    {
        CharacterSabotageActionAuthoringAdmission? currentAdmission = admissions.Admission;
        CharacterSabotageCycleBoundary? currentBoundary = cycleBoundary.Boundary;
        CharacterSabotageActionConsiderationRequest? currentAuthorRequest =
            authoring.AuthorRequest;
        return currentAdmission is not null
            && currentBoundary is not null
            && currentAuthorRequest is not null
            && ReferenceEquals(authoring.Admission, currentAdmission)
            && ReferenceEquals(currentAdmission.Boundary, currentBoundary)
            && ReferenceEquals(currentAdmission.Actor, source.Character)
            && CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                currentBoundary,
                currentBoundary,
                authorization.Authorization,
                turnAdmission.Admission,
                turns.Activity)
            && ReferenceEquals(currentAuthorRequest, expectedAuthorRequest)
            && ReferenceEquals(moveAuthor.Request, currentAuthorRequest)
            && HasExactMoveBinding(currentAuthorRequest)
            && ReferenceEquals(currentAuthorRequest.Actor, currentAdmission.Actor)
            && currentAuthorRequest.ActionKinds.Count == 1
            && ReferenceEquals(
                currentAuthorRequest.ActionKinds[0],
                CharacterSabotageActionKind.Move);
    }

    private bool HasExactMoveBinding(CharacterSabotageActionConsiderationRequest authorRequest) =>
        moveAuthor.CandidateBinding
            is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate> binding
        && ReferenceEquals(binding.Request, authorRequest)
        && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(binding.Candidate, source.FollowUpCandidate);
}
