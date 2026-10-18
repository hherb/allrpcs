namespace Allrpcs.Application.Input;

/// <summary>One player focused-search input action.</summary>
public enum FocusedSearchInputAction
{
    /// <summary>Cancels the Seeker's active focused search.</summary>
    Cancel,

    /// <summary>Completes the active search with its authored outcome.</summary>
    Complete,

    /// <summary>Resolves the Seeker's first retained outcome.</summary>
    Resolve,

    /// <summary>Starts a focused search of the focused interaction target.</summary>
    Start,
}
