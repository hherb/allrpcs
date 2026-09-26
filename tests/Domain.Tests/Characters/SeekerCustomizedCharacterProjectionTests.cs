using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class SeekerCustomizedCharacterProjectionTests
{
    [Fact]
    public void ProjectsFreshCustomizedCharacterWithExactProvenance()
    {
        Character current = Character(new CharacterId("seeker"));
        SeekerCustomizationSelectionState selected = SelectedState(current.Identity.Id);

        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selected, current);

        SeekerCustomizationProfile profile = selected.SelectedProfile!;
        Character customized = application.CustomizedCharacter;
        Assert.Same(selected, application.SelectedState);
        Assert.Same(current, application.CurrentCharacter);
        Assert.NotSame(current, customized);
        Assert.Same(profile.Identity, customized.Identity);
        Assert.Same(profile.Background.PublicBackground, customized.PublicBackground);
        Assert.Same(current.PrivateBackground, customized.PrivateBackground);
        Assert.Same(current.Capabilities, customized.Capabilities);
        Assert.Same(current.Limitations, customized.Limitations);
        Assert.Same(current.ControllerAssignment, customized.ControllerAssignment);
        Assert.Same(current.Conditions, customized.Conditions);
        Assert.Same(current.Inventory, customized.Inventory);
        Assert.Same(current.Knowledge, customized.Knowledge);
        Assert.Same(current.Beliefs, customized.Beliefs);
        Assert.Same(current.Relationships, customized.Relationships);
        Assert.Same(current.Goals, customized.Goals);
        Assert.Same(current.Identity.Id, customized.Identity.Id);
        Assert.Same(current.Identity, application.CurrentCharacter.Identity);
    }

    [Fact]
    public void RejectsEmptyRebuiltWrongAndAlreadyProjectedInputs()
    {
        Character current = Character(new CharacterId("seeker"));
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState empty =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        SeekerCustomizationSelectionState rebuilt = SelectedState(
            new CharacterId(current.Identity.Id.Value));
        SeekerCustomizationSelectionState selected = SelectedState(current.Identity.Id);
        Character wrong = Character(new CharacterId("healer"));
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selected, current);

        Assert.Throws<InvalidOperationException>(() =>
            SeekerCustomizedCharacterProjection.Project(empty, current));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerCustomizedCharacterProjection.Project(rebuilt, current));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerCustomizedCharacterProjection.Project(selected, wrong));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerCustomizedCharacterProjection.Project(
                selected,
                application.CustomizedCharacter));
        Assert.Null(empty.SelectedProfile);
        Assert.Same(current.Identity, application.CurrentCharacter.Identity);
    }

    [Fact]
    public void RejectsMissingInputs()
    {
        Character current = Character(new CharacterId("seeker"));
        SeekerCustomizationSelectionState selected = SelectedState(current.Identity.Id);

        Assert.Throws<ArgumentNullException>(() =>
            SeekerCustomizedCharacterProjection.Project(null!, current));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerCustomizedCharacterProjection.Project(selected, null!));
    }

    private static SeekerCustomizationSelectionState SelectedState(CharacterId characterId)
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState initial =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        return SeekerCustomizationSelectionStateTransition.Select(
            initial,
            new CharacterIdentity(characterId, "Mara"),
            new SeekerPronouns("they", "them", "their", "theirs", "themself"),
            options.AppearanceChoices[1],
            options.BackgroundChoices[2]);
    }

    private static Character Character(CharacterId characterId) => new(
        new CharacterIdentity(characterId, "SEEKER"),
        new PublicCharacterBackground(string.Empty),
        new PrivateCharacterBackground("Private history."),
        new CharacterCapabilitySet([]),
        new CharacterLimitationSet([]),
        new CharacterControllerAssignment(characterId, ControllerType.Human),
        new CharacterConditionSet([]),
        new CharacterInventory([]),
        new CharacterKnowledge([new KnowledgeId("stone-marker-meaning")]),
        new CharacterBeliefSet([]),
        new CharacterRelationshipSet([]),
        new CharacterGoalSet([]));
}
