using System.Collections.ObjectModel;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Retains one ID-only opening-region investigation route.</summary>
public sealed class OpeningRegionInvestigationRoute
{
    /// <summary>Creates one immutable route without activating or evaluating it.</summary>
    public OpeningRegionInvestigationRoute(
        string key,
        IEnumerable<LocationId> locationIds,
        IEnumerable<EvidenceId> evidenceIds)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(locationIds);
        ArgumentNullException.ThrowIfNull(evidenceIds);

        if (!CanonicalIdentifierText.IsCanonical(key))
        {
            throw new ArgumentException("A route key must be canonical.", nameof(key));
        }

        Key = key;
        LocationIds = RetainUnique(locationIds, nameof(locationIds));
        EvidenceIds = RetainUnique(evidenceIds, nameof(evidenceIds));
    }

    /// <summary>Gets the canonical route key.</summary>
    public string Key { get; }

    /// <summary>Gets the exact location identifiers in stable authored order.</summary>
    public IReadOnlyList<LocationId> LocationIds { get; }

    /// <summary>Gets the exact evidence identifiers in stable authored order.</summary>
    public IReadOnlyList<EvidenceId> EvidenceIds { get; }

    private static ReadOnlyCollection<TIdentifier> RetainUnique<TIdentifier>(
        IEnumerable<TIdentifier> identifiers,
        string parameterName)
        where TIdentifier : class
    {
        List<TIdentifier> retained = [];
        HashSet<TIdentifier> unique = [];
        foreach (TIdentifier? identifier in identifiers)
        {
            if (identifier is null || !unique.Add(identifier))
            {
                throw new ArgumentException(
                    "Route identifier lists cannot contain null or duplicate values.",
                    parameterName);
            }

            retained.Add(identifier);
        }

        if (retained.Count == 0)
        {
            throw new ArgumentException(
                "A route identifier list requires at least one value.",
                parameterName);
        }

        return retained.AsReadOnly();
    }
}
