using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Guards one fresh Move author with the exact current follow-up-cycle admission.</summary>
public partial class CharacterSabotageActionMoveCycleAuthoringCoordinator : Node
{
    private CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator admissions = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private CharacterSabotageActionMoveRecoveryCoordinator recovery = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact follow-up-cycle admission that guarded authoring.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <summary>Gets the exact request produced by the fresh guarded author.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admissions = GetNode<CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator>(
            "../CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../CycledCharacterSabotageActionMoveRequestController");
        recovery = GetNode<CharacterSabotageActionMoveRecoveryCoordinator>(
            "../CharacterSabotageActionMoveRecoveryCoordinator");
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Invokes the fresh Move author once for the exact current admission.</summary>
    public bool TryAuthor(CharacterSabotageActionAuthoringAdmission? expectedAdmission)
    {
        CharacterSabotageActionAuthoringAdmission? currentAdmission = admissions.Admission;
        if (Admission is not null
            || AuthorRequest is not null
            || currentAdmission is null
            || !ReferenceEquals(currentAdmission, expectedAdmission)
            || !admissions.IsCurrentAdmission(currentAdmission)
            || !ReferenceEquals(currentAdmission.Actor, source.Character)
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence
            || !recovery.IsCurrentRecovery(placements, evidence)
            || !moveAuthor.TryCreate(
                currentAdmission.Actor,
                source.PhysicalAccess,
                source.WorldObjects,
                placements,
                evidence,
                source.Destination,
                source.CycleCandidate))
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
            || !ReferenceEquals(binding.Candidate, source.CycleCandidate))
        {
            throw new InvalidOperationException(
                "Successful Move authoring must retain its exact typed candidate binding.");
        }

        Admission = currentAdmission;
        AuthorRequest = request;
        return true;
    }

    /// <summary>Checks the exact author output against its complete current admission graph.</summary>
    internal bool IsCurrentAuthorRequest(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest) =>
        Admission is not null
        && AuthorRequest is not null
        && ReferenceEquals(AuthorRequest, expectedAuthorRequest)
        && ReferenceEquals(Admission, admissions.Admission)
        && admissions.IsCurrentAdmission(Admission)
        && ReferenceEquals(Admission.Actor, source.Character)
        && recovery.IsCurrentRecovery(state.Placements, state.Evidence)
        && ReferenceEquals(moveAuthor.Request, AuthorRequest)
        && moveAuthor.CandidateBinding
            is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate> binding
        && ReferenceEquals(binding.Request, AuthorRequest)
        && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(binding.Candidate, source.CycleCandidate)
        && ReferenceEquals(AuthorRequest.Actor, Admission.Actor)
        && AuthorRequest.ActionKinds.Count == 1
        && ReferenceEquals(AuthorRequest.ActionKinds[0], CharacterSabotageActionKind.Move);
}
