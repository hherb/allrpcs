namespace Allrpcs.Application.Journal;

/// <summary>Composes a safe experiments-section snapshot from validated exact provenance.</summary>
public static class JournalExperimentSectionSnapshotProjection
{
    /// <summary>Returns a fresh snapshot without exposing its journal or authoritative events.</summary>
    public static JournalExperimentSectionSnapshot Project(
        JournalExperimentSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<JournalExperimentAuthoredCopy> safeEntries =
            JournalExperimentAuthoredCopyProjection.Project(binding);

        return new JournalExperimentSectionSnapshot(
            binding.Section,
            binding.SeekerId,
            safeEntries);
    }
}
