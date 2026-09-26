using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Journal;

/// <summary>Retains exact filtered observations with their authoritative world-object snapshot.</summary>
public sealed class JournalObservationWorldBinding
{
    internal JournalObservationWorldBinding(
        JournalObservationSectionBinding observations,
        WorldObjectSnapshot worldObjects)
    {
        Observations = observations;
        WorldObjects = worldObjects;
    }

    /// <summary>Gets the exact observations-section and filtered-perception binding.</summary>
    public JournalObservationSectionBinding Observations { get; }

    /// <summary>Gets the exact authoritative world-object snapshot covering every visible target.</summary>
    public WorldObjectSnapshot WorldObjects { get; }
}
