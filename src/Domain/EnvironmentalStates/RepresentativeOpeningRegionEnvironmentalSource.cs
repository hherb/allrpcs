using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>Authors the qualitative baseline weather, lighting, and sound regions.</summary>
public static class RepresentativeOpeningRegionEnvironmentalSource
{
    /// <summary>Creates fresh baseline states over one exact retained opening topology.</summary>
    public static OpeningRegionEnvironmentalRegionSet Create(OpeningRegionSnapshot topology)
    {
        ArgumentNullException.ThrowIfNull(topology);
        EnvironmentalState steadyRain = new(
            "steady-rain",
            "Steady rain lies over the opening region without advancing time or disease.");
        EnvironmentalState windDrivenRain = new(
            "wind-driven-rain",
            "Exposed rain moves across the southern road and abandoned wagon.");
        EnvironmentalState shelteredLight = new(
            "warm-sheltered-light",
            "Sheltered village spaces retain a muted warm greybox light.");
        EnvironmentalState waterwayLight = new(
            "cool-overcast-light",
            "The stream and laundry sit under cool overcast greybox light.");
        EnvironmentalState woodlandLight = new(
            "canopy-muted-light",
            "The woodland edge and old shrine sit beneath canopy-muted greybox light.");
        EnvironmentalState roadLight = new(
            "exposed-overcast-light",
            "The southern road and wagon sit beneath exposed overcast greybox light.");
        EnvironmentalState villageSound = new(
            "muted-village-rain",
            "Shelter and nearby structures soften the sound of steady rain.");
        EnvironmentalState waterwaySound = new(
            "running-water-and-rain",
            "Running water and rain define the stream and abandoned-laundry corridor.");
        EnvironmentalState woodlandSound = new(
            "rain-under-trees",
            "Rain through leaves defines the clearing, shrine, and closed path.");
        EnvironmentalState roadSound = new(
            "road-wind-and-wagon",
            "Wind, rain, and an occasional inert wagon creak define the southern road.");
        EnvironmentalStateSet states = new(
        [
            steadyRain,
            windDrivenRain,
            shelteredLight,
            waterwayLight,
            woodlandLight,
            roadLight,
            villageSound,
            waterwaySound,
            woodlandSound,
            roadSound,
        ]);
        return new OpeningRegionEnvironmentalRegionSet(
            topology,
            states,
            CreateRegions(topology, states));
    }

    private static IEnumerable<OpeningRegionEnvironmentalRegion> CreateRegions(
        OpeningRegionSnapshot topology,
        EnvironmentalStateSet states)
    {
        Dictionary<string, Location> locations = topology.Locations.ToDictionary(
            location => location.Identity.Id.Value,
            StringComparer.Ordinal);
        Dictionary<string, EnvironmentalState> byKey = states.Entries.ToDictionary(
            state => state.Key,
            StringComparer.Ordinal);
        return
        [
            Region(
                "village-shelter",
                locations,
                byKey,
                ["sickhouse", "healer-garden", "village-square"],
                "steady-rain",
                "warm-sheltered-light",
                "muted-village-rain"),
            Region(
                "waterway",
                locations,
                byKey,
                ["village-edge", "stream", "abandoned-laundry"],
                "steady-rain",
                "cool-overcast-light",
                "running-water-and-rain"),
            Region(
                "woodland-edge",
                locations,
                byKey,
                ["woodland-clearing", "old-shrine", "closed-deeper-forest-path"],
                "steady-rain",
                "canopy-muted-light",
                "rain-under-trees"),
            Region(
                "southern-road",
                locations,
                byKey,
                ["southern-road", "abandoned-wagon"],
                "wind-driven-rain",
                "exposed-overcast-light",
                "road-wind-and-wagon"),
        ];
    }

    private static OpeningRegionEnvironmentalRegion Region(
        string key,
        IReadOnlyDictionary<string, Location> locations,
        IReadOnlyDictionary<string, EnvironmentalState> states,
        IEnumerable<string> locationIds,
        string weatherKey,
        string lightingKey,
        string soundKey) => new(
            key,
            locationIds.Select(id => locations[id]),
            states[weatherKey],
            states[lightingKey],
            states[soundKey]);
}
