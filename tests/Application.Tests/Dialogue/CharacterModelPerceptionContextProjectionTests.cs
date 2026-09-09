using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelPerceptionContextProjectionTests
{
    [Fact]
    public void ProjectsIdentifiedDescriptionsInPerceptionOrderWithExactText()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        string firstText = "  Pale petals with a silver edge.  ";
        string secondText = "A weathered stone marker.";
        WorldObject first = CreateWorldObject("healing-flowers", "Healing flowers", firstText);
        WorldObject second = CreateWorldObject("stone-marker", "Stone marker", secondText);
        WorldObjectSnapshot worldObjects = new([second, first]);
        WorldObjectVisibility firstVisibility = new(
            new ObjectId("healing-flowers"),
            VisibilityState.Identified);
        WorldObjectVisibility secondVisibility = new(
            new ObjectId("stone-marker"),
            VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = new(
            new CharacterId("healer"),
            [firstVisibility, secondVisibility]);

        CharacterModelContextEntry[] entries = CharacterModelPerceptionContextProjection
            .Project(character, perception, worldObjects)
            .ToArray();

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.FilteredPerception, entry.Kind);
                Assert.Same(firstText, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.FilteredPerception, entry.Kind);
                Assert.Same(secondText, entry.Text);
            });
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(firstVisibility, entry),
            entry => Assert.Same(secondVisibility, entry));
        Assert.Collection(
            worldObjects.Entries,
            entry => Assert.Same(second, entry),
            entry => Assert.Same(first, entry));
    }

    [Fact]
    public void ExcludesEveryOtherVisibilityStateAndEmptyDescriptions()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        WorldObjectVisibility[] visibilityEntries = VisibilityState.All
            .Select((state, index) => new WorldObjectVisibility(
                new ObjectId($"object-{index}"),
                state))
            .ToArray();
        WorldObject[] worldObjects = VisibilityState.All
            .Select((_, index) => CreateWorldObject(
                $"object-{index}",
                $"Hidden name {index}",
                index == 2 ? string.Empty : $"Hidden description {index}"))
            .ToArray();
        CharacterPerceptionSnapshot perception = new(character.Identity.Id, visibilityEntries);

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelPerceptionContextProjection.Project(
                character,
                perception,
                new WorldObjectSnapshot(worldObjects));

        Assert.Empty(entries);
        Assert.Equal(VisibilityState.All.Count, perception.Entries.Count);
        Assert.Equal(VisibilityState.All.Count, worldObjects.Length);
    }

    [Fact]
    public void RejectsMissingIdentifiedObjectWithoutChangingInputs()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        WorldObject retained = CreateWorldObject("retained-object", "Retained", "Retained text.");
        WorldObjectVisibility missing = new(
            new ObjectId("missing-object"),
            VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = new(character.Identity.Id, [missing]);
        WorldObjectSnapshot worldObjects = new([retained]);

        Assert.Throws<ArgumentException>(() => CharacterModelPerceptionContextProjection.Project(
            character,
            perception,
            worldObjects));
        Assert.Same(missing, Assert.Single(perception.Entries));
        Assert.Same(retained, Assert.Single(worldObjects.Entries));
    }

    [Fact]
    public void AcceptsNoPerceptionAndRejectsNullOrMismatchedInputs()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterPerceptionSnapshot perception = new(character.Identity.Id, []);
        WorldObjectSnapshot worldObjects = new([]);
        CharacterPerceptionSnapshot wrongPerception = new(new CharacterId("someone-else"), []);

        Assert.Empty(CharacterModelPerceptionContextProjection.Project(
            character,
            perception,
            worldObjects));
        Assert.Throws<ArgumentNullException>(() => CharacterModelPerceptionContextProjection.Project(
            null!,
            perception,
            worldObjects));
        Assert.Throws<ArgumentNullException>(() => CharacterModelPerceptionContextProjection.Project(
            character,
            null!,
            worldObjects));
        Assert.Throws<ArgumentNullException>(() => CharacterModelPerceptionContextProjection.Project(
            character,
            perception,
            null!));
        Assert.Throws<ArgumentException>(() => CharacterModelPerceptionContextProjection.Project(
            character,
            wrongPerception,
            worldObjects));
    }

    internal static WorldObject CreateWorldObject(
        string objectId,
        string displayName,
        string description) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), displayName),
            new WorldObjectDescription(description));
}
