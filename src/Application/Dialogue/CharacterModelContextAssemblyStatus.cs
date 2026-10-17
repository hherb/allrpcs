namespace Allrpcs.Application.Dialogue;

/// <summary>Describes whether exact mandatory context fits the model context limits.</summary>
public enum CharacterModelContextAssemblyStatus
{
    /// <summary>A complete generation request is available.</summary>
    Success,

    /// <summary>No request was created because mandatory context alone exceeds the budget.</summary>
    RequiredContextBudgetExceeded,
}
