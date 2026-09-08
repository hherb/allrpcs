using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>Projects only evidence visible through one character's perception snapshot.</summary>
public static class DiscoveryEvidenceProjection
{
    private static readonly IReadOnlyDictionary<VisibilityState, int> VisibilityRanks =
        VisibilityState.All
            .Select((state, rank) => (state, rank))
            .ToDictionary(entry => entry.state, entry => entry.rank);

    /// <summary>Returns visible evidence identifiers without exposing omitted definitions.</summary>
    public static CharacterEvidenceViewSnapshot ForCharacter(
        DiscoveryEvidenceSnapshot evidence,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(perception);

        return new CharacterEvidenceViewSnapshot(
            perception.CharacterId,
            evidence.Entries
                .Where(definition => IsVisible(definition, perception))
                .Select(definition => definition.EvidenceId));
    }

    private static bool IsVisible(
        DiscoveryEvidence evidence,
        CharacterPerceptionSnapshot perception)
    {
        WorldObjectVisibility? visibility = perception.Entries.FirstOrDefault(
            entry => entry.ObjectId == evidence.ObjectId);
        return visibility is not null &&
            VisibilityRanks[visibility.VisibilityState] >=
            VisibilityRanks[evidence.MinimumVisibilityState];
    }
}
