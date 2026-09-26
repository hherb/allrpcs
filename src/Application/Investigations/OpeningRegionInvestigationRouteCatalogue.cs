namespace Allrpcs.Application.Investigations;

/// <summary>Retains opening-region investigation routes in stable authored order.</summary>
public sealed class OpeningRegionInvestigationRouteCatalogue
{
    private static readonly string[] CanonicalKeys =
    [
        "stream-laundry",
        "shrine",
        "road-wagon",
    ];

    /// <summary>Creates an immutable catalogue with unique canonical route keys.</summary>
    public OpeningRegionInvestigationRouteCatalogue(
        IEnumerable<OpeningRegionInvestigationRoute> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        List<OpeningRegionInvestigationRoute> retained = routes.ToList();
        if (retained.Count != CanonicalKeys.Length)
        {
            throw new ArgumentException(
                "The opening-region catalogue requires all three canonical routes.",
                nameof(routes));
        }

        for (int index = 0; index < CanonicalKeys.Length; index++)
        {
            OpeningRegionInvestigationRoute? route = retained[index];
            if (route is null
                || !string.Equals(route.Key, CanonicalKeys[index], StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Routes must use the complete canonical opening-region order.",
                    nameof(routes));
            }
        }

        Routes = retained.AsReadOnly();
    }

    /// <summary>Gets exact routes in stable authored order.</summary>
    public IReadOnlyList<OpeningRegionInvestigationRoute> Routes { get; }
}
