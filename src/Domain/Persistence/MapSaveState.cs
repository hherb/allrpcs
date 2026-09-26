using System.Collections.ObjectModel;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains only visibility-safe opening-region map values for persistence.</summary>
public sealed class MapSaveState
{
    /// <summary>Creates one immutable visibility-safe map persistence state.</summary>
    public MapSaveState(
        CharacterId seekerId,
        IEnumerable<LocationId> reportedLocationIds,
        IEnumerable<LocationId> verifiedLocationIds,
        IEnumerable<MapNoteSaveEntry> notes)
    {
        ArgumentNullException.ThrowIfNull(seekerId);

        SeekerId = seekerId;
        ReportedLocationIds = Copy(reportedLocationIds, nameof(reportedLocationIds));
        VerifiedLocationIds = Copy(verifiedLocationIds, nameof(verifiedLocationIds));
        Notes = Copy(notes, nameof(notes));
    }

    /// <summary>Gets the exact stable identifier of the map owner.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets safe reported-location identifiers in stable order.</summary>
    public IReadOnlyList<LocationId> ReportedLocationIds { get; }

    /// <summary>Gets safe verified-location identifiers in stable order.</summary>
    public IReadOnlyList<LocationId> VerifiedLocationIds { get; }

    /// <summary>Gets location-bound player-authored note values in stable order.</summary>
    public IReadOnlyList<MapNoteSaveEntry> Notes { get; }

    private static ReadOnlyCollection<T> Copy<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        List<T> retained = values.ToList();
        if (retained.Any(value => value is null))
        {
            throw new ArgumentException(
                "Persisted map values cannot contain null entries.",
                parameterName);
        }

        return retained.AsReadOnly();
    }
}

/// <summary>Retains one persisted player-authored note for one location identifier.</summary>
public sealed class MapNoteSaveEntry
{
    /// <summary>Creates one immutable persisted location and note-text pair.</summary>
    public MapNoteSaveEntry(LocationId locationId, string text)
    {
        ArgumentNullException.ThrowIfNull(locationId);
        ArgumentNullException.ThrowIfNull(text);

        LocationId = locationId;
        Text = text;
    }

    /// <summary>Gets the exact stable identifier of the noted location.</summary>
    public LocationId LocationId { get; }

    /// <summary>Gets the persisted player-authored text for restoration validation.</summary>
    public string Text { get; }
}
