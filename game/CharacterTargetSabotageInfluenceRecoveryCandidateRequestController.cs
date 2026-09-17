using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one consideration request for the exact current recovery candidate.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateRequestController : Node
{
    private CharacterTargetSabotageInfluenceRecoveryCandidateController candidates = null!;

    /// <summary>Gets the exact one-shot consideration request accepted by this controller.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidates = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
    }

    /// <summary>Creates one request for the exact current recoverable influence candidate.</summary>
    public bool TryCreate(CharacterTargetSabotageInfluenceRecoveryCandidate expectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(expectedCandidate);

        if (Request is not null
            || !ReferenceEquals(candidates.Candidate, expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceResult influenceResult =
            expectedCandidate.InfluenceResult;
        Character target = influenceResult.Candidate.Occurrence.Target;
        CharacterRelationshipMemory relationship = influenceResult.Relationship;
        if (!CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                relationship,
                expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            target,
            relationship,
            [expectedCandidate]);
        CharacterTargetSabotageInfluenceRecoveryDecision abstention = new(
            target.Identity.Id,
            null);
        if (!CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
                request,
                abstention))
        {
            return false;
        }

        Request = request;
        return true;
    }
}
