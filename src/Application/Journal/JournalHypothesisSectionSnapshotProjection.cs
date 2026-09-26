namespace Allrpcs.Application.Journal;

/// <summary>Composes a safe hypotheses-section snapshot from validated exact provenance.</summary>
public static class JournalHypothesisSectionSnapshotProjection
{
    /// <summary>Returns a fresh snapshot without exposing its journal or source aggregates.</summary>
    public static JournalHypothesisSectionSnapshot Project(
        JournalHypothesisSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<JournalHypothesisAuthoredCopy> safeEntries =
            JournalHypothesisAuthoredCopyProjection.Project(binding);

        return new JournalHypothesisSectionSnapshot(
            binding.Section,
            binding.SeekerId,
            safeEntries);
    }
}
