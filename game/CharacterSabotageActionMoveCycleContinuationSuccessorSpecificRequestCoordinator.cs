using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one acknowledged fifth-cycle Move result into its typed request.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator
        binding = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact fresh typed fifth-cycle Move request composed here.</summary>
    public CharacterObjectSabotageMoveConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator");
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Composes once from the exact acknowledged result and current typed candidate.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            expectedBinding)
    {
        WorldObjectPlacementSnapshot? placements = state.Placements;
        DiscoveryEvidenceSnapshot? evidence = state.Evidence;
        if (Request is not null
            || placements is null
            || evidence is null
            || !binding.IsCurrentAcknowledgedBinding(expectedResult, expectedBinding)
            || !ReferenceEquals(expectedBinding!.Request.Actor, source.Character)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                placements,
                evidence,
                expectedBinding.Candidate))
        {
            return false;
        }

        Request = new CharacterObjectSabotageMoveConsiderationRequest(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [expectedBinding.Candidate]);
        return true;
    }

    /// <summary>Checks the fresh typed request against its current acknowledged graph.</summary>
    internal bool IsCurrentRequest(
        CharacterObjectSabotageMoveConsiderationRequest? expectedRequest)
    {
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            candidateBinding = binding.CandidateBinding;
        return IsRetainedRequest(expectedRequest)
            && binding.IsCurrentAcknowledgedBinding(
                binding.AcknowledgedResult,
                candidateBinding)
            && ReferenceEquals(Request!.Placements, state.Placements)
            && ReferenceEquals(Request.Evidence, state.Evidence)
            && CharacterObjectSabotageMovePolicy.CanApply(
                Request.Character,
                Request.PhysicalAccess,
                Request.WorldObjects,
                Request.Placements,
                Request.Evidence,
                candidateBinding!.Candidate);
    }

    /// <summary>Checks retained request provenance after its snapshots have been applied.</summary>
    internal bool IsRetainedRequest(
        CharacterObjectSabotageMoveConsiderationRequest? expectedRequest)
    {
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            candidateBinding = binding.CandidateBinding;
        return Request is not null
            && ReferenceEquals(Request, expectedRequest)
            && binding.IsRetainedAcknowledgedBinding(
                binding.AcknowledgedResult,
                candidateBinding)
            && ReferenceEquals(Request.Character, source.Character)
            && ReferenceEquals(Request.PhysicalAccess, source.PhysicalAccess)
            && ReferenceEquals(Request.WorldObjects, source.WorldObjects)
            && Request.Candidates.Count == 1
            && ReferenceEquals(Request.Candidates[0], candidateBinding!.Candidate);
    }
}
