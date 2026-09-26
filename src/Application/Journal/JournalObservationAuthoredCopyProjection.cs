using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Journal;

/// <summary>Copies authored text only from exactly identified structural observations.</summary>
public static class JournalObservationAuthoredCopyProjection
{
    /// <summary>Returns fresh read-only copies in stable identified-observation order.</summary>
    public static IReadOnlyList<JournalObservationAuthoredCopy> Project(
        IReadOnlyList<JournalObservationEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Any(entry => entry is null))
        {
            throw new ArgumentException(
                "Structural journal observations cannot contain null entries.",
                nameof(entries));
        }

        List<JournalObservationAuthoredCopy> copies = new();
        foreach (JournalObservationEntry entry in entries)
        {
            if (entry.Visibility.VisibilityState != VisibilityState.Identified)
            {
                continue;
            }

            copies.Add(new JournalObservationAuthoredCopy(
                entry.Visibility.ObjectId,
                entry.WorldObject.Identity.DisplayName,
                entry.WorldObject.Description.Text));
        }

        return copies.AsReadOnly();
    }
}
