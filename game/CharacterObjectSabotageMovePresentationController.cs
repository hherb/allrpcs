using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Projects one exact world-object placement onto explicit authored scene positions.</summary>
public partial class CharacterObjectSabotageMovePresentationController : Node
{
    private Node2D body = null!;
    private Location sourceLocation = null!;
    private Location destinationLocation = null!;
    private Vector2 sourcePosition;
    private Vector2 destinationPosition;

    /// <summary>Gets the exact authoritative world object represented by the body.</summary>
    public WorldObject? Target { get; private set; }

    /// <summary>Gets the exact most recently presented placement snapshot.</summary>
    public WorldObjectPlacementSnapshot? LastPresentedPlacements { get; private set; }

    /// <summary>Initializes exact authored placement projection and presents it once.</summary>
    public bool TryInitialize(
        WorldObject target,
        Node2D targetBody,
        Location source,
        Vector2 sourceScenePosition,
        Location destination,
        Vector2 destinationScenePosition,
        WorldObjectPlacementSnapshot placements)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(targetBody);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(placements);

        if (source.Identity.Id == destination.Identity.Id)
        {
            throw new ArgumentException(
                "Source and destination location identifiers must differ.",
                nameof(destination));
        }

        RequireFinite(sourceScenePosition, nameof(sourceScenePosition));
        RequireFinite(destinationScenePosition, nameof(destinationScenePosition));
        if (Target is not null || LastPresentedPlacements is not null)
        {
            return false;
        }

        if (!TryResolvePosition(
                placements,
                target,
                source,
                sourceScenePosition,
                destination,
                destinationScenePosition,
                out Vector2 position))
        {
            return false;
        }

        Target = target;
        body = targetBody;
        sourceLocation = source;
        sourcePosition = sourceScenePosition;
        destinationLocation = destination;
        destinationPosition = destinationScenePosition;
        body.Position = position;
        LastPresentedPlacements = placements;
        return true;
    }

    /// <summary>Projects one fresh exact placement from the last presented snapshot.</summary>
    public bool TryPresent(
        WorldObjectPlacementSnapshot expectedPlacements,
        WorldObjectPlacementSnapshot currentPlacements)
    {
        ArgumentNullException.ThrowIfNull(expectedPlacements);
        ArgumentNullException.ThrowIfNull(currentPlacements);

        if (Target is not { } target
            || !ReferenceEquals(LastPresentedPlacements, expectedPlacements)
            || ReferenceEquals(expectedPlacements, currentPlacements)
            || !TryResolvePosition(
                currentPlacements,
                target,
                sourceLocation,
                sourcePosition,
                destinationLocation,
                destinationPosition,
                out Vector2 position))
        {
            return false;
        }

        body.Position = position;
        LastPresentedPlacements = currentPlacements;
        return true;
    }

    private static bool TryResolvePosition(
        WorldObjectPlacementSnapshot placements,
        WorldObject target,
        Location source,
        Vector2 sourceScenePosition,
        Location destination,
        Vector2 destinationScenePosition,
        out Vector2 position)
    {
        WorldObjectPlacement? placement = placements.Entries.SingleOrDefault(
            entry => entry.WorldObject.Identity.Id == target.Identity.Id);
        if (placement is null || !ReferenceEquals(placement.WorldObject, target))
        {
            position = default;
            return false;
        }

        if (ReferenceEquals(placement.Location, source))
        {
            position = sourceScenePosition;
            return true;
        }

        if (ReferenceEquals(placement.Location, destination))
        {
            position = destinationScenePosition;
            return true;
        }

        position = default;
        return false;
    }

    private static void RequireFinite(Vector2 position, string parameterName)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, position, "Position must be finite.");
        }
    }
}
