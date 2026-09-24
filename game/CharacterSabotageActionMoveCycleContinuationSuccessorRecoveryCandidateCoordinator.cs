using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one eligible recovery candidate for an acknowledged fifth-cycle Move.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator
        binding = null!;

    /// <summary>Gets the one exact fifth-cycle Move recovery candidate authored here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <summary>Gets the exact explicitly supplied evidence view retained here.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator");
    }

    /// <summary>Authors once from exact acknowledgement, authority, candidate, and evidence.</summary>
    public bool TryAuthor(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult,
        WorldObjectPlacementSnapshot? expectedPlacements,
        DiscoveryEvidenceSnapshot? expectedEvidence,
        CharacterObjectSabotageMoveCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (RecoveryCandidate is not null
            || EvidenceView is not null
            || expectedCandidate is null
            || expectedEvidenceView is null
            || !binding.IsCurrentAcknowledgedOutcome(
                expectedResult,
                expectedPlacements,
                expectedEvidence)
            || !ReferenceEquals(expectedResult!.SelectedCandidate, expectedCandidate))
        {
            return false;
        }

        WorldObjectPlacement? sourcePlacement = null;
        foreach (WorldObjectPlacement entry in expectedResult.Request.Placements.Entries)
        {
            if (!ReferenceEquals(entry.WorldObject, expectedCandidate.Destination.Occurrence.Target))
            {
                continue;
            }

            if (sourcePlacement is not null)
            {
                return false;
            }

            sourcePlacement = entry;
        }

        if (sourcePlacement is null)
        {
            return false;
        }

        CharacterObjectSabotageMoveRecoveryCandidate candidate = new(
            expectedCandidate,
            sourcePlacement);
        if (!CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
            expectedPlacements!,
            expectedEvidence!,
            expectedEvidenceView,
            candidate))
        {
            return false;
        }

        RecoveryCandidate = candidate;
        EvidenceView = expectedEvidenceView;
        return true;
    }

    /// <summary>Checks exact retained recovery authorship against current acknowledgement.</summary>
    internal bool IsCurrentRecoveryCandidate(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        CharacterObjectSabotageMoveConsiderationResult? result = binding.AcknowledgedResult;
        return expectedCandidate is not null
            && expectedEvidenceView is not null
            && result is not null
            && ReferenceEquals(RecoveryCandidate, expectedCandidate)
            && ReferenceEquals(EvidenceView, expectedEvidenceView)
            && binding.IsCurrentAcknowledgedOutcome(result, binding.Placements, binding.Evidence)
            && ReferenceEquals(result.SelectedCandidate, expectedCandidate.Candidate)
            && result.Request.Placements.Entries.Count(entry =>
                ReferenceEquals(
                    entry.WorldObject,
                    expectedCandidate.Candidate.Destination.Occurrence.Target)) == 1
            && ReferenceEquals(
                result.Request.Placements.Entries.Single(entry =>
                    ReferenceEquals(
                        entry.WorldObject,
                        expectedCandidate.Candidate.Destination.Occurrence.Target)),
                expectedCandidate.SourcePlacement)
            && CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                binding.Placements!,
                binding.Evidence!,
                expectedEvidenceView,
                expectedCandidate);
    }
}
