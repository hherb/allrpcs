using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one recovery candidate for the exact current applied influence result.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateController : Node
{
    private CharacterTargetSabotageInfluenceController influence = null!;

    /// <summary>Gets the exact one-shot recovery candidate accepted by this controller.</summary>
    public CharacterTargetSabotageInfluenceRecoveryCandidate? Candidate { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        influence = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
    }

    /// <summary>Creates one candidate from the exact current influence result and inverse change.</summary>
    public bool TryCreate(
        CharacterTargetSabotageInfluenceResult expectedInfluenceResult,
        CharacterRelationshipChange recoveryChange)
    {
        ArgumentNullException.ThrowIfNull(expectedInfluenceResult);
        ArgumentNullException.ThrowIfNull(recoveryChange);

        if (Candidate is not null
            || !ReferenceEquals(influence.LatestAppliedResult, expectedInfluenceResult)
            || !ReferenceEquals(influence.Relationship, expectedInfluenceResult.Relationship))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            expectedInfluenceResult,
            recoveryChange);
        if (!CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                influence.Relationship,
                candidate))
        {
            return false;
        }

        Candidate = candidate;
        return true;
    }
}
