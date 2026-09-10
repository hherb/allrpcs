using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterCommitmentMemoryTests
{
    [Fact]
    public void KindVocabularyIsClosedCanonicalAndStable()
    {
        Assert.Collection(
            CharacterCommitmentKind.All,
            kind => Assert.Same(CharacterCommitmentKind.Promise, kind),
            kind => Assert.Same(CharacterCommitmentKind.AssignedTask, kind),
            kind => Assert.Same(CharacterCommitmentKind.Other, kind));
        Assert.Same(CharacterCommitmentKind.Promise, CharacterCommitmentKind.Parse("promise"));
        Assert.Same(
            CharacterCommitmentKind.AssignedTask,
            CharacterCommitmentKind.Parse("assigned-task"));
        Assert.Same(CharacterCommitmentKind.Other, CharacterCommitmentKind.Parse("other"));
        Assert.Throws<ArgumentException>(() => CharacterCommitmentKind.Parse("complete"));
        Assert.Throws<ArgumentNullException>(() => CharacterCommitmentKind.Parse(null!));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterCommitmentKind>)CharacterCommitmentKind.All)
                .Add(CharacterCommitmentKind.Promise));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void RetainsExactInputsForEveryKind(CharacterCommitmentKind kind)
    {
        CharacterCommitmentId commitmentId = new("examine-herbs");
        CharacterId characterId = new("healer");
        CharacterId counterpartyId = new("seeker");
        CharacterTurnId sourceTurnId = new("healer-turn-3");
        const string Description = "  Examine the moonwort before collecting it.\r\n";

        CharacterCommitmentMemory commitment = new(
            commitmentId,
            characterId,
            kind,
            counterpartyId,
            sourceTurnId,
            Description);

        Assert.Same(commitmentId, commitment.CommitmentId);
        Assert.Same(characterId, commitment.CharacterId);
        Assert.Same(kind, commitment.Kind);
        Assert.Same(counterpartyId, commitment.CounterpartyCharacterId);
        Assert.Same(sourceTurnId, commitment.SourceTurnId);
        Assert.Equal(Description, commitment.Description);
    }

    [Fact]
    public void AcceptsExactTextBoundAndRejectsInvalidShape()
    {
        string maximum = new('c', CharacterCommitmentMemory.MaximumDescriptionLength);
        CharacterCommitmentMemory commitment = Commitment(description: maximum);

        Assert.Equal(maximum, commitment.Description);
        Assert.Throws<ArgumentException>(() => Commitment(description: " "));
        Assert.Throws<ArgumentException>(() => Commitment(
            description: new string('c', CharacterCommitmentMemory.MaximumDescriptionLength + 1)));
        Assert.Throws<ArgumentException>(() => Commitment(
            characterId: new CharacterId("healer"),
            counterpartyId: new CharacterId("healer")));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterCommitmentId commitmentId = new("examine-herbs");
        CharacterId characterId = new("healer");
        CharacterId counterpartyId = new("seeker");
        CharacterTurnId sourceTurnId = new("healer-turn-3");

        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            null!, characterId, CharacterCommitmentKind.Promise,
            counterpartyId, sourceTurnId, "Commitment."));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            commitmentId, null!, CharacterCommitmentKind.Promise,
            counterpartyId, sourceTurnId, "Commitment."));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            commitmentId, characterId, null!, counterpartyId, sourceTurnId, "Commitment."));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            commitmentId, characterId, CharacterCommitmentKind.Promise,
            null!, sourceTurnId, "Commitment."));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            commitmentId, characterId, CharacterCommitmentKind.Promise,
            counterpartyId, null!, "Commitment."));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemory(
            commitmentId, characterId, CharacterCommitmentKind.Promise,
            counterpartyId, sourceTurnId, null!));
    }

    public static TheoryData<CharacterCommitmentKind> Kinds =>
        new()
        {
            CharacterCommitmentKind.Promise,
            CharacterCommitmentKind.AssignedTask,
            CharacterCommitmentKind.Other,
        };

    internal static CharacterCommitmentMemory Commitment(
        CharacterId? characterId = null,
        CharacterId? counterpartyId = null,
        string commitmentId = "examine-herbs",
        string description = "Examine the moonwort.") =>
        new(
            new CharacterCommitmentId(commitmentId),
            characterId ?? new CharacterId("healer"),
            CharacterCommitmentKind.Promise,
            counterpartyId ?? new CharacterId("seeker"),
            new CharacterTurnId("healer-turn-3"),
            description);
}
