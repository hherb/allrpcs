using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact inert temporary move-sabotage source.</summary>
public partial class TemporaryCharacterObjectSabotageMoveSourceSmokeCheck : Node
{
    private const string WaterCaskObjectId = "village-water-cask";
    private const string SourceLocationId = "village-square";
    private const string DestinationLocationId = "abandoned-wagon";

    private static readonly string[] ExpectedPropertyNames =
    [
        "Candidate",
        "Character",
        "Destination",
        "Evidence",
        "PhysicalAccess",
        "Placements",
        "RecoveryCandidate",
        "SourceLocation",
        "WorldObjects",
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveCandidate candidate = source.Candidate;
        CharacterObjectSabotageOccurrence occurrence = candidate.Destination.Occurrence;
        WorldObject target = occurrence.Target;
        WorldObjectPlacement sourcePlacement = source.Placements.Entries.Single();
        Location destination = candidate.Destination.Destination;

        if (!HasStableExactOutput(source, candidate)
            || source.WorldObjects.Entries.Count != 3
            || !ReferenceEquals(source.WorldObjects.Entries[^1], target)
            || target.Identity.Id.Value != WaterCaskObjectId
            || !ReferenceEquals(sourcePlacement.WorldObject, target)
            || !ReferenceEquals(sourcePlacement.Location, source.SourceLocation)
            || source.SourceLocation.Identity.Id.Value != SourceLocationId
            || !ReferenceEquals(destination, source.Destination)
            || destination.Identity.Id.Value != DestinationLocationId
            || ReferenceEquals(source.SourceLocation, source.Destination)
            || source.SourceLocation.Identity.Id == source.Destination.Identity.Id
            || !ReferenceEquals(occurrence.Proposal.ActorId, source.Character.Identity.Id)
            || !ReferenceEquals(
                occurrence.Proposal.AccessRequirement.TargetObjectId,
                target.Identity.Id)
            || !ReferenceEquals(
                source.PhysicalAccess.AccessibleObjectIds.Single(),
                target.Identity.Id)
            || occurrence.Proposal.ActionKind != CharacterSabotageActionKind.Move
            || !ReferenceEquals(candidate.Evidence.Occurrence, occurrence)
            || !ReferenceEquals(candidate.Evidence.Evidence.ObjectId, target.Identity.Id)
            || source.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                source.Evidence,
                candidate)
            || !ReferenceEquals(source.RecoveryCandidate.Candidate, candidate)
            || !ReferenceEquals(source.RecoveryCandidate.SourcePlacement, sourcePlacement)
            || !HasClosedSourceSurface())
        {
            Fail("The temporary move source did not retain one exact eligible recoverable move.");
            return;
        }

        GD.Print(
            "Temporary move sabotage source smoke passed: exact enemy, tail water cask, access, "
            + "source and destination, empty evidence, eligible move, aligned recovery, inert "
            + "startup, and closed output.");
        GetTree().Quit();
    }

    private static bool HasStableExactOutput(
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveCandidate candidate) =>
        ReferenceEquals(source.Character, source.Character)
        && ReferenceEquals(source.PhysicalAccess, source.PhysicalAccess)
        && ReferenceEquals(source.WorldObjects, source.WorldObjects)
        && ReferenceEquals(source.SourceLocation, source.SourceLocation)
        && ReferenceEquals(source.Destination, source.Destination)
        && ReferenceEquals(source.Placements, source.Placements)
        && ReferenceEquals(source.Evidence, source.Evidence)
        && ReferenceEquals(source.Candidate, candidate)
        && ReferenceEquals(source.RecoveryCandidate, source.RecoveryCandidate)
        && source.Placements.Entries
            is System.Collections.ObjectModel.ReadOnlyCollection<WorldObjectPlacement>
        && source.Evidence.Entries
            is System.Collections.ObjectModel.ReadOnlyCollection<Allrpcs.Domain.Discovery.DiscoveryEvidence>;

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageMoveSource);
        string[] propertyNames = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        return propertyNames.SequenceEqual(ExpectedPropertyNames)
            && methods.Length == 1
            && methods[0].Name == nameof(_Ready);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
