using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelStreamTextPresentationTests
{
    [Fact]
    public void ConcatenatesOnlyExactTextDeltasInStableOrder()
    {
        CharacterModelTextDelta first = new("Heal");
        CharacterModelTextDelta second = new("er");
        CharacterModelStreamTermination termination = new(
            CharacterModelStreamTerminationStatus.TimedOut,
            null,
            null);
        CharacterModelStreamSnapshot snapshot = new([first, second, termination]);

        CharacterModelStreamTextPresentation result =
            CharacterModelStreamTextPresentation.Project(snapshot);

        Assert.Equal("Healer", result.Text);
        Assert.True(result.HasText);
        Assert.Equal(6, result.Text.Length);
        Assert.Same(first, snapshot.Updates[0]);
        Assert.Same(second, snapshot.Updates[1]);
        Assert.Same(termination, snapshot.Updates[2]);
    }

    [Fact]
    public void ReturnsCanonicalEmptyWithoutExposingTerminalPayloads()
    {
        CharacterTurnRequest request =
            CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(
                    request.RespondingCharacterId,
                    "private complete speech"),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        CharacterModelStreamTermination completed = new(
            CharacterModelStreamTerminationStatus.Completed,
            response,
            null);
        CharacterModelStreamTermination failed = new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(CharacterModelFailureCode.ProviderRejected));

        CharacterModelStreamTextPresentation empty =
            CharacterModelStreamTextPresentation.Project(new CharacterModelStreamSnapshot([]));
        CharacterModelStreamTextPresentation completedResult =
            CharacterModelStreamTextPresentation.Project(new CharacterModelStreamSnapshot([completed]));
        CharacterModelStreamTextPresentation failedResult =
            CharacterModelStreamTextPresentation.Project(new CharacterModelStreamSnapshot([failed]));

        Assert.Same(CharacterModelStreamTextPresentation.Empty, empty);
        Assert.Same(empty, completedResult);
        Assert.Same(empty, failedResult);
        Assert.Empty(empty.Text);
        Assert.False(empty.HasText);
        Assert.DoesNotContain(response.Speech!.Text, empty.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(
            failed.Failure!.Code.ToString(),
            failedResult.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsNullAndRetainsOnlyProjectedText()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelStreamTextPresentation.Project(null!));

        CharacterModelStreamTextPresentation result =
            CharacterModelStreamTextPresentation.Project(
                new CharacterModelStreamSnapshot([new CharacterModelTextDelta("visible")]));
        System.Reflection.PropertyInfo[] properties =
            typeof(CharacterModelStreamTextPresentation).GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.Collection(
            properties.OrderBy(property => property.Name, StringComparer.Ordinal),
            property =>
            {
                Assert.Equal(nameof(CharacterModelStreamTextPresentation.HasText), property.Name);
                Assert.Equal(typeof(bool), property.PropertyType);
                Assert.True(property.CanRead);
                Assert.Null(property.SetMethod);
            },
            property =>
            {
                Assert.Equal(nameof(CharacterModelStreamTextPresentation.Text), property.Name);
                Assert.Equal(typeof(string), property.PropertyType);
                Assert.True(property.CanRead);
                Assert.Null(property.SetMethod);
            });
        Assert.Equal("visible", result.ToString());
    }
}
