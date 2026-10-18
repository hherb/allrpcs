using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationOpenAiRequestTests
{
    internal static CharacterObjectSabotageContaminationConsiderationRequest CreateRequest() =>
        CreateInput().Request;

    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        Input input = CreateInput();

        string serialized = Serialize(
            input.Request,
            "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            CharacterStructuredDecisions.ObjectSabotageContamination.MaximumOutputTokens,
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
    public void SerializesOnlyBoundedTrustedContextInExactCandidateOrder()
    {
        Input input = CreateInput();
        using JsonDocument requestDocument = JsonDocument.Parse(
            Serialize(
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
            input.Character.Limitations.Entries.Select(limitation => limitation.Description),
            context.GetProperty("limitations").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            input.Character.Goals.Entries.Select(goal => goal.Description),
            context.GetProperty("goals").EnumerateArray().Select(value => value.GetString()));

        JsonElement[] candidates = context.GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(input.Candidates.Count, candidates.Length);
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.Evidence.EvidenceId.Value),
            candidates.Select(candidate => candidate.GetProperty("evidenceId").GetString()));
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.Occurrence.Target.Identity.Id.Value),
            candidates.Select(candidate => candidate.GetProperty("targetObjectId").GetString()));
        Assert.All(candidates, candidate =>
            Assert.Equal("contaminate", candidate.GetProperty("actionKind").GetString()));
        Assert.Equal(
            input.Candidates.Select(candidate =>
                candidate.Occurrence.Proposal.AccessRequirement.RequiredCapabilityKey),
            candidates.Select(candidate =>
                candidate.GetProperty("requiredCapabilityKey").GetString()));
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.Evidence.IsCritical),
            candidates.Select(candidate => candidate.GetProperty("isCriticalEvidence").GetBoolean()));
        Assert.False(context.TryGetProperty("physicalAccess", out _));
        Assert.False(context.TryGetProperty("worldObjects", out _));
        Assert.False(context.TryGetProperty("contamination", out _));
        Assert.False(context.TryGetProperty("evidence", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaInCandidateOrderIncludingAbstention()
    {
        Input input = CreateInput();
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        Assert.Equal(
            "character_object_sabotage_contamination_decision",
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
        Assert.Equal(input.Candidates.Count + 1, selectionValues.Length);
        Assert.Equal(
            input.Candidates.Select(candidate => candidate.Evidence.EvidenceId.Value),
            selectionValues[..^1].Select(value => value.GetString()));
        Assert.Equal(JsonValueKind.Null, selectionValues[^1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SupportsAnExactAbstentionOnlySchemaForAnEmptyCandidateRequest()
    {
        Input input = CreateInput();
        CharacterObjectSabotageContaminationConsiderationRequest emptyRequest = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.Evidence,
            []);
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
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
        CharacterObjectSabotageContaminationConsiderationRequest request = CreateInput().Request;

        Assert.Throws<ArgumentNullException>(() =>
            Serialize(
                null!,
                "gpt-test-model"));
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            Serialize(request, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            Serialize(request, " model "));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                "model\0name"));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                request,
                new string(
                    'm',
                    Allrpcs.Application.Dialogue.CharacterModelSettings.MaximumModelNameLength
                        + 1)));
    }



    private static void AssertRequestUnchanged(Input input)
    {
        Assert.Same(input.Character, input.Request.Character);
        Assert.Same(input.PhysicalAccess, input.Request.PhysicalAccess);
        Assert.Same(input.WorldObjects, input.Request.WorldObjects);
        Assert.Same(input.Contamination, input.Request.Contamination);
        Assert.Same(input.Evidence, input.Request.Evidence);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.True(input.Candidates.Zip(
            input.Request.Candidates,
            ReferenceEquals).All(same => same));
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject firstTarget = WorldObject("village-water-cask", "Water cask");
        WorldObject secondTarget = WorldObject("healer-workbench", "Healer workbench");
        CharacterObjectSabotageEvidence firstCandidate = Candidate(
            character,
            firstTarget,
            "water-cask-contamination-signs",
            false);
        CharacterObjectSabotageEvidence secondCandidate = Candidate(
            character,
            secondTarget,
            "workbench-contamination-signs",
            true);
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [firstTarget.Identity.Id, secondTarget.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([firstTarget, secondTarget]);
        WorldObjectContaminationSnapshot contamination = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        IReadOnlyList<CharacterObjectSabotageEvidence> candidates =
            [firstCandidate, secondCandidate];
        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            candidates);

        return new Input(
            character,
            physicalAccess,
            worldObjects,
            contamination,
            evidence,
            candidates,
            request);
    }

    private static CharacterObjectSabotageEvidence Candidate(
        Character character,
        WorldObject target,
        string evidenceId,
        bool isCritical)
    {
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        return new CharacterObjectSabotageEvidence(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId(evidenceId),
                target.Identity.Id,
                VisibilityState.Noticed,
                isCritical));
    }

    private static WorldObject WorldObject(string id, string name) =>
        new(new WorldObjectIdentity(new ObjectId(id), name), new WorldObjectDescription(name));

    private sealed record Input(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        IReadOnlyList<CharacterObjectSabotageEvidence> Candidates,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
    private static string Serialize(CharacterObjectSabotageContaminationConsiderationRequest request, string modelName) =>
        CharacterStructuredDecisions.ObjectSabotageContamination.Serialize(request, Allrpcs.Application.Dialogue.CharacterModelProvider.OpenAi, modelName);

}
