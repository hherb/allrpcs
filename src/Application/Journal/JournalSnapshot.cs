using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Retains one immutable safe journal snapshot with content-free pending sections.</summary>
public sealed class JournalSnapshot
{
    internal JournalSnapshot(
        JournalSectionCatalogue catalogue,
        JournalObservationSectionSnapshot observations,
        JournalTestimonySectionSnapshot? testimony,
        IEnumerable<JournalSectionDefinition> pendingSections)
    {
        Catalogue = catalogue;
        Observations = observations;
        Testimony = testimony;
        PendingSections = pendingSections.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact canonical journal-section catalogue.</summary>
    public JournalSectionCatalogue Catalogue { get; }

    /// <summary>Gets the exact visibility-safe observations-section snapshot.</summary>
    public JournalObservationSectionSnapshot Observations { get; }

    /// <summary>Gets the exact Seeker identifier retained by the observations snapshot.</summary>
    public CharacterId SeekerId => Observations.SeekerId;

    /// <summary>Gets the exact audience-free testimony snapshot when installed.</summary>
    public JournalTestimonySectionSnapshot? Testimony { get; }

    /// <summary>Gets copied content-free pending-section definitions in canonical order.</summary>
    public IReadOnlyList<JournalSectionDefinition> PendingSections { get; }
}
