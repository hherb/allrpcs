using System.Runtime.CompilerServices;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact authoritative influence-recovery relationship transition.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryController : Node
{
    private readonly ConditionalWeakTable<
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest,
        object> resolvedRequests = new();
    private readonly object resolvedRequestMarker = new();

    /// <summary>Gets the initialized exact current relationship, if any.</summary>
    public CharacterRelationshipMemory? Relationship { get; private set; }

    /// <summary>Gets the latest exact applied influence-recovery result, if any.</summary>
    public CharacterTargetSabotageInfluenceRecoveryResult? LatestAppliedResult { get; private set; }

    /// <summary>Initializes this controller once from one exact current relationship.</summary>
    public bool TryInitialize(CharacterRelationshipMemory relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (Relationship is not null)
        {
            return false;
        }

        Relationship = relationship;
        return true;
    }

    /// <summary>Resolves one exact accepted recovery result from current relationship state.</summary>
    public bool TryResolveConsideration(
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request =
            considerationResult.Request;
        if (resolvedRequests.TryGetValue(request, out _)
            || !ReferenceEquals(Relationship, request.Relationship))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceRecoveryResult? result =
            CharacterTargetSabotageInfluenceRecoveryConsiderationTransition.Apply(
                considerationResult);
        if (result is not null)
        {
            Relationship = result.Relationship;
            LatestAppliedResult = result;
        }

        resolvedRequests.Add(request, resolvedRequestMarker);
        return true;
    }
}
