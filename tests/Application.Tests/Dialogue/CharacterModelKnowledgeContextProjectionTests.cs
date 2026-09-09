using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelKnowledgeContextProjectionTests
{
    [Fact]
    public void ProjectsOnlyReferencedKnowledgeInCharacterOrderWithExactText()
    {
        KnowledgeId firstReference = new("moonwort-use");
        KnowledgeId secondReference = new("stream-location");
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]),
            knowledge: new CharacterKnowledge([firstReference, secondReference]));
        string firstText = "  Moonwort must be cooled before use.  ";
        string secondText = "The stream runs east of the shrine.";
        KnowledgeEntry first = new(new KnowledgeId("moonwort-use"), firstText);
        KnowledgeEntry second = new(new KnowledgeId("stream-location"), secondText);
        KnowledgeEntry unrelated = new(new KnowledgeId("hidden-truth"), "Hidden truth.");
        KnowledgeSnapshot knowledge = new([unrelated, second, first]);

        CharacterModelContextEntry[] entries = CharacterModelKnowledgeContextProjection
            .Project(character, knowledge)
            .ToArray();

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RelevantKnowledge, entry.Kind);
                Assert.Same(firstText, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RelevantKnowledge, entry.Kind);
                Assert.Same(secondText, entry.Text);
            });
        Assert.Collection(
            character.Knowledge.KnowledgeReferences,
            entry => Assert.Same(firstReference, entry),
            entry => Assert.Same(secondReference, entry));
        Assert.Collection(
            knowledge.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(first, entry));
    }

    [Fact]
    public void AcceptsNoReferencesAndDoesNotExposeUnreferencedKnowledge()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]),
            knowledge: new CharacterKnowledge([]));
        KnowledgeEntry hidden = new(new KnowledgeId("hidden-truth"), "Hidden truth.");

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelKnowledgeContextProjection.Project(
                character,
                new KnowledgeSnapshot([hidden]));

        Assert.Empty(entries);
    }

    [Fact]
    public void RejectsMissingReferencedKnowledgeAndNullInputsWithoutChangingSources()
    {
        KnowledgeId reference = new("missing-knowledge");
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]),
            knowledge: new CharacterKnowledge([reference]));
        KnowledgeEntry retained = new(new KnowledgeId("retained"), "Retained knowledge.");
        KnowledgeSnapshot knowledge = new([retained]);

        Assert.Throws<ArgumentException>(() =>
            CharacterModelKnowledgeContextProjection.Project(character, knowledge));
        Assert.Same(reference, Assert.Single(character.Knowledge.KnowledgeReferences));
        Assert.Same(retained, Assert.Single(knowledge.Entries));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelKnowledgeContextProjection.Project(null!, knowledge));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelKnowledgeContextProjection.Project(character, null!));
    }
}
