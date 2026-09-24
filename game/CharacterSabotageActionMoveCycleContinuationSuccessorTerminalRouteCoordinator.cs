using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact recovered fifth-cycle Move as a terminal route.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator
    : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator authoring =
        null!;
    private CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator binding =
        null!;
    private CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator recovery =
        null!;

    /// <summary>Gets the exact guarded author request retained at terminal state.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind retained with the terminal route.</summary>
    public CharacterSabotageActionKind? ActionKind { get; private set; }

    /// <summary>Gets the exact recovered placement authority retained at terminal state.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <summary>Gets the exact recovered evidence authority retained at terminal state.</summary>
    public DiscoveryEvidenceSnapshot? RecoveredEvidence { get; private set; }

    /// <summary>Gets the exact guarded author request whose terminal route was reset.</summary>
    public CharacterSabotageActionConsiderationRequest? ResetAuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind whose terminal route was reset.</summary>
    public CharacterSabotageActionKind? ResetActionKind { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator");
        binding = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator");
        recovery = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator");
    }

    /// <summary>Retains only the exact current recovered fifth-cycle Move graph once.</summary>
    public bool TryRetain(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements,
        DiscoveryEvidenceSnapshot? expectedRecoveredEvidence)
    {
        if (AuthorRequest is not null
            || ActionKind is not null
            || RecoveredPlacements is not null
            || RecoveredEvidence is not null
            || ResetAuthorRequest is not null
            || ResetActionKind is not null
            || expectedAuthorRequest is null
            || !ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
            || !HasRetainedAuthorProvenance(expectedAuthorRequest)
            || expectedAuthorRequest.ActionKinds.Count != 1
            || !ReferenceEquals(
                expectedAuthorRequest.ActionKinds[0],
                CharacterSabotageActionKind.Move)
            || !recovery.IsCurrentRecovery(
                expectedRecoveredPlacements,
                expectedRecoveredEvidence))
        {
            return false;
        }

        AuthorRequest = expectedAuthorRequest;
        ActionKind = expectedActionKind;
        RecoveredPlacements = expectedRecoveredPlacements;
        RecoveredEvidence = expectedRecoveredEvidence;
        return true;
    }

    /// <summary>Clears only this seam's exact retained terminal route once.</summary>
    public bool TryReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements,
        DiscoveryEvidenceSnapshot? expectedRecoveredEvidence)
    {
        if (ResetAuthorRequest is not null
            || ResetActionKind is not null
            || !IsCurrentRoute(
                expectedAuthorRequest,
                expectedActionKind,
                expectedRecoveredPlacements,
                expectedRecoveredEvidence))
        {
            return false;
        }

        AuthorRequest = null;
        ActionKind = null;
        RecoveredPlacements = null;
        RecoveredEvidence = null;
        ResetAuthorRequest = expectedAuthorRequest;
        ResetActionKind = expectedActionKind;
        return true;
    }

    /// <summary>Checks the exact retained route while recovered authority remains current.</summary>
    internal bool IsCurrentRoute(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements,
        DiscoveryEvidenceSnapshot? expectedRecoveredEvidence) =>
        expectedAuthorRequest is not null
        && expectedRecoveredPlacements is not null
        && expectedRecoveredEvidence is not null
        && ReferenceEquals(AuthorRequest, expectedAuthorRequest)
        && ReferenceEquals(ActionKind, expectedActionKind)
        && ReferenceEquals(RecoveredPlacements, expectedRecoveredPlacements)
        && ReferenceEquals(RecoveredEvidence, expectedRecoveredEvidence)
        && ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
        && HasRetainedAuthorProvenance(expectedAuthorRequest)
        && recovery.IsCurrentRecovery(expectedRecoveredPlacements, expectedRecoveredEvidence);

    /// <summary>Checks the exact reset provenance while recovered authority remains current.</summary>
    internal bool IsCurrentReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements,
        DiscoveryEvidenceSnapshot? expectedRecoveredEvidence) =>
        expectedAuthorRequest is not null
        && expectedRecoveredPlacements is not null
        && expectedRecoveredEvidence is not null
        && AuthorRequest is null
        && ActionKind is null
        && RecoveredPlacements is null
        && RecoveredEvidence is null
        && ReferenceEquals(ResetAuthorRequest, expectedAuthorRequest)
        && ReferenceEquals(ResetActionKind, expectedActionKind)
        && ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
        && HasRetainedAuthorProvenance(expectedAuthorRequest)
        && recovery.IsCurrentRecovery(expectedRecoveredPlacements, expectedRecoveredEvidence);

    private bool HasRetainedAuthorProvenance(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest) =>
        ReferenceEquals(authoring.AuthorRequest, expectedAuthorRequest)
        && binding.CandidateBinding is { } candidateBinding
        && ReferenceEquals(candidateBinding.Request, expectedAuthorRequest)
        && binding.IsRetainedAcknowledgedBinding(
            binding.AcknowledgedResult,
            candidateBinding);
}
