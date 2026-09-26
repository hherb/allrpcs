using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Journal;

/// <summary>Validates authoritative world-object coverage for filtered journal observations.</summary>
public static class JournalObservationWorldProjection
{
    /// <summary>Binds exact inputs only when every visible target exists in the supplied world.</summary>
    public static JournalObservationWorldBinding Bind(
        JournalObservationSectionBinding observations,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(worldObjects);

        foreach (WorldObjectVisibility visibility in observations.Perception.Entries)
        {
            if (WorldObjectLookup.FindById(visibility.ObjectId, worldObjects) is null)
            {
                throw new ArgumentException(
                    "Every filtered journal-perception target must exist in the authoritative world.",
                    nameof(worldObjects));
            }
        }

        return new JournalObservationWorldBinding(observations, worldObjects);
    }
}
