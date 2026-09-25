namespace Allrpcs.Domain.Locations;

/// <summary>Connects two distinct exact authored locations for ordinary traversal.</summary>
public sealed record OpeningRegionConnection
{
    /// <summary>Creates one immutable bidirectional opening-region connection.</summary>
    public OpeningRegionConnection(Location first, Location second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (first.Identity.Id == second.Identity.Id)
        {
            throw new ArgumentException(
                "An opening-region connection requires two distinct locations.",
                nameof(second));
        }

        First = first;
        Second = second;
    }

    /// <summary>Gets the first exact endpoint.</summary>
    public Location First { get; }

    /// <summary>Gets the second exact endpoint.</summary>
    public Location Second { get; }
}
