using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class JournalSaveStateJsonPersistenceTests
{
    [Fact]
    public void SerializesStableCompleteJournalDocument()
    {
        SaveSnapshot snapshot = JournalVersion(PopulatedJournal());

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);

        Assert.Contains(
            "\"journal\":{\"seekerCharacterId\":\"seeker\","
            + "\"selectedSectionKey\":\"experiments\",\"observations\":[{"
            + "\"objectId\":\"old-shrine\",\"displayName\":\"Old Shrine\","
            + "\"description\":\"Layered marks.\"}],\"testimony\":[{"
            + "\"speakerCharacterId\":\"healer\",\"text\":\"The stream changed.\","
            + "\"deliveryMode\":\"whispering\"}],\"hypotheses\":[{"
            + "\"hypothesisId\":\"stream-water\",\"questionId\":\"grey-sleep-cause\","
            + "\"questionPrompt\":\"What causes it?\",\"statement\":\"The stream may.\"}],"
            + "\"experiments\":[{\"experimentId\":\"covered-water\","
            + "\"procedure\":\"Compare samples.\",\"observedOutcome\":\"Covered stayed clear.\"}],"
            + "\"openQuestions\":[{\"questionId\":\"grey-sleep-slowing\","
            + "\"prompt\":\"What slows it?\"}]}",
            serialized,
            StringComparison.Ordinal);
        Assert.StartsWith("{\"schemaVersion\":5,", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTripsEveryJournalValueAndIndependentCollections()
    {
        SaveSnapshot source = JournalVersion(PopulatedJournal());

        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(
            SaveSnapshotJsonSerializer.Serialize(source));

        JournalSaveState journal = restored.Journal!;
        Assert.Same(SaveSchemaVersion.Journal, restored.SchemaVersion);
        Assert.Equal("seeker", journal.SeekerId.Value);
        Assert.Equal("experiments", journal.SelectedSectionKey);
        Assert.Equal("old-shrine", Assert.Single(journal.Observations).ObjectId.Value);
        Assert.Same(DialogueDeliveryMode.Whispering, Assert.Single(journal.Testimony).DeliveryMode);
        Assert.Equal("stream-water", Assert.Single(journal.Hypotheses).HypothesisId.Value);
        Assert.Equal("covered-water", Assert.Single(journal.Experiments).ExperimentId.Value);
        Assert.Equal("grey-sleep-slowing", Assert.Single(journal.OpenQuestions).QuestionId.Value);
        Assert.NotSame(source.Journal, journal);
        Assert.NotSame(source.Journal!.Observations, journal.Observations);
    }

    [Fact]
    public void SerializesAndReadsExplicitEmptyJournalAndNoSelection()
    {
        JournalSaveState empty = EmptyJournal(new CharacterId("seeker"));

        string serialized = SaveSnapshotJsonSerializer.Serialize(JournalVersion(empty));
        JournalSaveState restored = SaveSnapshotJsonDeserializer.Deserialize(serialized).Journal!;

        Assert.Contains("\"selectedSectionKey\":null", serialized, StringComparison.Ordinal);
        Assert.Empty(restored.Observations);
        Assert.Empty(restored.Testimony);
        Assert.Empty(restored.Hypotheses);
        Assert.Empty(restored.Experiments);
        Assert.Empty(restored.OpenQuestions);
    }

    [Fact]
    public void CurrentLoadingMigratesVersionFourToEmptyJournalAndMapVersion()
    {
        SaveSnapshot versionFour = DialogueDeliveryVersion();

        SaveSnapshot current = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            SaveSnapshotJsonSerializer.Serialize(versionFour),
            versionFour.FirstPatient!);

        Assert.Same(SaveSchemaVersion.Map, current.SchemaVersion);
        Assert.NotNull(current.Journal);
        Assert.Equal("seeker", current.Journal.SeekerId.Value);
        Assert.Empty(current.Journal.Observations);
        Assert.Null(current.Journal.SelectedSectionKey);
        Assert.NotNull(current.Map);
        Assert.Empty(current.Map.ReportedLocationIds);
    }

    [Theory]
    [InlineData("\"selectedSectionKey\":\"unknown\"")]
    [InlineData("\"selectedSectionKey\":7")]
    [InlineData("\"observations\":null")]
    public void RejectsInvalidJournalStructure(string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(
            JournalVersion(EmptyJournal(new CharacterId("seeker"))));
        string malformed = replacement.StartsWith("\"selected", StringComparison.Ordinal)
            ? valid.Replace("\"selectedSectionKey\":null", replacement, StringComparison.Ordinal)
            : valid.Replace("\"observations\":[]", replacement, StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    private static SaveSnapshot JournalVersion(JournalSaveState journal)
    {
        SaveSnapshot versionFour = DialogueDeliveryVersion();
        return new SaveSnapshot(
            SaveSchemaVersion.Journal,
            versionFour.Discovery,
            versionFour.WorldEvents,
            versionFour.CharacterMemory,
            versionFour.FirstPatient,
            journal);
    }

    private static SaveSnapshot DialogueDeliveryVersion()
    {
        CharacterId seekerId = new("seeker");
        return new SaveSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(seekerId, []),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            new FirstPatientSaveState(
                RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
                new PreparedIngredientBatchSnapshot([]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([])));
    }

    private static JournalSaveState PopulatedJournal() => new(
        new CharacterId("seeker"),
        [new JournalObservationSaveEntry(new ObjectId("old-shrine"), "Old Shrine", "Layered marks.")],
        [
            new JournalTestimonySaveEntry(
                new CharacterId("healer"),
                "The stream changed.",
                DialogueDeliveryMode.Whispering),
        ],
        [
            new JournalHypothesisSaveEntry(
                new InvestigationHypothesisId("stream-water"),
                new InvestigationQuestionId("grey-sleep-cause"),
                "What causes it?",
                "The stream may."),
        ],
        [
            new JournalExperimentSaveEntry(
                new ExperimentId("covered-water"),
                "Compare samples.",
                "Covered stayed clear."),
        ],
        [
            new JournalOpenQuestionSaveEntry(
                new InvestigationQuestionId("grey-sleep-slowing"),
                "What slows it?"),
        ],
        "experiments");

    private static JournalSaveState EmptyJournal(CharacterId seekerId) => new(
        seekerId,
        [],
        [],
        [],
        [],
        [],
        null);
}
