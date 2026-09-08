using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterIntentionValidationResultTests
{
    private static readonly CharacterIntentionValidationErrorCode[] ExpectedErrorCodes =
    [
        CharacterIntentionValidationErrorCode.MissingTargetObject,
        CharacterIntentionValidationErrorCode.UnexpectedTargetObject,
    ];

    [Fact]
    public void RetainsExactErrorsAndReportsValidity()
    {
        CharacterIntentionValidationError error = new(
            CharacterIntentionValidationErrorCode.MissingTargetObject);
        CharacterIntentionValidationError[] source = [error];

        CharacterIntentionValidationResult invalidResult = new(source);
        CharacterIntentionValidationResult validResult = new([]);
        source[0] = new(CharacterIntentionValidationErrorCode.UnexpectedTargetObject);

        Assert.False(invalidResult.IsValid);
        Assert.Same(error, Assert.Single(invalidResult.Errors));
        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.Errors);
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<IList<CharacterIntentionValidationError>>(invalidResult.Errors)
                .Add(error));
    }

    [Fact]
    public void DefinesOnlyTheInitialValidationErrorCodes()
    {
        Assert.Equal(ExpectedErrorCodes, Enum.GetValues<CharacterIntentionValidationErrorCode>());
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CharacterIntentionValidationResult(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterIntentionValidationResult([null!]));
    }
}
