namespace Allrpcs.Application.Journal;

/// <summary>Composes a safe observations-section snapshot from validated exact provenance.</summary>
public static class JournalObservationSectionSnapshotProjection
{
    /// <summary>Returns a fresh snapshot without exposing perception or authoritative world state.</summary>
    public static JournalObservationSectionSnapshot Project(
        JournalObservationWorldBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<JournalObservationEntry> structuralEntries =
            JournalObservationEntryProjection.Project(binding);
        IReadOnlyList<JournalObservationAuthoredCopy> safeEntries =
            JournalObservationAuthoredCopyProjection.Project(structuralEntries);

        return new JournalObservationSectionSnapshot(
            binding.Observations.Section,
            binding.Observations.SeekerId,
            safeEntries);
    }
}
