using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromDistinctIdentifierText()
    {
        Input input = CreateInput();
        CharacterId distinctCharacterId = CharacterId.Parse(input.Character.Identity.Id.Value);
        EvidenceId distinctEvidenceId = EvidenceId.Parse(input.Candidate.Evidence.EvidenceId.Value);

        CharacterObjectSabotageConcealmentDecision decision =
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{distinctCharacterId}}","selectedEvidenceId":"{{distinctEvidenceId}}"}""");

        Assert.NotSame(distinctCharacterId, decision.CharacterId);
        Assert.NotSame(distinctEvidenceId, decision.SelectedEvidenceId);
        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Same(input.Candidate.Evidence.EvidenceId, decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterObjectSabotageConcealmentDecision decision =
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{input.Character.Identity.Id}}","selectedEvidenceId":null}""");

        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Null(decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "disturbed-cask-cover")]
    [InlineData("enemy-agent", "unknown-evidence")]
    [InlineData("INVALID", "disturbed-cask-cover")]
    [InlineData("enemy-agent", "INVALID")]
    public void RejectsInvalidCharacterAndSelectionIdentifiersWithOneBoundedError(
        string characterId,
        string selectedEvidenceId)
    {
        Input input = CreateInput();
        string document =
            $$"""{"characterId":"{{characterId}}","selectedEvidenceId":"{{selectedEvidenceId}}"}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
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
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
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
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
                input.Request,
                null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength
                        + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterObjectSabotageConcealmentDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(
            deserializerType.GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageConcealmentDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageConcealmentConsiderationRequest), typeof(string)],
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
        Assert.Same(input.Concealment, input.Request.Concealment);
        Assert.Same(input.Evidence, input.Request.Evidence);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            concealment,
            evidence,
            [candidate]);

        return new Input(
            character,
            physicalAccess,
            worldObjects,
            concealment,
            evidence,
            candidate,
            request);
    }

    private sealed record Input(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}
