using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterSabotageActionDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromValueMatchingActorAndKindText()
    {
        Input input = CreateInput();
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);
        CharacterSabotageActionKind distinctKind = CharacterSabotageActionKind.Parse(
            input.SelectedKind.Value);

        CharacterSabotageActionDecision decision =
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedActionKind":"{{distinctKind}}"}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(input.SelectedKind, decision.SelectedActionKind);
        Assert.True(CharacterSabotageActionDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterSabotageActionDecision decision =
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedActionKind":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedActionKind);
        Assert.True(CharacterSabotageActionDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "misquote")]
    [InlineData("enemy-agent", "delay")]
    [InlineData("INVALID", "misquote")]
    [InlineData("enemy-agent", "INVALID")]
    public void RejectsInvalidActorAndSelectionValuesWithOneBoundedError(
        string actorId,
        string selectedActionKind)
    {
        Input input = CreateInput();
        string document =
            $$"""{"actorId":"{{actorId}}","selectedActionKind":"{{selectedActionKind}}"}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(input.Request, document));

        Assert.Equal("serialized", exception.ParamName);
        Assert.DoesNotContain(actorId, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(selectedActionKind, exception.Message, StringComparison.Ordinal);
        AssertRequestUnchanged(input);
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        """{"actorId":"enemy-agent","selectedActionKind":null} trailing""",
        """{/*comment*/"actorId":"enemy-agent","selectedActionKind":null}""",
        """{"actorId":"enemy-agent","selectedActionKind":null,"unknown":null}""",
        """{"actorId":"enemy-agent","actorId":"enemy-agent","selectedActionKind":null}""",
        """{"actorId":"enemy-agent"}""",
        """{"selectedActionKind":null}""",
        """{"actorId":5,"selectedActionKind":null}""",
        """{"actorId":"enemy-agent","selectedActionKind":5}""",
        """{"actorId":"enemy-agent","selectedActionKind":{}}""",
        """{"actorId":"enemy-agent","selectedActionKind":[[[[null]]]]}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedTrailingOverDepthUnknownDuplicateMissingAndWrongTypeInput(
        string document)
    {
        Input input = CreateInput();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(input.Request, document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        Input input = CreateInput();
        const string document = """{"actorId":"enemy-agent","selectedActionKind":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(input.Request, null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterSabotageActionDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterSabotageActionDecisionJsonDeserializer.MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterSabotageActionDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(deserializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterSabotageActionDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterSabotageActionConsiderationRequest), typeof(string)],
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
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Equal(2, input.Request.ActionKinds.Count);
        Assert.Same(input.FirstKind, input.Request.ActionKinds[0]);
        Assert.Same(input.SelectedKind, input.Request.ActionKinds[1]);
    }

    internal static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionKind firstKind = CharacterSabotageActionKind.Conceal;
        CharacterSabotageActionKind selectedKind = CharacterSabotageActionKind.Misquote;
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [firstKind, selectedKind]);

        return new(actor, firstKind, selectedKind, request);
    }

    internal sealed record Input(
        Character Actor,
        CharacterSabotageActionKind FirstKind,
        CharacterSabotageActionKind SelectedKind,
        CharacterSabotageActionConsiderationRequest Request);
}
