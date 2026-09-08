using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Persistence;

/// <summary>Decides whether saved discovery state belongs to one known scenario character.</summary>
public static class DiscoverySaveStateRestorationPolicy
{
    /// <summary>
    /// Returns whether the saved state belongs to the searcher and references only known objects.
    /// </summary>
    public static bool CanRestore(
        DiscoverySaveState state,
        CharacterId searcherCharacterId,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(searcherCharacterId);
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (state.Perception.CharacterId != searcherCharacterId
            || state.Perception.Entries.Any(entry => !IsKnown(entry.ObjectId, worldObjects)))
        {
            return false;
        }

        return state.FocusedSearchActivities.Entries.All(activity => IsRestorable(
                activity.Request.SearcherCharacterId,
                activity.Request.TargetObjectId,
                searcherCharacterId,
                worldObjects))
            && state.FocusedSearchOutcomes.Entries.All(outcome => IsRestorable(
                outcome.Request.SearcherCharacterId,
                outcome.Request.TargetObjectId,
                searcherCharacterId,
                worldObjects))
            && state.PassiveNoticingCandidates.Entries.All(candidate => IsRestorable(
                candidate.ObserverCharacterId,
                candidate.TargetObjectId,
                searcherCharacterId,
                worldObjects))
            && state.ObservationReinterpretationCandidates.Entries.All(candidate => IsRestorable(
                candidate.ObserverCharacterId,
                candidate.TargetObjectId,
                searcherCharacterId,
                worldObjects));
    }

    private static bool IsRestorable(
        CharacterId ownerCharacterId,
        ObjectId targetObjectId,
        CharacterId searcherCharacterId,
        WorldObjectSnapshot worldObjects) =>
        ownerCharacterId == searcherCharacterId && IsKnown(targetObjectId, worldObjects);

    private static bool IsKnown(ObjectId objectId, WorldObjectSnapshot worldObjects) =>
        WorldObjectLookup.FindById(objectId, worldObjects) is not null;
}
