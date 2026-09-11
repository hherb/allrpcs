using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Experiments;

public sealed class ExperimentJournalTests
{
    [Fact]
    public void RecordRetainsExactIdentityEventProcedureAndObservedOutcome()
    {
        ExperimentId experimentId = new("cooling-trial");
        WorldEvent worldEvent = Event("cooling-trial-occurred");

        ExperimentJournalRecord record = new(
            experimentId,
            worldEvent,
            "Cool one prepared dose before administration.",
            "The patient became less restless.");

        Assert.Same(experimentId, record.ExperimentId);
        Assert.Same(worldEvent, record.WorldEvent);
        Assert.Equal("Cool one prepared dose before administration.", record.Procedure);
        Assert.Equal("The patient became less restless.", record.ObservedOutcome);
    }

    [Fact]
    public void RecordRequiresBoundedNonblankTextAndExposesOnlyImmutableState()
    {
        ExperimentId experimentId = new("cooling-trial");
        WorldEvent worldEvent = Event("cooling-trial-occurred");
        string maximumProcedure = new('p', ExperimentJournalRecord.MaximumProcedureLength);
        string maximumOutcome = new('o', ExperimentJournalRecord.MaximumObservedOutcomeLength);

        ExperimentJournalRecord record = new(
            experimentId,
            worldEvent,
            maximumProcedure,
            maximumOutcome);

        Assert.Equal(maximumProcedure, record.Procedure);
        Assert.Equal(maximumOutcome, record.ObservedOutcome);
        Assert.Throws<ArgumentNullException>(() =>
            new ExperimentJournalRecord(null!, worldEvent, "Procedure", "Outcome"));
        Assert.Throws<ArgumentNullException>(() =>
            new ExperimentJournalRecord(experimentId, null!, "Procedure", "Outcome"));
        Assert.Throws<ArgumentException>(() =>
            new ExperimentJournalRecord(experimentId, worldEvent, " ", "Outcome"));
        Assert.Throws<ArgumentException>(() =>
            new ExperimentJournalRecord(experimentId, worldEvent, "Procedure", " "));
        Assert.Throws<ArgumentException>(() => new ExperimentJournalRecord(
            experimentId,
            worldEvent,
            maximumProcedure + "x",
            "Outcome"));
        Assert.Throws<ArgumentException>(() => new ExperimentJournalRecord(
            experimentId,
            worldEvent,
            "Procedure",
            maximumOutcome + "x"));

        Type recordType = typeof(ExperimentJournalRecord);
        Assert.Equal(
            ["ExperimentId", "ObservedOutcome", "Procedure", "WorldEvent"],
            recordType.GetProperties().Select(property => property.Name).Order());
        Assert.All(recordType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(ExperimentId), typeof(WorldEvent), typeof(string), typeof(string)],
            Assert.Single(recordType.GetConstructors())
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray());
        Assert.Empty(recordType.GetEvents());
        Assert.Empty(recordType.GetMethods().Where(
            method => method.DeclaringType == recordType && !method.IsSpecialName));
        Assert.True(recordType.IsSealed);
    }

    [Fact]
    public void JournalCopiesDistinctRecordsInStableOrder()
    {
        ExperimentJournalRecord first = Record("first-trial", "first-trial-occurred");
        ExperimentJournalRecord second = Record("second-trial", "second-trial-occurred");
        ExperimentJournalRecord replacement = Record("replacement", "replacement-occurred");
        ExperimentJournalRecord[] supplied = [first, second];

        ExperimentJournalSnapshot journal = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            journal.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(second, record));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ExperimentJournalRecord>)journal.Records).Add(replacement));
    }

    [Fact]
    public void JournalRejectsNullDuplicateAndExcessiveRecords()
    {
        ExperimentJournalRecord[] maximum = Enumerable
            .Range(0, ExperimentJournalSnapshot.MaximumRecordCount)
            .Select(index => Record($"experiment-{index}", $"experiment-{index}-occurred"))
            .ToArray();
        ExperimentJournalRecord duplicateIdentity = new(
            maximum[0].ExperimentId,
            Event("different-event"),
            "Procedure",
            "Outcome");
        ExperimentJournalRecord duplicateEvent = new(
            new ExperimentId("different-identity"),
            maximum[0].WorldEvent,
            "Procedure",
            "Outcome");

        Assert.Empty(new ExperimentJournalSnapshot([]).Records);
        Assert.Equal(
            ExperimentJournalSnapshot.MaximumRecordCount,
            new ExperimentJournalSnapshot(maximum).Records.Count);
        Assert.Throws<ArgumentNullException>(() => new ExperimentJournalSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new ExperimentJournalSnapshot([null!]));
        Assert.Throws<ArgumentException>(() =>
            new ExperimentJournalSnapshot([maximum[0], duplicateIdentity]));
        Assert.Throws<ArgumentException>(() =>
            new ExperimentJournalSnapshot([maximum[0], duplicateEvent]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExperimentJournalSnapshot(
            maximum.Append(Record("extra", "extra-occurred"))));

        Type journalType = typeof(ExperimentJournalSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(journalType.GetProperties());
        Assert.Equal("Records", property.Name);
        Assert.Equal(typeof(IReadOnlyList<ExperimentJournalRecord>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal(
            typeof(IEnumerable<ExperimentJournalRecord>),
            Assert.Single(journalType.GetConstructors()).GetParameters()[0].ParameterType);
        Assert.Empty(journalType.GetEvents());
        Assert.Empty(journalType.GetMethods().Where(
            method => method.DeclaringType == journalType && !method.IsSpecialName));
        Assert.True(journalType.IsSealed);
    }

    [Fact]
    public void PolicyRequiresTheExactLatestAuthoredEventAndAvailableJournalSpace()
    {
        ExperimentId experimentId = new("cooling-trial");
        WorldEvent worldEvent = Event("cooling-trial-occurred");
        WorldEvent earlier = Event("preparation-occurred");
        WorldEventSnapshot worldEvents = new([earlier, worldEvent]);
        WorldEventLogSnapshot latestLog = new([earlier.EventId, worldEvent.EventId]);
        ExperimentJournalSnapshot journal = new([]);

        Assert.True(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            worldEvents,
            latestLog,
            journal));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            new WorldEvent(worldEvent.EventId, worldEvent.TimeEffect),
            worldEvents,
            latestLog,
            journal));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            new WorldEventSnapshot([earlier]),
            latestLog,
            journal));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            worldEvents,
            new WorldEventLogSnapshot([worldEvent.EventId, earlier.EventId]),
            journal));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            worldEvents,
            new WorldEventLogSnapshot([]),
            journal));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            experimentId,
            worldEvent,
            worldEvents,
            latestLog,
            new ExperimentJournalSnapshot([Record("cooling-trial", "other-event")])));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            new ExperimentId("other-trial"),
            worldEvent,
            worldEvents,
            latestLog,
            new ExperimentJournalSnapshot([Record("retained", "cooling-trial-occurred")])));
        Assert.False(ExperimentRecordingPolicy.CanRecord(
            new ExperimentId("extra"),
            worldEvent,
            worldEvents,
            latestLog,
            new ExperimentJournalSnapshot(Enumerable.Range(
                0,
                ExperimentJournalSnapshot.MaximumRecordCount).Select(
                    index => Record($"experiment-{index}", $"event-{index}")))));
    }

    [Fact]
    public void TransitionAtomicallyAppendsOneExactUniversalExperiment()
    {
        ExperimentJournalRecord retained = Record("first-trial", "first-trial-occurred");
        ExperimentJournalSnapshot journal = new([retained]);
        ExperimentId experimentId = new("cooling-trial");
        WorldEvent worldEvent = Event("cooling-trial-occurred");
        WorldEventSnapshot worldEvents = new([retained.WorldEvent, worldEvent]);
        WorldEventLogSnapshot eventLog = new(
            [retained.WorldEvent.EventId, worldEvent.EventId]);

        ExperimentJournalSnapshot result = ExperimentRecordingTransition.Record(
            experimentId,
            worldEvent,
            "Cool one prepared dose before administration.",
            "The patient became less restless.",
            worldEvents,
            eventLog,
            journal);

        Assert.Collection(
            result.Records,
            record => Assert.Same(retained, record),
            record =>
            {
                Assert.Same(experimentId, record.ExperimentId);
                Assert.Same(worldEvent, record.WorldEvent);
                Assert.Equal(
                    "Cool one prepared dose before administration.",
                    record.Procedure);
                Assert.Equal("The patient became less restless.", record.ObservedOutcome);
            });
        Assert.Single(journal.Records);
        Assert.Same(retained, journal.Records[0]);
    }

    [Fact]
    public void TransitionRejectsIneligibleInputsWithoutChangingTheJournal()
    {
        ExperimentJournalRecord retained = Record("retained", "retained-occurred");
        ExperimentJournalSnapshot journal = new([retained]);
        WorldEvent worldEvent = Event("new-event");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot emptyLog = new([]);

        Assert.Throws<InvalidOperationException>(() => ExperimentRecordingTransition.Record(
            new ExperimentId("new-experiment"),
            worldEvent,
            "Procedure",
            "Outcome",
            worldEvents,
            emptyLog,
            journal));
        Assert.Throws<ArgumentNullException>(() => ExperimentRecordingTransition.Record(
            null!, worldEvent, "Procedure", "Outcome", worldEvents, emptyLog, journal));
        Assert.Throws<ArgumentNullException>(() => ExperimentRecordingTransition.Record(
            new ExperimentId("new-experiment"),
            null!,
            "Procedure",
            "Outcome",
            worldEvents,
            emptyLog,
            journal));
        Assert.Single(journal.Records);
        Assert.Same(retained, journal.Records[0]);
    }

    private static ExperimentJournalRecord Record(string experimentId, string eventId) => new(
        new ExperimentId(experimentId),
        Event(eventId),
        "Test a bounded procedure.",
        "Record a qualitative outcome.");

    private static WorldEvent Event(string eventId) => new(
        new WorldEventId(eventId),
        WorldEventTimeEffect.Advance);
}
