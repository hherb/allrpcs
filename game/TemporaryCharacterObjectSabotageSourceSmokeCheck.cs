using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact authored enemy concealment candidate without applying it.</summary>
public partial class TemporaryCharacterObjectSabotageSourceSmokeCheck : Node
{
    private static readonly string[] ExpectedPropertyNames =
        ["Candidate", "Candidates", "Character", "Concealment", "Evidence", "PhysicalAccess", "WorldObjects"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageSource source =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        CharacterObjectSabotageEvidence candidate = source.Candidate;

        if (source.Candidates.Count != 1
            || source.Candidates.Count > CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount
            || !ReferenceEquals(source.Candidates[0], candidate)
            || source.Candidates is not System.Collections.ObjectModel.ReadOnlyCollection<
                CharacterObjectSabotageEvidence>
            || !ReferenceEquals(candidate.Occurrence.Proposal.ActorId, source.Character.Identity.Id)
            || !ReferenceEquals(
                candidate.Occurrence.Proposal.AccessRequirement.TargetObjectId,
                candidate.Occurrence.Target.Identity.Id)
            || !ReferenceEquals(
                candidate.Occurrence.Proposal.ActionKind,
                CharacterSabotageActionKind.Conceal)
            || !ReferenceEquals(
                candidate.Occurrence.Proposal.AccessRequirement.RequiredKnowledgeId,
                source.Character.Knowledge.KnowledgeReferences.Single())
            || source.WorldObjects.Entries.Count != 3
            || !ReferenceEquals(source.WorldObjects.Entries[^1], candidate.Occurrence.Target)
            || !ReferenceEquals(
                source.PhysicalAccess.AccessibleObjectIds.Single(),
                candidate.Occurrence.Target.Identity.Id)
            || !ReferenceEquals(candidate.Evidence.ObjectId, candidate.Occurrence.Target.Identity.Id)
            || candidate.Evidence.MinimumVisibilityState != VisibilityState.Noticed
            || candidate.Evidence.IsCritical
            || source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageConcealmentPolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                source.Concealment,
                source.Evidence,
                candidate)
            || !HasClosedSourceSurface())
        {
            Fail("The temporary source did not author one exact eligible concealment candidate.");
            return;
        }

        GD.Print(
            "Temporary object sabotage source smoke passed: exact enemy, access, world object, "
            + "empty initial state, concealment occurrence, and noticed non-critical evidence "
            + "are eligible without applying an effect.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSource);
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
