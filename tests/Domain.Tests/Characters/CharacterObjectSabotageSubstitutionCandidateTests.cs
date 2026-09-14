using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionCandidateTests
{
    [Fact]
    public void RetainsExactSubstitutionAndEvidenceForTheSameOccurrence()
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence();
        CharacterObjectSabotageSubstitution substitution = new(occurrence, Replacement());
        CharacterObjectSabotageEvidence evidence = new(occurrence, Evidence());

        CharacterObjectSabotageSubstitutionCandidate candidate = new(substitution, evidence);

        Assert.Same(substitution, candidate.Substitution);
        Assert.Same(evidence, candidate.Evidence);
        Assert.Same(occurrence, candidate.Substitution.Occurrence);
        Assert.Same(occurrence, candidate.Evidence.Occurrence);
        Assert.Same(occurrence.Target, candidate.Substitution.Occurrence.Target);
        Assert.Same(substitution.Replacement, candidate.Substitution.Replacement);
        Assert.Same(evidence.Evidence, candidate.Evidence.Evidence);
    }

    [Fact]
    public void RejectsDistinctValueEqualOccurrenceAlignment()
    {
        CharacterObjectSabotageOccurrence exactOccurrence = Occurrence();
        CharacterObjectSabotageOccurrence valueEqualOccurrence = Occurrence();
        CharacterObjectSabotageSubstitution substitution = new(
            exactOccurrence,
            Replacement());
        CharacterObjectSabotageEvidence evidence = new(
            valueEqualOccurrence,
            Evidence());

        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageSubstitutionCandidate(substitution, evidence));
        Assert.NotSame(exactOccurrence, valueEqualOccurrence);
        Assert.Equal(exactOccurrence.Proposal.ActorId, valueEqualOccurrence.Proposal.ActorId);
        Assert.Equal(exactOccurrence.Target.Identity, valueEqualOccurrence.Target.Identity);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyClosedReadOnlyState()
    {
        CharacterObjectSabotageOccurrence occurrence = Occurrence();
        CharacterObjectSabotageSubstitution substitution = new(occurrence, Replacement());
        CharacterObjectSabotageEvidence evidence = new(occurrence, Evidence());

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionCandidate(null!, evidence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionCandidate(substitution, null!));

        Type candidateType = typeof(CharacterObjectSabotageSubstitutionCandidate);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Evidence", typeof(CharacterObjectSabotageEvidence)),
            ("Substitution", typeof(CharacterObjectSabotageSubstitution)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = candidateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = candidateType
            .GetMethods()
            .Where(method => method.DeclaringType == candidateType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(candidateType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(candidateType.IsSealed);
    }

    private static CharacterObjectSabotageOccurrence Occurrence()
    {
        ObjectId targetId = new("village-water-cask");

        return new CharacterObjectSabotageOccurrence(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    targetId,
                    "substitute-supplies",
                    new KnowledgeId("water-cask-contents")),
                CharacterSabotageActionKind.Substitute),
            new WorldObject(
                new WorldObjectIdentity(targetId, "Village water cask"),
                new WorldObjectDescription("A covered cask beside the village well.")));
    }

    private static WorldObject Replacement() =>
        new(
            new WorldObjectIdentity(new ObjectId("tainted-water-cask"), "Tainted water cask"),
            new WorldObjectDescription("A similar cask filled from a stagnant pool."));

    private static DiscoveryEvidence Evidence() =>
        new(
            new EvidenceId("mismatched-cask-hoops"),
            new ObjectId("village-water-cask"),
            VisibilityState.Noticed,
            false);
}
