using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact inert temporary substitution-sabotage source.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionSourceSmokeCheck : Node
{
    private const string WaterCaskObjectId = "village-water-cask";
    private const string ReplacementObjectId = "tainted-water-cask";

    private static readonly string[] ExpectedPropertyNames =
        ["Candidate", "Character", "Evidence", "PhysicalAccess", "Substitutions", "WorldObjects"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageSubstitutionSource source =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        CharacterObjectSabotageSubstitutionCandidate candidate = source.Candidate;
        CharacterObjectSabotageOccurrence occurrence = candidate.Substitution.Occurrence;
        WorldObject target = occurrence.Target;
        WorldObject replacement = candidate.Substitution.Replacement;
        DiscoveryEvidence discoverableEvidence = candidate.Evidence.Evidence;

        if (!HasStableExactOutput(source, candidate)
            || source.WorldObjects.Entries.Count != 4
            || !ReferenceEquals(source.WorldObjects.Entries[2], target)
            || target.Identity.Id.Value != WaterCaskObjectId
            || replacement.Identity.Id.Value != ReplacementObjectId
            || ReferenceEquals(target, replacement)
            || target.Identity.Id == replacement.Identity.Id
            || !ReferenceEquals(occurrence.Proposal.ActorId, source.Character.Identity.Id)
            || !ReferenceEquals(
                occurrence.Proposal.AccessRequirement.TargetObjectId,
                target.Identity.Id)
            || !ReferenceEquals(
                occurrence.Proposal.AccessRequirement.RequiredKnowledgeId,
                source.Character.Knowledge.KnowledgeReferences.Single())
            || !ReferenceEquals(
                source.PhysicalAccess.AccessibleObjectIds.Single(),
                target.Identity.Id)
            || occurrence.Proposal.ActionKind != CharacterSabotageActionKind.Substitute
            || !ReferenceEquals(candidate.Evidence.Occurrence, occurrence)
            || !ReferenceEquals(discoverableEvidence.ObjectId, target.Identity.Id)
            || discoverableEvidence.MinimumVisibilityState != VisibilityState.Noticed
            || discoverableEvidence.IsCritical
            || source.Substitutions.Entries.Count != 0
            || source.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageSubstitutionPolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Substitutions,
                source.Evidence,
                candidate)
            || !HasClosedSourceSurface())
        {
            Fail("The temporary substitution source did not retain one exact eligible candidate.");
            return;
        }

        GD.Print(
            "Temporary substitution sabotage source smoke passed: exact enemy, water cask, "
            + "complete access, distinct replacement, empty immutable state, eligible evidence-bound "
            + "candidate, inert startup, and closed output.");
        GetTree().Quit();
    }

    private static bool HasStableExactOutput(
        TemporaryCharacterObjectSabotageSubstitutionSource source,
        CharacterObjectSabotageSubstitutionCandidate candidate) =>
        ReferenceEquals(source.Character, source.Character)
        && ReferenceEquals(source.PhysicalAccess, source.PhysicalAccess)
        && ReferenceEquals(source.WorldObjects, source.WorldObjects)
        && ReferenceEquals(source.Substitutions, source.Substitutions)
        && ReferenceEquals(source.Evidence, source.Evidence)
        && ReferenceEquals(source.Candidate, candidate)
        && source.PhysicalAccess.AccessibleObjectIds
            is System.Collections.ObjectModel.ReadOnlyCollection<Allrpcs.Domain.Identifiers.ObjectId>
        && source.WorldObjects.Entries
            is System.Collections.ObjectModel.ReadOnlyCollection<WorldObject>
        && source.Substitutions.Entries
            is System.Collections.ObjectModel.ReadOnlyCollection<CharacterObjectSabotageSubstitution>
        && source.Evidence.Entries
            is System.Collections.ObjectModel.ReadOnlyCollection<DiscoveryEvidence>;

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSubstitutionSource);
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
