namespace Allrpcs.Application.Journal;

/// <summary>Composes safe observations with the remaining content-free journal sections.</summary>
public static class JournalSnapshotProjection
{
    private const int FirstPendingSectionIndex = 1;

    /// <summary>Returns a fresh journal snapshot only for exact canonical observation provenance.</summary>
    public static JournalSnapshot Compose(
        JournalSectionCatalogue catalogue,
        JournalObservationSectionSnapshot observations)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(observations);

        if (!ReferenceEquals(observations.Section, catalogue.Sections[0]))
        {
            throw new ArgumentException(
                "The observations snapshot must retain the catalogue's exact observations definition.",
                nameof(observations));
        }

        IReadOnlyList<JournalSectionDefinition> pendingSections = catalogue.Sections
            .Skip(FirstPendingSectionIndex)
            .ToList()
            .AsReadOnly();
        return new JournalSnapshot(catalogue, observations, pendingSections);
    }
}
