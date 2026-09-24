using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact recovery candidate for an acknowledged third-cycle Move.</summary>
public partial class CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator : Node
{
    private CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator binding = null!;

    /// <summary>Gets the one exact third-cycle Move recovery candidate authored here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator>(
            "../CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator");
    }

    /// <summary>Authors once from the exact acknowledged outcome and pre-move source placement.</summary>
    public bool TryAuthor(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult,
        WorldObjectPlacementSnapshot? expectedPlacements,
        DiscoveryEvidenceSnapshot? expectedEvidence,
        CharacterObjectSabotageMoveCandidate? expectedCandidate)
    {
        if (RecoveryCandidate is not null
            || expectedCandidate is null
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

        RecoveryCandidate = new CharacterObjectSabotageMoveRecoveryCandidate(
            expectedCandidate,
            sourcePlacement);
        return true;
    }

    /// <summary>Checks the exact authored recovery candidate against current acknowledgement.</summary>
    internal bool IsCurrentRecoveryCandidate(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate)
    {
        CharacterObjectSabotageMoveConsiderationResult? result = binding.AcknowledgedResult;
        return expectedCandidate is not null
            && result is not null
            && ReferenceEquals(RecoveryCandidate, expectedCandidate)
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
                expectedCandidate.SourcePlacement);
    }
}
