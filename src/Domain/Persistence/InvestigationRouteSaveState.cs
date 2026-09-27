using System.Collections.ObjectModel;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains only stable identifiers for initialized opening-region route lifecycle state.</summary>
public sealed class InvestigationRouteSaveState
{
    /// <summary>Creates one immutable ID-only route lifecycle document.</summary>
    public InvestigationRouteSaveState(
        IEnumerable<InvestigationRouteProgressSaveEntry> progress,
        IEnumerable<string> acceptedCompletionRouteKeys,
        string? consumedCompletionRouteKey)
    {
        Progress = Copy(progress, nameof(progress));
        AcceptedCompletionRouteKeys = Copy(acceptedCompletionRouteKeys, nameof(acceptedCompletionRouteKeys));
        if (AcceptedCompletionRouteKeys.Any(key => !CanonicalIdentifierText.IsCanonical(key)))
        {
            throw new ArgumentException(
                "Accepted completion route keys must be canonical.",
                nameof(acceptedCompletionRouteKeys));
        }

        if (consumedCompletionRouteKey is not null
            && !CanonicalIdentifierText.IsCanonical(consumedCompletionRouteKey))
        {
            throw new ArgumentException(
                "A consumed completion route key must be canonical.",
                nameof(consumedCompletionRouteKey));
        }

        ConsumedCompletionRouteKey = consumedCompletionRouteKey;
    }

    /// <summary>Gets one saved progress entry per canonical route in authored order.</summary>
    public IReadOnlyList<InvestigationRouteProgressSaveEntry> Progress { get; }

    /// <summary>Gets accepted completion route keys in observation order.</summary>
    public IReadOnlyList<string> AcceptedCompletionRouteKeys { get; }

    /// <summary>Gets the consumed accepted route key, or null before consumption.</summary>
    public string? ConsumedCompletionRouteKey { get; }

    private static ReadOnlyCollection<T> Copy<T>(IEnumerable<T> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        List<T> retained = values.ToList();
        if (retained.Any(value => value is null))
        {
            throw new ArgumentException("Saved route values cannot contain null entries.", parameterName);
        }

        return retained.AsReadOnly();
    }
}

/// <summary>Retains ID-only observations for one canonical opening-region route.</summary>
public sealed class InvestigationRouteProgressSaveEntry
{
    /// <summary>Creates one immutable route-key and observation-ID composition.</summary>
    public InvestigationRouteProgressSaveEntry(
        string routeKey,
        IEnumerable<LocationId> recordedLocationIds,
        IEnumerable<EvidenceId> recordedEvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(routeKey);
        if (!CanonicalIdentifierText.IsCanonical(routeKey))
        {
            throw new ArgumentException("A saved route key must be canonical.", nameof(routeKey));
        }

        RouteKey = routeKey;
        RecordedLocationIds = Copy(recordedLocationIds, nameof(recordedLocationIds));
        RecordedEvidenceIds = Copy(recordedEvidenceIds, nameof(recordedEvidenceIds));
    }

    /// <summary>Gets the canonical route key.</summary>
    public string RouteKey { get; }

    /// <summary>Gets recorded location identifiers in record order.</summary>
    public IReadOnlyList<LocationId> RecordedLocationIds { get; }

    /// <summary>Gets recorded evidence identifiers in record order.</summary>
    public IReadOnlyList<EvidenceId> RecordedEvidenceIds { get; }

    private static ReadOnlyCollection<T> Copy<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        List<T> retained = values.ToList();
        if (retained.Any(value => value is null))
        {
            throw new ArgumentException("Saved route identifiers cannot contain null entries.", parameterName);
        }

        return retained.AsReadOnly();
    }
}
