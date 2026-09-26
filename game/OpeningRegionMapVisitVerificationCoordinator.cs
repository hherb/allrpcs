using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Coordinates one explicit exact live opening-region map visit verification.</summary>
public partial class OpeningRegionMapVisitVerificationCoordinator : Node
{
    private const float VisitRadiusPixels = 24.0f;

    private SeekerMapController map = null!;
    private OpeningRegionProjectionSource projectionSource = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    private Seeker seeker = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        map = GetNode<SeekerMapController>("../SeekerMapController");
        projectionSource = GetNode<OpeningRegionProjectionSource>(
            "../OpeningRegionProjectionSource");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        seeker = GetNode<Seeker>("../Seeker");
    }

    /// <summary>Verifies one explicitly supplied exact current anchor visit.</summary>
    public bool TryVerifyVisit(
        SeekerMapSnapshot expectedMap,
        CharacterId seekerId,
        NumericsVector2 seekerPosition,
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(map.Map, expectedMap)
            || !ReferenceEquals(seekerCharacter.Character.Identity.Id, seekerId)
            || !ReferenceEquals(projectionSource.Projection, projection)
            || ToNumerics(seeker.Position) != seekerPosition)
        {
            return false;
        }

        OpeningRegionLocationVisit? visit;
        try
        {
            visit = OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                seekerPosition,
                VisitRadiusPixels);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return visit is not null
            && map.TryVerifyVisit(expectedMap, projection, visit, location);
    }

    private static NumericsVector2 ToNumerics(Godot.Vector2 position) =>
        new(position.X, position.Y);
}
