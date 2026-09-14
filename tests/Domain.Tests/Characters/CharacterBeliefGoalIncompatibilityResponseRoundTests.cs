using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityResponseRoundTests
{
    [Fact]
    public void RetainsExactlyTwoPerspectivesAndResponsesInStableDeclarationOrder()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration);
        CharacterBeliefGoalIncompatibilityResponse[] responses = CreateResponses(perspectives);
        CharacterBelief[] firstBeliefs = [.. declaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] firstGoals = [.. declaration.FirstCharacter.Goals.Entries];
        CharacterBelief[] secondBeliefs = [.. declaration.SecondCharacter.Beliefs.Entries];
        CharacterGoal[] secondGoals = [.. declaration.SecondCharacter.Goals.Entries];

        CharacterBeliefGoalIncompatibilityResponseRound round = new(perspectives, responses);

        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterBeliefGoalIncompatibilityPerspective>>(round.Perspectives);
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterBeliefGoalIncompatibilityResponse>>(round.Responses);
        Assert.Collection(
            round.Perspectives,
            perspective => Assert.Same(perspectives[0], perspective),
            perspective => Assert.Same(perspectives[1], perspective));
        Assert.Collection(
            round.Responses,
            response => Assert.Same(responses[0], response),
            response => Assert.Same(responses[1], response));
        Assert.Same(declaration.FirstCharacter, round.Perspectives[0].SubjectCharacter);
        Assert.Same(declaration.SecondCharacter, round.Perspectives[1].SubjectCharacter);
        Assert.Same(round.Perspectives[0], round.Responses[0].Perspective);
        Assert.Same(round.Perspectives[1], round.Responses[1].Perspective);
        Assert.Equal(firstBeliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(firstGoals, declaration.FirstCharacter.Goals.Entries);
        Assert.Equal(secondBeliefs, declaration.SecondCharacter.Beliefs.Entries);
        Assert.Equal(secondGoals, declaration.SecondCharacter.Goals.Entries);
    }

    [Fact]
    public void CopiesCallerCollectionsAndFreshFixturesRemainIndependent()
    {
        CharacterBeliefGoalIncompatibility firstDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective[] firstPerspectives =
            [.. CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(firstDeclaration)];
        CharacterBeliefGoalIncompatibilityResponse[] firstResponses = CreateResponses(firstPerspectives);
        CharacterBeliefGoalIncompatibilityResponseRound first = new(
            firstPerspectives,
            firstResponses);
        CharacterBeliefGoalIncompatibility secondDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> secondPerspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(secondDeclaration);
        CharacterBeliefGoalIncompatibilityResponseRound second = new(
            secondPerspectives,
            CreateResponses(secondPerspectives));

        firstPerspectives[0] = firstPerspectives[1];
        firstResponses[0] = firstResponses[1];

        Assert.Same(firstDeclaration.FirstCharacter, first.Perspectives[0].SubjectCharacter);
        Assert.Same(firstDeclaration.SecondCharacter, first.Perspectives[1].SubjectCharacter);
        Assert.Same(first.Perspectives[0], first.Responses[0].Perspective);
        Assert.Same(first.Perspectives[1], first.Responses[1].Perspective);
        Assert.NotSame(first.Perspectives[0], second.Perspectives[0]);
        Assert.NotSame(first.Responses[0], second.Responses[0]);
        Assert.NotSame(firstDeclaration.FirstCharacter, secondDeclaration.FirstCharacter);
    }

    [Fact]
    public void RejectsNullCollectionsEntriesAndIncompleteRounds()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective[] perspectives =
            [.. CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)];
        CharacterBeliefGoalIncompatibilityResponse[] responses = CreateResponses(perspectives);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(null!, responses));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(perspectives, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                new CharacterBeliefGoalIncompatibilityPerspective[] { perspectives[0], null! },
                responses));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                perspectives,
                new CharacterBeliefGoalIncompatibilityResponse[] { responses[0], null! }));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound([perspectives[0]], responses));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                perspectives,
                [responses[0]]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                [perspectives[0], perspectives[1], perspectives[0]],
                responses));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                perspectives,
                [responses[0], responses[1], responses[0]]));
    }

    [Fact]
    public void RejectsDuplicateReversedAndNonExactPerspectiveAlignment()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective[] perspectives =
            [.. CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)];
        CharacterBeliefGoalIncompatibilityResponse[] responses = CreateResponses(perspectives);
        CharacterBeliefGoalIncompatibilityPerspective replacementFirst = new(
            declaration,
            declaration.FirstCharacter);

        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                [perspectives[0], perspectives[0]],
                responses));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                [perspectives[1], perspectives[0]],
                [responses[1], responses[0]]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                perspectives,
                [responses[0], responses[0]]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                perspectives,
                [
                    new CharacterBeliefGoalIncompatibilityResponse(
                        replacementFirst,
                        responses[0].Kind,
                        responses[0].Explanation),
                    responses[1],
                ]));
    }

    [Fact]
    public void RejectsPerspectivesFromDifferentAndValueEqualReplacementDeclarations()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective[] perspectives =
            [.. CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)];
        CharacterBeliefGoalIncompatibilityResponse[] responses = CreateResponses(perspectives);
        CharacterBeliefGoalIncompatibility otherDeclaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective otherSecond = new(
            otherDeclaration,
            otherDeclaration.SecondCharacter);
        CharacterBeliefGoalIncompatibility replacementDeclaration = new(
            declaration.FirstCharacter,
            declaration.FirstBelief,
            declaration.FirstGoal,
            declaration.SecondCharacter,
            declaration.SecondBelief,
            declaration.SecondGoal,
            declaration.Rationale);
        CharacterBeliefGoalIncompatibilityPerspective replacementSecond = new(
            replacementDeclaration,
            replacementDeclaration.SecondCharacter);

        Assert.NotSame(declaration, replacementDeclaration);
        Assert.Equal(declaration, replacementDeclaration);
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                [perspectives[0], otherSecond],
                [responses[0], CreateResponse(otherSecond, false)]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityResponseRound(
                [perspectives[0], replacementSecond],
                [responses[0], CreateResponse(replacementSecond, false)]));
    }

    [Fact]
    public void ExposesOnlyClosedImmutableRoundState()
    {
        Type roundType = typeof(CharacterBeliefGoalIncompatibilityResponseRound);
        (string Name, Type Type, bool CanWrite)[] properties = roundType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                ("Perspectives", typeof(IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective>)),
                ("Responses", typeof(IReadOnlyList<CharacterBeliefGoalIncompatibilityResponse>)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(roundType.GetEvents());
        Assert.True(roundType.IsSealed);
    }

    private static CharacterBeliefGoalIncompatibilityResponse[] CreateResponses(
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives) =>
        [
            CreateResponse(perspectives[0], true),
            CreateResponse(perspectives[1], false),
        ];

    private static CharacterBeliefGoalIncompatibilityResponse CreateResponse(
        CharacterBeliefGoalIncompatibilityPerspective perspective,
        bool isFirst) => new(
            perspective,
            isFirst
                ? CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation
                : CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderGoal,
            isFirst
                ? "Compare the evidence and seek a jointly supportable path."
                : "Protect civilians by reconsidering the delay objective.");
}
