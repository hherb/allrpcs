using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterObjectSabotageMoveDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromDistinctIdentifierText()
    {
        Input input = CreateInput();
        CharacterId distinctCharacterId = CharacterId.Parse(input.Character.Identity.Id.Value);
        EvidenceId distinctEvidenceId = EvidenceId.Parse(input.Candidate.Evidence.Evidence.EvidenceId.Value);

        CharacterObjectSabotageMoveDecision decision =
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{distinctCharacterId}}","selectedEvidenceId":"{{distinctEvidenceId}}"}""");

        Assert.NotSame(distinctCharacterId, decision.CharacterId);
        Assert.NotSame(distinctEvidenceId, decision.SelectedEvidenceId);
        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Same(input.Candidate.Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterObjectSabotageMoveDecision decision =
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{input.Character.Identity.Id}}","selectedEvidenceId":null}""");

        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Null(decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageMoveDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "cask-drag-marks")]
    [InlineData("enemy-agent", "unknown-evidence")]
    [InlineData("INVALID", "cask-drag-marks")]
    [InlineData("enemy-agent", "INVALID")]
    public void RejectsInvalidCharacterAndSelectionIdentifiersWithOneBoundedError(
        string characterId,
        string selectedEvidenceId)
    {
        Input input = CreateInput();
        string document =
            $$"""{"characterId":"{{characterId}}","selectedEvidenceId":"{{selectedEvidenceId}}"}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        """{"characterId":"enemy-agent","selectedEvidenceId":null} trailing""",
        """{/*comment*/"characterId":"enemy-agent","selectedEvidenceId":null}""",
        """{"characterId":"enemy-agent","selectedEvidenceId":null,"unknown":null}""",
        """{"characterId":"enemy-agent","characterId":"enemy-agent","selectedEvidenceId":null}""",
        """{"characterId":"enemy-agent"}""",
        """{"selectedEvidenceId":null}""",
        """{"characterId":5,"selectedEvidenceId":null}""",
        """{"characterId":"enemy-agent","selectedEvidenceId":5}""",
        """{"characterId":"enemy-agent","selectedEvidenceId":{}}""",
        """{"characterId":"enemy-agent","selectedEvidenceId":[[[[null]]]]}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedTrailingOverDepthUnknownDuplicateMissingAndNonStringInput(
        string document)
    {
        Input input = CreateInput();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        Input input = CreateInput();
        const string document =
            """{"characterId":"enemy-agent","selectedEvidenceId":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(input.Request, null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageMoveDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterObjectSabotageMoveDecisionJsonDeserializer.MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterObjectSabotageMoveDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(
            deserializerType.GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageMoveDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageMoveConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumDocumentLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(deserializerType.GetConstructors());
        Assert.Empty(deserializerType.GetProperties());
        Assert.Empty(deserializerType.GetEvents());
        Assert.True(deserializerType.IsAbstract && deserializerType.IsSealed);
    }

    private static void AssertRequestUnchanged(Input input)
    {
        Assert.Same(input.Character, input.Request.Character);
        Assert.Same(input.PhysicalAccess, input.Request.PhysicalAccess);
        Assert.Same(input.WorldObjects, input.Request.WorldObjects);
        Assert.Same(input.Placements, input.Request.Placements);
        Assert.Same(input.Evidence, input.Request.Evidence);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacementSnapshot placements = new(
            [new WorldObjectPlacement(target, source)]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            placements,
            evidence,
            [candidate]);

        return new Input(
            character,
            physicalAccess,
            worldObjects,
            placements,
            evidence,
            candidate,
            request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed record Input(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}
