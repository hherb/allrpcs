using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Characters;

public sealed class SeekerCustomizationRestorationPolicyTests
{
    private static readonly CharacterId LiveSeekerId = new("seeker");

    [Fact]
    public void KeepsTheSessionWhenNothingWasSaved()
    {
        Assert.True(SeekerCustomizationRestorationPolicy.TryDecide(Unselected(), null, LiveSeekerId, out var apply));
        Assert.Null(apply);
    }

    [Fact]
    public void RebuildsTheSavedSelectionOnTheLiveSeekerIdentity()
    {
        SeekerCustomizationSelectionState current = Unselected();

        Assert.True(SeekerCustomizationRestorationPolicy.TryDecide(current, Saved("forester"), LiveSeekerId, out var apply));

        SeekerCustomizationProfile profile = apply!.SelectedProfile!;
        Assert.Same(current, apply.PreviousState);
        Assert.Same(LiveSeekerId, profile.Identity.Id);
        Assert.Equal("Mara", profile.Identity.DisplayName);
        Assert.Equal("forester", profile.Background.Key);
        Assert.Same(current.Options.BackgroundChoices.Single(choice => choice.Key == "forester"), profile.Background);
    }

    [Fact]
    public void KeepsAnIdenticalSelectionAndRejectsADifferentSeeker()
    {
        SeekerCustomizationSelectionState current = Unselected();
        Assert.True(SeekerCustomizationRestorationPolicy.TryDecide(current, Saved("forester"), LiveSeekerId, out var selected));

        Assert.True(SeekerCustomizationRestorationPolicy.TryDecide(selected!, Saved("forester"), LiveSeekerId, out var keep));
        Assert.Null(keep);
        Assert.False(SeekerCustomizationRestorationPolicy.TryDecide(selected!, Saved("scribe"), LiveSeekerId, out var reject));
        Assert.Null(reject);
    }

    [Fact]
    public void RejectsUnknownOptionKeys()
    {
        Assert.False(SeekerCustomizationRestorationPolicy.TryDecide(Unselected(), Saved("knight"), LiveSeekerId, out var apply));
        Assert.Null(apply);
        Assert.False(SeekerCustomizationRestorationPolicy.TryDecide(
            Unselected(), Saved("forester", "crown"), LiveSeekerId, out var unknownAppearance));
        Assert.Null(unknownAppearance);
    }

    private static SeekerCustomizationSelectionState Unselected() =>
        SeekerCustomizationSelectionStateTransition.Initialize(RepresentativeSeekerCustomizationOptionsSource.Create());

    private static SeekerCustomizationSaveState Saved(string background, string appearance = "travelling-cloak") => new(
        new CharacterIdentity(new CharacterId("seeker"), "Mara"),
        new SeekerPronouns("she", "her", "her", "hers", "herself"),
        appearance,
        background);
}
