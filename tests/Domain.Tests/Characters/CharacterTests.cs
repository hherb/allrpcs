using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTests
{
    [Fact]
    public void RetainsEverySuppliedComponentExactly()
    {
        CharacterParts parts = CreateParts();

        Character character = parts.CreateCharacter();

        Assert.Same(parts.Identity, character.Identity);
        Assert.Same(parts.PublicBackground, character.PublicBackground);
        Assert.Same(parts.PrivateBackground, character.PrivateBackground);
        Assert.Same(parts.Capabilities, character.Capabilities);
        Assert.Same(parts.Limitations, character.Limitations);
        Assert.Same(parts.ControllerAssignment, character.ControllerAssignment);
        Assert.Same(parts.Conditions, character.Conditions);
        Assert.Same(parts.Inventory, character.Inventory);
        Assert.Same(parts.Knowledge, character.Knowledge);
        Assert.Same(parts.Beliefs, character.Beliefs);
        Assert.Same(parts.Relationships, character.Relationships);
        Assert.Same(parts.Goals, character.Goals);
    }

    [Fact]
    public void ComposesValidEmptyChildCollections()
    {
        Character character = CreateParts().CreateCharacter();

        Assert.Empty(character.PublicBackground.Text);
        Assert.Empty(character.PrivateBackground.Text);
        Assert.Empty(character.Capabilities.Entries);
        Assert.Empty(character.Limitations.Entries);
        Assert.Empty(character.Conditions.Entries);
        Assert.Empty(character.Inventory.ObjectReferences);
        Assert.Empty(character.Knowledge.KnowledgeReferences);
        Assert.Empty(character.Beliefs.Entries);
        Assert.Empty(character.Relationships.Entries);
        Assert.Empty(character.Goals.Entries);
    }

    [Fact]
    public void AcceptsEquivalentControllerCharacterIdentifier()
    {
        CharacterParts parts = CreateParts();
        CharacterParts equivalent = parts with
        {
            ControllerAssignment = new CharacterControllerAssignment(
                new CharacterId(parts.Identity.Id.Value),
                ControllerType.Llm),
        };

        Character character = equivalent.CreateCharacter();

        Assert.NotSame(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Equal(character.Identity.Id, character.ControllerAssignment.CharacterId);
    }

    [Fact]
    public void RejectsEveryNullComponent()
    {
        CharacterParts parts = CreateParts();
        CharacterParts[] invalidParts =
        [
            parts with { Identity = null! },
            parts with { PublicBackground = null! },
            parts with { PrivateBackground = null! },
            parts with { Capabilities = null! },
            parts with { Limitations = null! },
            parts with { ControllerAssignment = null! },
            parts with { Conditions = null! },
            parts with { Inventory = null! },
            parts with { Knowledge = null! },
            parts with { Beliefs = null! },
            parts with { Relationships = null! },
            parts with { Goals = null! },
        ];

        Assert.All(
            invalidParts,
            invalid => Assert.Throws<ArgumentNullException>(invalid.CreateCharacter));
    }

    [Fact]
    public void RejectsControllerAssignmentForAnotherCharacter()
    {
        CharacterParts parts = CreateParts();
        CharacterParts mismatched = parts with
        {
            ControllerAssignment = new CharacterControllerAssignment(
                new CharacterId("sage"),
                ControllerType.Llm),
        };

        Assert.Throws<ArgumentException>(mismatched.CreateCharacter);
    }

    [Fact]
    public void ExposesOnlyTheCompleteReadOnlyCompositionSurface()
    {
        Type characterType = typeof(Character);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Beliefs", typeof(CharacterBeliefSet)),
            ("Capabilities", typeof(CharacterCapabilitySet)),
            ("Conditions", typeof(CharacterConditionSet)),
            ("ControllerAssignment", typeof(CharacterControllerAssignment)),
            ("Goals", typeof(CharacterGoalSet)),
            ("Identity", typeof(CharacterIdentity)),
            ("Inventory", typeof(CharacterInventory)),
            ("Knowledge", typeof(CharacterKnowledge)),
            ("Limitations", typeof(CharacterLimitationSet)),
            ("PrivateBackground", typeof(PrivateCharacterBackground)),
            ("PublicBackground", typeof(PublicCharacterBackground)),
            ("Relationships", typeof(CharacterRelationshipSet)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = characterType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = characterType
            .GetMethods()
            .Where(method => method.DeclaringType == characterType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(characterType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(characterType.IsSealed);
    }

    private static CharacterParts CreateParts()
    {
        CharacterId characterId = new("travelling-healer");

        return new CharacterParts(
            new CharacterIdentity(characterId, "The travelling healer"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet(Array.Empty<CharacterCapability>()),
            new CharacterLimitationSet(Array.Empty<CharacterLimitation>()),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet(Array.Empty<CharacterCondition>()),
            new CharacterInventory(Array.Empty<ObjectId>()),
            new CharacterKnowledge(Array.Empty<KnowledgeId>()),
            new CharacterBeliefSet(Array.Empty<CharacterBelief>()),
            new CharacterRelationshipSet(Array.Empty<CharacterRelationship>()),
            new CharacterGoalSet(Array.Empty<CharacterGoal>()));
    }

    private sealed record CharacterParts(
        CharacterIdentity Identity,
        PublicCharacterBackground PublicBackground,
        PrivateCharacterBackground PrivateBackground,
        CharacterCapabilitySet Capabilities,
        CharacterLimitationSet Limitations,
        CharacterControllerAssignment ControllerAssignment,
        CharacterConditionSet Conditions,
        CharacterInventory Inventory,
        CharacterKnowledge Knowledge,
        CharacterBeliefSet Beliefs,
        CharacterRelationshipSet Relationships,
        CharacterGoalSet Goals)
    {
        public Character CreateCharacter()
        {
            return new Character(
                Identity,
                PublicBackground,
                PrivateBackground,
                Capabilities,
                Limitations,
                ControllerAssignment,
                Conditions,
                Inventory,
                Knowledge,
                Beliefs,
                Relationships,
                Goals);
        }
    }
}
