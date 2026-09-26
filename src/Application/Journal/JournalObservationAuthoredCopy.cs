using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Copies visibility-safe authored observation text without authoritative world state.</summary>
public sealed class JournalObservationAuthoredCopy
{
    internal JournalObservationAuthoredCopy(
        ObjectId objectId,
        string displayName,
        string description)
    {
        ObjectId = objectId;
        DisplayName = displayName;
        Description = description;
    }

    /// <summary>Gets the exact filtered-perception object identifier.</summary>
    public ObjectId ObjectId { get; }

    /// <summary>Gets the exact authored display name copied after identification.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the exact authored description copied after identification.</summary>
    public string Description { get; }
}
