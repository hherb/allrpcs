using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageSubstitutionRequestSerializerTests
{
    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        Input input = CreateInput();

        string serialized = OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.MaximumOutputTokens,
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
    public void SerializesOnlyTheBoundedTrustedCharacterAndEligibleSubstitutionContext()
    {
        Input input = CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(
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
            input.Character.Limitations.Entries.Select(limitation => limitation.Description),
            context.GetProperty("limitations").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            input.Character.Goals.Entries.Select(goal => goal.Description),
            context.GetProperty("goals").EnumerateArray().Select(value => value.GetString()));

        JsonElement candidate = Assert.Single(context.GetProperty("candidates").EnumerateArray());
        Assert.Equal(
            input.Candidate.Evidence.Evidence.EvidenceId.Value,
            candidate.GetProperty("evidenceId").GetString());
        Assert.Equal(input.Target.Identity.Id.Value, candidate.GetProperty("targetObjectId").GetString());
        Assert.Equal(
            input.Replacement.Identity.Id.Value,
            candidate.GetProperty("replacementObjectId").GetString());
        Assert.Equal(
            input.Replacement.Identity.DisplayName,
            candidate.GetProperty("replacementDisplayName").GetString());
        Assert.Equal(
            input.Replacement.Description.Text,
            candidate.GetProperty("replacementDescription").GetString());
        Assert.Equal(
            input.Candidate.Substitution.Occurrence.Proposal.AccessRequirement.RequiredCapabilityKey,
            candidate.GetProperty("requiredCapabilityKey").GetString());
        Assert.Equal(
            input.Candidate.Evidence.Evidence.IsCritical,
            candidate.GetProperty("isCriticalEvidence").GetBoolean());
        Assert.False(context.TryGetProperty("physicalAccess", out _));
        Assert.False(context.TryGetProperty("worldObjects", out _));
        Assert.False(context.TryGetProperty("substitutions", out _));
        Assert.False(context.TryGetProperty("evidence", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        Input input = CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        Assert.Equal(
            "character_object_sabotage_substitution_decision",
            format.GetProperty("name").GetString());
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
        CharacterObjectSabotageSubstitutionConsiderationRequest emptyRequest = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Substitutions,
            input.Evidence,
            []);
        using JsonDocument document = JsonDocument.Parse(
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(
                emptyRequest,
                "gpt-test-model"));

        JsonElement[] selectionValues = document.RootElement
            .GetProperty("text").GetProperty("format").GetProperty("schema")
            .GetProperty("properties").GetProperty("selectedEvidenceId")
            .GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(selectionValues);
        Assert.Equal(JsonValueKind.Null, selectionValues[0].ValueKind);
    }

    [Fact]
    public void RejectsNullsAndInvalidOrExcessiveModelNames()
    {
        CharacterObjectSabotageSubstitutionConsiderationRequest request = CreateInput().Request;

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(null!, "gpt-test"));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(request, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(request, " model "));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.Serialize(
                request,
                new string(
                    'm',
                    OpenAiCharacterObjectSabotageSubstitutionRequestSerializer.MaximumModelNameLength
                        + 1)));
    }

    [Fact]
    public void ExposesOnlyOneSerializerAndItsTwoBounds()
    {
        Type serializerType = typeof(OpenAiCharacterObjectSabotageSubstitutionRequestSerializer);
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
            [typeof(CharacterObjectSabotageSubstitutionConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["MaximumModelNameLength", "MaximumOutputTokens"],
            fields.Select(field => field.Name).Order());
        Assert.All(fields, field => Assert.True(field.IsLiteral));
        Assert.True(serializerType.IsAbstract && serializerType.IsSealed);
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
        WorldObject replacement = WorldObject("tainted-water-cask", "Tainted water cask");
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
            new CharacterObjectSabotageSubstitution(occurrence, replacement),
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
            target,
            replacement,
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
        WorldObject Target,
        WorldObject Replacement,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
