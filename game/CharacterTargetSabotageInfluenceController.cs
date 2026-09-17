using System.Runtime.CompilerServices;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact authoritative influence relationship transition.</summary>
public partial class CharacterTargetSabotageInfluenceController : Node
{
    private readonly ConditionalWeakTable<
        CharacterTargetSabotageInfluenceConsiderationRequest,
        object> resolvedRequests = new();
    private readonly object resolvedRequestMarker = new();

    /// <summary>Gets the initialized exact current relationship, if any.</summary>
    public CharacterRelationshipMemory? Relationship { get; private set; }

    /// <summary>Gets the latest exact applied influence result, if any.</summary>
    public CharacterTargetSabotageInfluenceResult? LatestAppliedResult { get; private set; }

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

    /// <summary>Resolves one exact accepted consideration result from current relationship state.</summary>
    public bool TryResolveConsideration(
        CharacterTargetSabotageInfluenceConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageInfluenceConsiderationRequest request =
            considerationResult.Request;
        if (resolvedRequests.TryGetValue(request, out _)
            || !ReferenceEquals(Relationship, request.Relationship))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceResult? result =
            CharacterTargetSabotageInfluenceConsiderationTransition.Apply(considerationResult);
        if (result is not null)
        {
            Relationship = result.Relationship;
            LatestAppliedResult = result;
        }

        resolvedRequests.Add(request, resolvedRequestMarker);
        return true;
    }
}
