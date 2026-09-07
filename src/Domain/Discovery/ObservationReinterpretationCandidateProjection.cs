using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Projects observation-reinterpretation candidates for one character without changing the
/// recorded snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateProjection
{
    /// <summary>
    /// Returns a new snapshot containing candidates for the supplied character in stable order.
    /// </summary>
    public static ObservationReinterpretationCandidateSnapshot ForCharacter(
        CharacterId observerCharacterId,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(candidates);

        return new ObservationReinterpretationCandidateSnapshot(
            candidates.Entries.Where(
                candidate => candidate.ObserverCharacterId == observerCharacterId));
    }
}
