using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Locations;

/// <summary>Authors the complete location topology required by the opening vertical slice.</summary>
public static class RepresentativeOpeningRegionSource
{
    /// <summary>Creates one fresh continuous multi-route opening-region graph.</summary>
    public static OpeningRegionSnapshot Create()
    {
        Location sickhouse = Location(
            "sickhouse",
            "Sickhouse",
            "A village sickhouse containing the first sleeping patient.");
        Location garden = Location(
            "healer-garden",
            "Healer's Garden",
            "A cultivated remedy garden beside the healer's preparation station.");
        Location square = Location(
            "village-square",
            "Village Square",
            "The central village meeting place and junction between investigation routes.");
        Location edge = Location(
            "village-edge",
            "Village Edge",
            "The outer village path leading toward water and woodland.");
        Location stream = Location(
            "stream",
            "Stream",
            "A flowing boundary connecting the village edge and abandoned laundry.");
        Location laundry = Location(
            "abandoned-laundry",
            "Abandoned Laundry",
            "A disused laundry beside the downstream route.");
        Location clearing = Location(
            "woodland-clearing",
            "Woodland Clearing",
            "A woodland clearing containing a concealed rare specimen.");
        Location shrine = Location(
            "old-shrine",
            "Old Shrine",
            "An old shrine containing layered inscription evidence.");
        Location road = Location(
            "southern-road",
            "Southern Road",
            "The southern route between the village and outer landmarks.");
        Location wagon = Location(
            "abandoned-wagon",
            "Abandoned Wagon",
            "An abandoned wagon beside the southern road.");
        Location closedPath = Location(
            "closed-deeper-forest-path",
            "Closed Forest Path",
            "A closed path marking the boundary toward the deeper forest.");
        Location[] locations =
        [
            sickhouse,
            garden,
            square,
            edge,
            stream,
            laundry,
            clearing,
            shrine,
            road,
            wagon,
            closedPath,
        ];
        OpeningRegionConnection[] connections =
        [
            new(square, sickhouse),
            new(square, garden),
            new(square, edge),
            new(square, road),
            new(edge, stream),
            new(stream, laundry),
            new(edge, clearing),
            new(laundry, clearing),
            new(clearing, shrine),
            new(shrine, road),
            new(road, wagon),
            new(clearing, closedPath),
        ];
        return new OpeningRegionSnapshot(sickhouse, locations, connections);
    }

    private static Location Location(string id, string displayName, string description) => new(
        new LocationIdentity(new LocationId(id), displayName),
        new LocationDescription(description));
}
