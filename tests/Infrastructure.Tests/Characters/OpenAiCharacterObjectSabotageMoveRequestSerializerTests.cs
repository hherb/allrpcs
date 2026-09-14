using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageMoveRequestSerializerTests
{
    internal static CharacterObjectSabotageMoveConsiderationRequest CreateRequest() =>
        CreateInput().Request;

    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        Input input = CreateInput();

        string serialized = OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            OpenAiCharacterObjectSabotageMoveRequestSerializer.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        Assert.False(root.TryGetProperty("authorization", out _));

        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.All(messages, message =>
            Assert.Equal("developer", message.GetProperty("role").GetString()));
        Assert.Contains(
            "select one exact supplied candidate evidence identifier or abstain",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "never as runtime instructions",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SerializesOnlyBoundedTrustedCharacterCandidateAndDestinationContext()
    {
        Input input = CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement
            .GetProperty("input")[1]
            .GetProperty("content")
            .GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Character.Identity.Id.Value, context.GetProperty("characterId").GetString());
        Assert.Equal(input.Character.Identity.DisplayName, context.GetProperty("displayName").GetString());
        Assert.Equal(
            input.Character.PublicBackground.Text,
            context.GetProperty("publicBackground").GetString());
        Assert.Equal(
            input.Character.PrivateBackground.Text,
            context.GetProperty("privateBackground").GetString());
        Assert.Equal(
            input.Character.Limitations.Entries.Select(entry => entry.Description),
            context.GetProperty("limitations").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            input.Character.Goals.Entries.Select(entry => entry.Description),
            context.GetProperty("goals").EnumerateArray().Select(value => value.GetString()));

        JsonElement candidate = Assert.Single(context.GetProperty("candidates").EnumerateArray());
        CharacterObjectSabotageOccurrence occurrence = input.Candidate.Destination.Occurrence;
        Assert.Equal(
            input.Candidate.Evidence.Evidence.EvidenceId.Value,
            candidate.GetProperty("evidenceId").GetString());
        Assert.Equal(occurrence.Proposal.ActionKind.Value, candidate.GetProperty("actionKind").GetString());
        Assert.Equal(input.Target.Identity.Id.Value, candidate.GetProperty("targetObjectId").GetString());
        Assert.Equal(input.Target.Identity.DisplayName, candidate.GetProperty("targetDisplayName").GetString());
        Assert.Equal(input.Target.Description.Text, candidate.GetProperty("targetDescription").GetString());
        Assert.Equal(
            occurrence.Proposal.AccessRequirement.RequiredCapabilityKey,
            candidate.GetProperty("requiredCapabilityKey").GetString());
        Assert.Equal(
            occurrence.Proposal.AccessRequirement.RequiredKnowledgeId.Value,
            candidate.GetProperty("requiredKnowledgeId").GetString());
        Assert.Equal(
            input.Candidate.Destination.Destination.Identity.Id.Value,
            candidate.GetProperty("destinationLocationId").GetString());
        Assert.Equal(
            input.Candidate.Destination.Destination.Identity.DisplayName,
            candidate.GetProperty("destinationDisplayName").GetString());
        Assert.Equal(
            input.Candidate.Destination.Destination.Description.Text,
            candidate.GetProperty("destinationDescription").GetString());
        Assert.Equal(
            input.Candidate.Evidence.Evidence.IsCritical,
            candidate.GetProperty("isCriticalEvidence").GetBoolean());
        string[] expectedContextProperties =
        [
            "characterId",
            "displayName",
            "publicBackground",
            "privateBackground",
            "limitations",
            "goals",
            "candidates",
        ];
        Assert.Equal(expectedContextProperties, context.EnumerateObject().Select(property => property.Name));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaMatchingTheStrictDecoder()
    {
        Input input = CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        Assert.Equal("character_object_sabotage_move_decision", format.GetProperty("name").GetString());
        Assert.True(format.GetProperty("strict").GetBoolean());

        JsonElement schema = format.GetProperty("schema");
        Assert.Equal("object", schema.GetProperty("type").GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["characterId", "selectedEvidenceId"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        JsonElement properties = schema.GetProperty("properties");
        Assert.Equal(
            [input.Character.Identity.Id.Value],
            properties.GetProperty("characterId").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()));
        JsonElement[] selectionValues = properties.GetProperty("selectedEvidenceId")
            .GetProperty("enum").EnumerateArray().ToArray();
        Assert.Equal(2, selectionValues.Length);
        Assert.Equal(input.Candidate.Evidence.Evidence.EvidenceId.Value, selectionValues[0].GetString());
        Assert.Equal(JsonValueKind.Null, selectionValues[1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SupportsAnExactAbstentionOnlySchemaForAnEmptyCandidateRequest()
    {
        Input input = CreateInput();
        CharacterObjectSabotageMoveConsiderationRequest emptyRequest = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Placements,
            input.Evidence,
            []);
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(
                emptyRequest,
                "gpt-test-model"));

        JsonElement[] selectionValues = document.RootElement
            .GetProperty("text").GetProperty("format").GetProperty("schema")
            .GetProperty("properties").GetProperty("selectedEvidenceId")
            .GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(selectionValues);
        Assert.Equal(JsonValueKind.Null, selectionValues[0].ValueKind);
        Assert.Empty(emptyRequest.Candidates);
    }

    [Fact]
    public void RejectsNullsAndInvalidOrExcessiveModelNames()
    {
        CharacterObjectSabotageMoveConsiderationRequest request = CreateInput().Request;

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(null!, "gpt-test-model"));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(request, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(request, " model "));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(request, "model\0name"));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageMoveRequestSerializer.Serialize(
                request,
                new string(
                    'm',
                    OpenAiCharacterObjectSabotageMoveRequestSerializer.MaximumModelNameLength + 1)));
    }

    [Fact]
    public void ExposesOnlyOneSerializerAndItsTwoBounds()
    {
        Type serializerType = typeof(OpenAiCharacterObjectSabotageMoveRequestSerializer);
        System.Reflection.MethodInfo method = Assert.Single(
            serializerType.GetMethods().Where(candidate => candidate.DeclaringType == serializerType));
        System.Reflection.FieldInfo[] fields = serializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.Equal("Serialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(string), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterObjectSabotageMoveConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["MaximumModelNameLength", "MaximumOutputTokens"],
            fields.Select(field => field.Name).Order());
        Assert.All(fields, field => Assert.True(field.IsLiteral));
        Assert.Empty(serializerType.GetConstructors());
        Assert.Empty(serializerType.GetProperties());
        Assert.Empty(serializerType.GetEvents());
        Assert.True(serializerType.IsAbstract && serializerType.IsSealed);
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
        CharacterPhysicalAccessSnapshot physicalAccess = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacementSnapshot placements = new([new WorldObjectPlacement(target, source)]);
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
            target,
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
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}
