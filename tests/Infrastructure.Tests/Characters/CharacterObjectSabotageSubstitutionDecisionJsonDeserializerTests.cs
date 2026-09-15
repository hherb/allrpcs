using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromDistinctIdentifierText()
    {
        Input input = CreateInput();
        CharacterId distinctCharacterId = CharacterId.Parse(input.Character.Identity.Id.Value);
        EvidenceId distinctEvidenceId = EvidenceId.Parse(
            input.Candidate.Evidence.Evidence.EvidenceId.Value);

        CharacterObjectSabotageSubstitutionDecision decision =
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{distinctCharacterId}}","selectedEvidenceId":"{{distinctEvidenceId}}"}""");

        Assert.NotSame(distinctCharacterId, decision.CharacterId);
        Assert.NotSame(distinctEvidenceId, decision.SelectedEvidenceId);
        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Same(input.Candidate.Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterObjectSabotageSubstitutionDecision decision =
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"characterId":"{{input.Character.Identity.Id}}","selectedEvidenceId":null}""");

        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Null(decision.SelectedEvidenceId);
        Assert.True(CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "water-cask-substitution-signs")]
    [InlineData("enemy-agent", "unknown-evidence")]
    [InlineData("INVALID", "water-cask-substitution-signs")]
    [InlineData("enemy-agent", "INVALID")]
    public void RejectsInvalidCharacterAndSelectionIdentifiersWithOneBoundedError(
        string characterId,
        string selectedEvidenceId)
    {
        Input input = CreateInput();
        string document =
            $$"""{"characterId":"{{characterId}}","selectedEvidenceId":"{{selectedEvidenceId}}"}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
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
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
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
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
                input.Request,
                null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterObjectSabotageSubstitutionDecisionJsonDeserializer.MaximumDocumentLength
                        + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterObjectSabotageSubstitutionDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(
            deserializerType.GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterObjectSabotageSubstitutionDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageSubstitutionConsiderationRequest), typeof(string)],
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
        Assert.Same(input.Substitutions, input.Request.Substitutions);
        Assert.Same(input.Evidence, input.Request.Evidence);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask", "Water cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(
                occurrence,
                WorldObject("tainted-water-cask", "Tainted water cask")),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            substitutions,
            evidence,
            [candidate]);

        return new Input(
            character,
            physicalAccess,
            worldObjects,
            substitutions,
            evidence,
            candidate,
            request);
    }

    private static WorldObject WorldObject(string id, string name) =>
        new(new WorldObjectIdentity(new ObjectId(id), name), new WorldObjectDescription(name));

    private sealed record Input(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
