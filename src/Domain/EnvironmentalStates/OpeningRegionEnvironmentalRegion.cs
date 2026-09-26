using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>Groups exact opening locations under one authored weather, light, and sound profile.</summary>
public sealed class OpeningRegionEnvironmentalRegion
{
    /// <summary>Creates one immutable environmental region without runtime presentation.</summary>
    public OpeningRegionEnvironmentalRegion(
        string key,
        IEnumerable<Location> locations,
        EnvironmentalState weatherState,
        EnvironmentalState lightingState,
        EnvironmentalState soundState)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(weatherState);
        ArgumentNullException.ThrowIfNull(lightingState);
        ArgumentNullException.ThrowIfNull(soundState);
        List<Location> retained = [];
        HashSet<Location> exactLocations = new(ReferenceEqualityComparer.Instance);
        foreach (Location location in locations)
        {
            if (location is null || !exactLocations.Add(location))
            {
                throw new ArgumentException(
                    "Environmental-region locations must be non-null exact unique instances.",
                    nameof(locations));
            }

            retained.Add(location);
        }

        if (retained.Count == 0)
        {
            throw new ArgumentException(
                "An environmental region requires at least one authored location.",
                nameof(locations));
        }

        Locations = retained.AsReadOnly();
        WeatherState = weatherState;
        LightingState = lightingState;
        SoundState = soundState;
    }

    /// <summary>Gets the stable canonical environmental-region key.</summary>
    public string Key { get; }

    /// <summary>Gets exact authored locations in stable supplied order.</summary>
    public IReadOnlyList<Location> Locations { get; }

    /// <summary>Gets the qualitative weather state for the region.</summary>
    public EnvironmentalState WeatherState { get; }

    /// <summary>Gets the qualitative lighting state for the region.</summary>
    public EnvironmentalState LightingState { get; }

    /// <summary>Gets the qualitative sound state for the region.</summary>
    public EnvironmentalState SoundState { get; }
}
