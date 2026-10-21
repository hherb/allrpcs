using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveContinuationStateTests
{
    [Fact]
    public void RetainsProfileAndPositionsInSuppliedOrder()
    {
        CharacterPositionSaveState healer = new(new CharacterId("healer"), 1, 2);
        CharacterPositionSaveState seeker = new(new CharacterId("seeker"), 3, 4);

        SaveContinuationState state = new(Profile(), [healer, seeker]);

        Assert.Equal(Profile(), state.SeekerProfile);
        Assert.Equal([healer, seeker], state.Positions);
    }

    [Fact]
    public void RejectsDuplicateOrNullPositionsAndNonFiniteCoordinates()
    {
        CharacterPositionSaveState seeker = new(new CharacterId("seeker"), 3, 4);

        Assert.Throws<ArgumentException>(() => new SaveContinuationState(null, [seeker, seeker]));
        Assert.Throws<ArgumentException>(() => new SaveContinuationState(null, [null!]));
        Assert.Throws<ArgumentException>(() => new CharacterPositionSaveState(new CharacterId("seeker"), float.NaN, 0));
        Assert.Throws<ArgumentException>(() => new CharacterPositionSaveState(new CharacterId("seeker"), 0, float.PositiveInfinity));
    }

    [Fact]
    public void ProfileRequiresTheSeekerIdentityAndCanonicalKeys()
    {
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");

        Assert.Throws<ArgumentException>(() => new SeekerCustomizationSaveState(
            new CharacterIdentity(new CharacterId("healer"), "Mara"), pronouns, "weathered-traveller", "forester"));
        Assert.Throws<ArgumentException>(() => new SeekerCustomizationSaveState(
            new CharacterIdentity(new CharacterId("seeker"), "Mara"), pronouns, "Weathered", "forester"));
    }

    [Fact]
    public void ProfileRoundTripsAndMatchesItsSelection()
    {
        SeekerCustomizationOptions options = RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationProfile profile = SeekerCustomizationSelection.Compose(
            options,
            new CharacterIdentity(new CharacterId("seeker"), "Mara"),
            new SeekerPronouns("she", "her", "her", "hers", "herself"),
            options.AppearanceChoices[0],
            options.BackgroundChoices[0]);
        SeekerCustomizationProfile other = SeekerCustomizationSelection.Compose(
            options,
            new CharacterIdentity(new CharacterId("seeker"), "Mara"),
            new SeekerPronouns("she", "her", "her", "hers", "herself"),
            options.AppearanceChoices[0],
            options.BackgroundChoices[1]);

        SeekerCustomizationSaveState saved = SeekerCustomizationSaveState.From(profile);

        Assert.True(saved.Matches(profile));
        Assert.False(saved.Matches(other));
        Assert.Equal(options.BackgroundChoices[0].Key, saved.BackgroundKey);
    }

    [Fact]
    public void OnlyTheContinuationSchemaRetainsContinuationAndRecoveryMigratesToIt()
    {
        SaveSnapshot recovery = SaveSnapshotFirstPatientRecoveryMigration.Migrate(
            SaveSnapshotFirstPatientRecoveryMigrationTests.RouteVersion());

        SaveSnapshot current = SaveSnapshotContinuationMigration.Migrate(recovery);

        Assert.Same(SaveSchemaVersion.Continuation, current.SchemaVersion);
        Assert.Same(recovery.CharacterMemory, current.CharacterMemory);
        Assert.Same(recovery.FirstPatientRecovery, current.FirstPatientRecovery);
        Assert.Null(current.Continuation!.SeekerProfile);
        Assert.Empty(current.Continuation.Positions);
        Assert.Throws<ArgumentException>(() => SaveSnapshotContinuationMigration.Migrate(current));
        Assert.Throws<ArgumentException>(() => new SaveSnapshot(
            recovery.SchemaVersion,
            recovery.Discovery,
            recovery.WorldEvents,
            recovery.CharacterMemory,
            recovery.FirstPatient,
            recovery.Journal,
            recovery.Map,
            recovery.InvestigationRoutes,
            recovery.FirstPatientRecovery,
            current.Continuation));
    }

    private static SeekerCustomizationSaveState Profile() => new(
        new CharacterIdentity(new CharacterId("seeker"), "Mara"),
        new SeekerPronouns("she", "her", "her", "hers", "herself"),
        "weathered-traveller",
        "forester");
}
