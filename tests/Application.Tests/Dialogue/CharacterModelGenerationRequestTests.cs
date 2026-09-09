using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationRequestTests
{
    [Fact]
    public void RetainsTheExactProviderNeutralTurnRequest()
    {
        CharacterTurnRequest turnRequest = CreateTurnRequest();
        CharacterModelContextEntry dialogueEntry = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            turnRequest.DialogueEvent.Utterance.Text);
        CharacterModelContextSnapshot context = new([dialogueEntry]);

        CharacterModelGenerationRequest request = new(turnRequest, context);

        Assert.Same(turnRequest, request.TurnRequest);
        Assert.Same(context, request.Context);
        Assert.Same(dialogueEntry, Assert.Single(request.Context.Entries));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnRequest turnRequest = CreateTurnRequest();
        CharacterModelContextSnapshot context = ContextFor(turnRequest);

        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationRequest(
            null!,
            context));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationRequest(
            turnRequest,
            null!));
    }

    [Fact]
    public void RequiresOneExactUntrustedPlayerDialogueEntry()
    {
        CharacterTurnRequest turnRequest = CreateTurnRequest();
        CharacterModelContextEntry exactEntry = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            turnRequest.DialogueEvent.Utterance.Text);
        string distinctEqualText = new(turnRequest.DialogueEvent.Utterance.Text.ToCharArray());
        CharacterModelContextEntry distinctEntry = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            distinctEqualText);
        CharacterModelContextEntry changedEntry = new(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            "Changed player dialogue.");

        Assert.Throws<ArgumentException>(() => new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot([])));
        Assert.Throws<ArgumentException>(() => new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot([exactEntry, exactEntry])));
        Assert.Throws<ArgumentException>(() => new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot([distinctEntry])));
        Assert.Throws<ArgumentException>(() => new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot([changedEntry])));
    }

    [Fact]
    public void ExposesOnlyTheImmutableTurnRequest()
    {
        Type requestType = typeof(CharacterModelGenerationRequest);

        Assert.True(requestType.IsSealed);
        Assert.Single(requestType.GetConstructors());
        Assert.Collection(
            requestType.GetProperties().OrderBy(property => property.Name),
            property => AssertGetOnlyProperty(
                property,
                nameof(CharacterModelGenerationRequest.Context),
                typeof(CharacterModelContextSnapshot)),
            property => AssertGetOnlyProperty(
                property,
                nameof(CharacterModelGenerationRequest.TurnRequest),
                typeof(CharacterTurnRequest)));
    }

    private static CharacterModelContextSnapshot ContextFor(CharacterTurnRequest turnRequest) => new(
        [new CharacterModelContextEntry(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            turnRequest.DialogueEvent.Utterance.Text)]);

    private static void AssertGetOnlyProperty(
        System.Reflection.PropertyInfo property,
        string name,
        Type propertyType)
    {
        Assert.Equal(name, property.Name);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.NotNull(property.GetMethod);
        Assert.Null(property.SetMethod);
    }

    private static CharacterTurnRequest CreateTurnRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Can you help?");

        return new CharacterTurnRequest(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
