using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Projects passive-noticing candidates for one character without changing the recorded snapshot.
/// </summary>
public static class PassiveNoticingCandidateProjection
{
    /// <summary>
    /// Returns a new snapshot containing candidates for the supplied character in stable order.
    /// </summary>
    public static PassiveNoticingCandidateSnapshot ForCharacter(
        CharacterId observerCharacterId,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(observerCharacterId);
        ArgumentNullException.ThrowIfNull(candidates);

        return new PassiveNoticingCandidateSnapshot(
            candidates.Entries.Where(
                candidate => candidate.ObserverCharacterId == observerCharacterId));
    }
}
