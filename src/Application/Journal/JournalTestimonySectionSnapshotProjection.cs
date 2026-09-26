namespace Allrpcs.Application.Journal;

/// <summary>Composes a safe testimony-section snapshot from validated exact provenance.</summary>
public static class JournalTestimonySectionSnapshotProjection
{
    /// <summary>Returns a fresh snapshot without exposing its journal, events, or audiences.</summary>
    public static JournalTestimonySectionSnapshot Project(
        JournalTestimonySectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<JournalTestimonyAuthoredCopy> safeEntries =
            JournalTestimonyAuthoredCopyProjection.Project(binding);

        return new JournalTestimonySectionSnapshot(
            binding.Section,
            binding.SeekerId,
            safeEntries);
    }
}
