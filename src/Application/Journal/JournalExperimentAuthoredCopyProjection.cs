using Allrpcs.Domain.Experiments;

namespace Allrpcs.Application.Journal;

/// <summary>Projects journal-safe authored copies from exact experiment provenance.</summary>
public static class JournalExperimentAuthoredCopyProjection
{
    /// <summary>Returns fresh read-only copies in stable recorded order.</summary>
    public static IReadOnlyList<JournalExperimentAuthoredCopy> Project(
        JournalExperimentSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        List<JournalExperimentAuthoredCopy> copies =
            new(binding.Experiments.Records.Count);
        foreach (ExperimentJournalRecord record in binding.Experiments.Records)
        {
            copies.Add(new JournalExperimentAuthoredCopy(
                record.ExperimentId,
                record.Procedure,
                record.ObservedOutcome));
        }

        return copies.AsReadOnly();
    }
}
