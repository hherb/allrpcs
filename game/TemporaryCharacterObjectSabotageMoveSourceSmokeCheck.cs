using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact inert temporary initial and follow-up move candidates.</summary>
public partial class TemporaryCharacterObjectSabotageMoveSourceSmokeCheck : Node
{
    private const string WaterCaskObjectId = "village-water-cask";
    private const string SourceLocationId = "village-square";
    private const string DestinationLocationId = "abandoned-wagon";

    private static readonly string[] ExpectedPropertyNames =
    [
        "Candidate",
        "Character",
        "ContinuationCandidate",
        "CycleCandidate",
        "Destination",
        "Evidence",
        "FollowUpCandidate",
        "PhysicalAccess",
        "Placements",
        "RecoveryCandidate",
        "SourceLocation",
        "SuccessorCandidate",
        "WorldObjects",
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterObjectSabotageMoveCandidate candidate = source.Candidate;
        CharacterObjectSabotageMoveCandidate followUpCandidate = source.FollowUpCandidate;
        CharacterObjectSabotageMoveCandidate cycleCandidate = source.CycleCandidate;
        CharacterObjectSabotageMoveCandidate continuationCandidate = source.ContinuationCandidate;
        CharacterObjectSabotageMoveCandidate successorCandidate = source.SuccessorCandidate;
        CharacterObjectSabotageOccurrence occurrence = candidate.Destination.Occurrence;
        WorldObject target = occurrence.Target;
        WorldObjectPlacement sourcePlacement = source.Placements.Entries.Single();
        Location destination = candidate.Destination.Destination;
        DiscoveryEvidenceSnapshot priorEvidence = new([candidate.Evidence.Evidence]);

        if (!HasStableExactOutput(
                source,
                candidate,
                followUpCandidate,
                cycleCandidate,
                continuationCandidate,
                successorCandidate)
            || source.WorldObjects.Entries.Count != 4
            || !ReferenceEquals(source.WorldObjects.Entries[2], target)
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
            || !ReferenceEquals(followUpCandidate.Destination, candidate.Destination)
            || ReferenceEquals(followUpCandidate.Evidence, candidate.Evidence)
            || followUpCandidate.Evidence.Evidence.EvidenceId
                == candidate.Evidence.Evidence.EvidenceId
            || !ReferenceEquals(cycleCandidate.Destination, candidate.Destination)
            || ReferenceEquals(cycleCandidate.Evidence, candidate.Evidence)
            || ReferenceEquals(cycleCandidate.Evidence, followUpCandidate.Evidence)
            || cycleCandidate.Evidence.Evidence.EvidenceId
                == candidate.Evidence.Evidence.EvidenceId
            || cycleCandidate.Evidence.Evidence.EvidenceId
                == followUpCandidate.Evidence.Evidence.EvidenceId
            || !ReferenceEquals(continuationCandidate.Destination, candidate.Destination)
            || ReferenceEquals(continuationCandidate.Evidence, candidate.Evidence)
            || ReferenceEquals(continuationCandidate.Evidence, followUpCandidate.Evidence)
            || ReferenceEquals(continuationCandidate.Evidence, cycleCandidate.Evidence)
            || continuationCandidate.Evidence.Evidence.EvidenceId
                == candidate.Evidence.Evidence.EvidenceId
            || continuationCandidate.Evidence.Evidence.EvidenceId
                == followUpCandidate.Evidence.Evidence.EvidenceId
            || continuationCandidate.Evidence.Evidence.EvidenceId
                == cycleCandidate.Evidence.Evidence.EvidenceId
            || !ReferenceEquals(successorCandidate.Destination, candidate.Destination)
            || ReferenceEquals(successorCandidate.Evidence, candidate.Evidence)
            || ReferenceEquals(successorCandidate.Evidence, followUpCandidate.Evidence)
            || ReferenceEquals(successorCandidate.Evidence, cycleCandidate.Evidence)
            || ReferenceEquals(successorCandidate.Evidence, continuationCandidate.Evidence)
            || successorCandidate.Evidence.Evidence.EvidenceId
                == candidate.Evidence.Evidence.EvidenceId
            || successorCandidate.Evidence.Evidence.EvidenceId
                == followUpCandidate.Evidence.Evidence.EvidenceId
            || successorCandidate.Evidence.Evidence.EvidenceId
                == cycleCandidate.Evidence.Evidence.EvidenceId
            || successorCandidate.Evidence.Evidence.EvidenceId
                == continuationCandidate.Evidence.Evidence.EvidenceId
            || source.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                source.Evidence,
                candidate)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                priorEvidence,
                followUpCandidate)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                new DiscoveryEvidenceSnapshot(
                    [candidate.Evidence.Evidence, followUpCandidate.Evidence.Evidence]),
                cycleCandidate)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                new DiscoveryEvidenceSnapshot(
                    [
                        candidate.Evidence.Evidence,
                        followUpCandidate.Evidence.Evidence,
                        cycleCandidate.Evidence.Evidence,
                    ]),
                continuationCandidate)
            || !CharacterObjectSabotageMovePolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Placements,
                new DiscoveryEvidenceSnapshot(
                    [
                        candidate.Evidence.Evidence,
                        followUpCandidate.Evidence.Evidence,
                        cycleCandidate.Evidence.Evidence,
                        continuationCandidate.Evidence.Evidence,
                    ]),
                successorCandidate)
            || !ReferenceEquals(source.RecoveryCandidate.Candidate, candidate)
            || !ReferenceEquals(source.RecoveryCandidate.SourcePlacement, sourcePlacement)
            || !HasClosedSourceSurface())
        {
            Fail("The temporary move source did not retain exact eligible move candidates.");
            return;
        }

        GD.Print(
            "Temporary move sabotage source smoke passed: exact enemy, water cask, access, "
            + "source and destination, empty then distinct evidence, eligible initial, follow-up, "
            + "next-cycle, continuation, and successor moves, aligned recovery, inert startup, and closed "
            + "output.");
        GetTree().Quit();
    }

    private static bool HasStableExactOutput(
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveCandidate candidate,
        CharacterObjectSabotageMoveCandidate followUpCandidate,
        CharacterObjectSabotageMoveCandidate cycleCandidate,
        CharacterObjectSabotageMoveCandidate continuationCandidate,
        CharacterObjectSabotageMoveCandidate successorCandidate) =>
        ReferenceEquals(source.Character, source.Character)
        && ReferenceEquals(source.PhysicalAccess, source.PhysicalAccess)
        && ReferenceEquals(source.WorldObjects, source.WorldObjects)
        && ReferenceEquals(source.SourceLocation, source.SourceLocation)
        && ReferenceEquals(source.Destination, source.Destination)
        && ReferenceEquals(source.Placements, source.Placements)
        && ReferenceEquals(source.Evidence, source.Evidence)
        && ReferenceEquals(source.Candidate, candidate)
        && ReferenceEquals(source.FollowUpCandidate, followUpCandidate)
        && ReferenceEquals(source.CycleCandidate, cycleCandidate)
        && ReferenceEquals(source.ContinuationCandidate, continuationCandidate)
        && ReferenceEquals(source.SuccessorCandidate, successorCandidate)
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
