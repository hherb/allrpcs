using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks stale-safe exact move-placement projection onto one object body.</summary>
public partial class CharacterObjectSabotageMovePresentationControllerSmokeCheck : Node
{
    private static readonly Vector2 SourcePosition = new(120.0f, 240.0f);
    private static readonly Vector2 DestinationPosition = new(480.0f, 360.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterObjectSabotageMovePresentationController controller = new();
        Node2D body = new() { Position = new Vector2(1.0f, 2.0f) };
        WorldObject target = WorldObject("village-water-cask");
        WorldObject unrelated = WorldObject("herb-patch");
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        WorldObjectPlacement retained = new(unrelated, Location("meadow"));
        WorldObjectPlacement sourcePlacement = new(target, source);
        WorldObjectPlacementSnapshot initial = new([retained, sourcePlacement]);

        if (!RejectsInvalidInitialization(target, body, source, destination, initial)
            || !controller.TryInitialize(
                target,
                body,
                source,
                SourcePosition,
                destination,
                DestinationPosition,
                initial)
            || !ReferenceEquals(controller.Target, target)
            || !ReferenceEquals(controller.LastPresentedPlacements, initial)
            || body.Position != SourcePosition
            || controller.TryInitialize(
                target,
                body,
                source,
                SourcePosition,
                destination,
                DestinationPosition,
                initial))
        {
            Fail("Move presentation did not initialize one exact source placement.");
            return;
        }

        WorldObjectPlacementSnapshot moved = new(
            [retained, new WorldObjectPlacement(target, destination)]);
        if (!RejectsInvalidPresentation(controller, body, target, source, destination, initial, moved)
            || !controller.TryPresent(initial, moved)
            || !ReferenceEquals(controller.LastPresentedPlacements, moved)
            || body.Position != DestinationPosition
            || controller.TryPresent(initial, moved)
            || !ReferenceEquals(controller.LastPresentedPlacements, moved)
            || body.Position != DestinationPosition)
        {
            Fail("Move presentation did not project one exact destination once.");
            return;
        }

        WorldObjectPlacementSnapshot recovered = new([retained, sourcePlacement]);
        if (!controller.TryPresent(moved, recovered)
            || !ReferenceEquals(controller.LastPresentedPlacements, recovered)
            || body.Position != SourcePosition
            || !ReferenceEquals(recovered.Entries[0], retained)
            || !ReferenceEquals(recovered.Entries[1], sourcePlacement)
            || !HasClosedSurface())
        {
            Fail("Move presentation did not restore the exact authored source position.");
            return;
        }

        GD.Print(
            "Move sabotage presentation smoke passed: exact source initialization, explicit "
            + "destination projection and recovery, stable unrelated placement, and stale, "
            + "value-equal, unsupported, repeated, null, and invalid input rejection.");
        body.Free();
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsInvalidInitialization(
        WorldObject target,
        Node2D body,
        Location source,
        Location destination,
        WorldObjectPlacementSnapshot placements)
    {
        CharacterObjectSabotageMovePresentationController[] controllers =
            Enumerable.Range(0, 8)
                .Select(_ => new CharacterObjectSabotageMovePresentationController())
                .ToArray();
        bool rejected = ThrowsArgumentNull(() => controllers[0].TryInitialize(
                null!, body, source, SourcePosition, destination, DestinationPosition, placements))
            && ThrowsArgumentNull(() => controllers[1].TryInitialize(
                target, null!, source, SourcePosition, destination, DestinationPosition, placements))
            && ThrowsArgumentNull(() => controllers[2].TryInitialize(
                target, body, null!, SourcePosition, destination, DestinationPosition, placements))
            && ThrowsArgumentNull(() => controllers[3].TryInitialize(
                target, body, source, SourcePosition, null!, DestinationPosition, placements))
            && ThrowsArgumentNull(() => controllers[4].TryInitialize(
                target, body, source, SourcePosition, destination, DestinationPosition, null!))
            && ThrowsArgument(() => controllers[5].TryInitialize(
                target, body, source, SourcePosition, source, DestinationPosition, placements))
            && ThrowsArgumentOutOfRange(() => controllers[6].TryInitialize(
                target,
                body,
                source,
                new Vector2(float.NaN, 0.0f),
                destination,
                DestinationPosition,
                placements))
            && ThrowsArgumentOutOfRange(() => controllers[7].TryInitialize(
                target,
                body,
                source,
                SourcePosition,
                destination,
                new Vector2(0.0f, float.PositiveInfinity),
                placements));
        foreach (CharacterObjectSabotageMovePresentationController controller in controllers)
        {
            rejected = rejected
                && controller.Target is null
                && controller.LastPresentedPlacements is null;
            controller.Free();
        }

        return rejected;
    }

    private static bool RejectsInvalidPresentation(
        CharacterObjectSabotageMovePresentationController controller,
        Node2D body,
        WorldObject target,
        Location source,
        Location destination,
        WorldObjectPlacementSnapshot initial,
        WorldObjectPlacementSnapshot moved)
    {
        WorldObjectPlacementSnapshot stale = new(initial.Entries);
        WorldObjectPlacementSnapshot replacementTarget = new(
            [new WorldObjectPlacement(WorldObject(target.Identity.Id.Value), destination)]);
        WorldObjectPlacementSnapshot replacementDestination = new(
            [new WorldObjectPlacement(target, Location(destination.Identity.Id.Value))]);
        WorldObjectPlacementSnapshot unsupported = new(
            [new WorldObjectPlacement(target, Location("woodland"))]);

        return !controller.TryPresent(stale, moved)
            && !controller.TryPresent(initial, replacementTarget)
            && !controller.TryPresent(initial, replacementDestination)
            && !controller.TryPresent(initial, unsupported)
            && ThrowsArgumentNull(() => controller.TryPresent(null!, moved))
            && ThrowsArgumentNull(() => controller.TryPresent(initial, null!))
            && ReferenceEquals(controller.LastPresentedPlacements, initial)
            && body.Position == SourcePosition
            && !ReferenceEquals(source, destination);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageMovePresentationController);
        string[] propertyNames = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return propertyNames.SequenceEqual(["LastPresentedPlacements", "Target"])
            && methodNames.SequenceEqual(["TryInitialize", "TryPresent"]);
    }

    private static bool ThrowsArgumentNull(Action action) =>
        Throws<ArgumentNullException>(action);

    private static bool ThrowsArgument(Action action) =>
        Throws<ArgumentException>(action);

    private static bool ThrowsArgumentOutOfRange(Action action) =>
        Throws<ArgumentOutOfRangeException>(action);

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), $"Location {id}"),
            new LocationDescription($"Description for {id}."));

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
