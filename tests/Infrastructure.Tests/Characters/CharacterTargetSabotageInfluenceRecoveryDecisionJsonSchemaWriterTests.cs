using System.Buffers;
using System.Reflection;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryDecisionJsonSchemaWriterTests
{
    [Fact]
    public void WritesOneClosedRequestBoundSchemaAtTheMaximumCandidateCount()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create(
                CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount);

        using JsonDocument document = WriteSchema(input.Request);
        JsonElement root = document.RootElement;
        JsonElement properties = root.GetProperty("properties");
        JsonElement actorId = properties.GetProperty("actorId");
        JsonElement selectedCandidateIndex = properties.GetProperty("selectedCandidateIndex");

        Assert.Equal("object", root.GetProperty("type").GetString());
        Assert.False(root.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["actorId", "selectedCandidateIndex"],
            root.GetProperty("required").EnumerateArray().Select(item => item.GetString()));
        Assert.Equal(["actorId", "selectedCandidateIndex"], EnumeratePropertyNames(properties));
        Assert.Equal("string", actorId.GetProperty("type").GetString());
        Assert.Equal(
            [input.Actor.Identity.Id.Value],
            actorId.GetProperty("enum").EnumerateArray().Select(item => item.GetString()));
        Assert.Equal(
            ["integer", "null"],
            selectedCandidateIndex.GetProperty("type")
                .EnumerateArray()
                .Select(item => item.GetString()));

        JsonElement[] allowedSelections = selectedCandidateIndex.GetProperty("enum")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(
            Enumerable.Range(
                0,
                CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount),
            allowedSelections[..^1].Select(item => item.GetInt32()));
        Assert.Equal(JsonValueKind.Null, allowedSelections[^1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullWriterAndRequestWithoutWritingProviderOrTransportState()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        Type writerType = GetWriterType();
        MethodInfo method = GetWriteMethod(writerType);
        ArrayBufferWriter<byte> buffer = new();
        using Utf8JsonWriter writer = new(buffer);

        TargetInvocationException nullWriter = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, [null, input.Request]));
        TargetInvocationException nullRequest = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, [writer, null]));

        Assert.IsType<ArgumentNullException>(nullWriter.InnerException);
        Assert.IsType<ArgumentNullException>(nullRequest.InnerException);
        Assert.Equal(0, buffer.WrittenCount);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RemainsAnInternalStaticWriterWithOneInternalEntryPoint()
    {
        Type writerType = GetWriterType();
        MethodInfo method = GetWriteMethod(writerType);

        Assert.False(writerType.IsPublic);
        Assert.True(writerType.IsAbstract && writerType.IsSealed);
        Assert.True(method.IsAssembly);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(void), method.ReturnType);
        Assert.Equal(
            [typeof(Utf8JsonWriter), typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(writerType.GetConstructors());
        Assert.Empty(writerType.GetProperties());
        Assert.Empty(writerType.GetEvents());
    }

    private static JsonDocument WriteSchema(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            GetWriteMethod(GetWriterType()).Invoke(null, [writer, request]);
        }

        return JsonDocument.Parse(buffer.WrittenMemory);
    }

    private static Type GetWriterType() =>
        typeof(CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer)
            .Assembly
            .GetType(
                "Allrpcs.Infrastructure.Characters."
                    + "CharacterTargetSabotageInfluenceRecoveryDecisionJsonSchemaWriter",
                throwOnError: true)!;

    private static MethodInfo GetWriteMethod(Type writerType) =>
        Assert.Single(writerType.GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name == "Write");

    private static string?[] EnumeratePropertyNames(JsonElement element) =>
        element.EnumerateObject().Select(property => property.Name).ToArray();

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.Same(input.InfluenceChange.Reason, input.Relationship.Reasons.Single());
    }
}
