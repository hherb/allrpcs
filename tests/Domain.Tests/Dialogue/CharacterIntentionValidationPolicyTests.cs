using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterIntentionValidationPolicyTests
{
    [Fact]
    public void AcceptsWaitWithoutTargetAndTargetedObjectIntentions()
    {
        CharacterIntention wait = new(CharacterIntentionType.Wait, null);
        ObjectId examinedTarget = new("moonwort-patch");
        CharacterIntention examine = new(CharacterIntentionType.ExamineObject, examinedTarget);
        ObjectId collectedTarget = new("yarrow-patch");
        CharacterIntention collect = new(CharacterIntentionType.CollectObject, collectedTarget);

        CharacterIntentionValidationResult waitResult =
            CharacterIntentionValidationPolicy.Validate(wait);
        CharacterIntentionValidationResult examineResult =
            CharacterIntentionValidationPolicy.Validate(examine);
        CharacterIntentionValidationResult collectResult =
            CharacterIntentionValidationPolicy.Validate(collect);

        Assert.True(waitResult.IsValid);
        Assert.True(examineResult.IsValid);
        Assert.True(collectResult.IsValid);
        Assert.Same(examinedTarget, examine.TargetObjectId);
        Assert.Same(collectedTarget, collect.TargetObjectId);
    }

    [Theory]
    [MemberData(nameof(TargetedTypes))]
    public void ReportsMissingTargetForTargetedIntentions(CharacterIntentionType type)
    {
        CharacterIntention intention = new(type, null);

        CharacterIntentionValidationResult result =
            CharacterIntentionValidationPolicy.Validate(intention);

        CharacterIntentionValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsValid);
        Assert.Equal(CharacterIntentionValidationErrorCode.MissingTargetObject, error.Code);
        Assert.Same(type, intention.Type);
        Assert.Null(intention.TargetObjectId);
    }

    [Fact]
    public void ReportsUnexpectedTargetForWaitIntention()
    {
        ObjectId targetObjectId = new("moonwort-patch");
        CharacterIntention intention = new(CharacterIntentionType.Wait, targetObjectId);

        CharacterIntentionValidationResult result =
            CharacterIntentionValidationPolicy.Validate(intention);

        CharacterIntentionValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsValid);
        Assert.Equal(CharacterIntentionValidationErrorCode.UnexpectedTargetObject, error.Code);
        Assert.Same(targetObjectId, intention.TargetObjectId);
    }

    [Fact]
    public void RejectsNullIntention()
    {
        Assert.Throws<ArgumentNullException>(
            () => CharacterIntentionValidationPolicy.Validate(null!));
    }

    public static TheoryData<CharacterIntentionType> TargetedTypes => new()
    {
        CharacterIntentionType.ExamineObject,
        CharacterIntentionType.CollectObject,
    };
}
