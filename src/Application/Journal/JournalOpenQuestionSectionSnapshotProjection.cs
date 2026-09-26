namespace Allrpcs.Application.Journal;

/// <summary>Composes a safe open-questions snapshot from validated exact provenance.</summary>
public static class JournalOpenQuestionSectionSnapshotProjection
{
    /// <summary>Returns a fresh snapshot without exposing its journal or source aggregates.</summary>
    public static JournalOpenQuestionSectionSnapshot Project(
        JournalOpenQuestionSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<JournalOpenQuestionAuthoredCopy> safeEntries =
            JournalOpenQuestionAuthoredCopyProjection.Project(binding);

        return new JournalOpenQuestionSectionSnapshot(
            binding.Section,
            binding.SeekerId,
            safeEntries);
    }
}
