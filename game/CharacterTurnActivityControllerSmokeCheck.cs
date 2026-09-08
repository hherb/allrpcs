using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused character-turn activity controller checks when explicitly enabled.</summary>
public partial class CharacterTurnActivityControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private CharacterTurnActivityController controller = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterTurnActivity? expectedSceneActivity = controller.Activity;
        CharacterTurnActivity awaiting = CreateActivity(CharacterTurnActivityState.AwaitingResponse);
        CharacterTurnActivityController target = new();
        AddChild(target);
        if (target.Activity is not null
            || target.TryCancel()
            || !target.TryInitialize(awaiting)
            || !ReferenceEquals(target.Activity, awaiting)
            || !target.TryCancel())
        {
            Fail("Character-turn controller failed awaiting-response initialization or cancellation.");
            return;
        }

        CharacterTurnActivity? cancelledAwaiting = target.Activity;
        if (cancelledAwaiting is null
            || ReferenceEquals(cancelledAwaiting, awaiting)
            || !ReferenceEquals(cancelledAwaiting.Request, awaiting.Request)
            || cancelledAwaiting.Response is not null
            || !ReferenceEquals(cancelledAwaiting.State, CharacterTurnActivityState.Cancelled)
            || target.TryCancel()
            || !ReferenceEquals(target.Activity, cancelledAwaiting)
            || target.TryInitialize(CreateActivity(CharacterTurnActivityState.Speaking))
            || !ReferenceEquals(target.Activity, cancelledAwaiting))
        {
            Fail("Character-turn controller failed exact cancellation or rejected-state preservation.");
            return;
        }

        if (!ProvesSpeakingCancellation()
            || !ProvesResponseReceptionAndCompletion()
            || !ProvesFreshStartAfterAbsentAndTerminalState()
            || !ProvesTerminalAndNullRejection()
            || !HasExpectedControllerSurface()
            || !ReferenceEquals(controller.Activity, expectedSceneActivity))
        {
            return;
        }

        GD.Print(
            "Character-turn controller smoke passed: exact one-time initialization, eligible fresh "
            + "starts, active replacement rejection, awaiting-response and speaking cancellation, "
            + "exact response reception and speaking completion, Domain-policy delegation, null "
            + "rejection, and unchanged rejected state were correct.");
        GetTree().Quit();
    }

    private bool ProvesSpeakingCancellation()
    {
        CharacterTurnActivity speaking = CreateActivity(CharacterTurnActivityState.Speaking);
        CharacterTurnActivityController target = new();
        AddChild(target);

        if (!target.TryInitialize(speaking) || !target.TryCancel())
        {
            Fail("Character-turn controller rejected speaking cancellation.");
            return false;
        }

        CharacterTurnActivity? cancelled = target.Activity;
        if (cancelled is null
            || ReferenceEquals(cancelled, speaking)
            || !ReferenceEquals(cancelled.Request, speaking.Request)
            || !ReferenceEquals(cancelled.Response, speaking.Response)
            || !ReferenceEquals(cancelled.State, CharacterTurnActivityState.Cancelled)
            || !ReferenceEquals(speaking.State, CharacterTurnActivityState.Speaking))
        {
            Fail("Character-turn controller failed to install the exact speaking cancellation result.");
            return false;
        }

        return true;
    }

    private bool ProvesTerminalAndNullRejection()
    {
        foreach (CharacterTurnActivityState state in new[]
                 {
                     CharacterTurnActivityState.Completed,
                     CharacterTurnActivityState.Cancelled,
                 })
        {
            CharacterTurnActivity activity = CreateActivity(state);
            CharacterTurnActivityController target = new();
            AddChild(target);
            if (!target.TryInitialize(activity)
                || target.TryCancel()
                || !ReferenceEquals(target.Activity, activity))
            {
                Fail("Character-turn controller changed completed or cancelled activity.");
                return false;
            }
        }

        CharacterTurnActivityController nullTarget = new();
        AddChild(nullTarget);
        try
        {
            nullTarget.TryInitialize(null!);
        }
        catch (ArgumentNullException)
        {
            if (nullTarget.Activity is null)
            {
                return true;
            }

            Fail("Character-turn controller changed state after null initialization.");
            return false;
        }

        Fail("Character-turn controller accepted null initialization.");
        return false;
    }

    private bool ProvesResponseReceptionAndCompletion()
    {
        CharacterTurnActivity awaiting = CreateActivity(CharacterTurnActivityState.AwaitingResponse);
        CharacterTurnResponse response = CreateResponse(awaiting.Request);
        CharacterTurnActivityController target = new();
        AddChild(target);

        if (target.TryReceiveResponse(response)
            || target.TryComplete()
            || !target.TryInitialize(awaiting)
            || !target.TryReceiveResponse(response))
        {
            Fail("Character-turn controller failed exact response reception.");
            return false;
        }

        CharacterTurnActivity? speaking = target.Activity;
        if (speaking is null
            || ReferenceEquals(speaking, awaiting)
            || !ReferenceEquals(speaking.Request, awaiting.Request)
            || !ReferenceEquals(speaking.Response, response)
            || !ReferenceEquals(speaking.State, CharacterTurnActivityState.Speaking)
            || !target.TryComplete())
        {
            Fail("Character-turn controller failed to install exact speaking activity.");
            return false;
        }

        CharacterTurnActivity? completed = target.Activity;
        if (completed is null
            || ReferenceEquals(completed, speaking)
            || !ReferenceEquals(completed.Request, speaking!.Request)
            || !ReferenceEquals(completed.Response, speaking.Response)
            || !ReferenceEquals(completed.State, CharacterTurnActivityState.Completed)
            || target.TryComplete()
            || target.TryCancel()
            || target.TryReceiveResponse(response)
            || !ReferenceEquals(target.Activity, completed))
        {
            Fail("Character-turn controller failed exact completion or terminal preservation.");
            return false;
        }

        try
        {
            target.TryReceiveResponse(null!);
        }
        catch (ArgumentNullException)
        {
            if (ReferenceEquals(target.Activity, completed))
            {
                return true;
            }
        }

        Fail("Character-turn controller failed null response rejection or state preservation.");
        return false;
    }

    private bool ProvesFreshStartAfterAbsentAndTerminalState()
    {
        CharacterTurnRequest firstRequest = CreateActivity(
            CharacterTurnActivityState.AwaitingResponse).Request;
        CharacterTurnRequest secondRequest = CreateActivity(
            CharacterTurnActivityState.AwaitingResponse).Request;
        CharacterTurnRequest thirdRequest = CreateActivity(
            CharacterTurnActivityState.AwaitingResponse).Request;
        CharacterTurnActivityController target = new();
        AddChild(target);

        if (!target.TryStart(firstRequest)
            || target.Activity is not { } first
            || !ReferenceEquals(first.Request, firstRequest)
            || !ReferenceEquals(first.State, CharacterTurnActivityState.AwaitingResponse)
            || target.TryStart(secondRequest)
            || !ReferenceEquals(target.Activity, first)
            || !target.TryCancel()
            || !target.TryStart(secondRequest)
            || target.Activity is not { } second
            || !ReferenceEquals(second.Request, secondRequest)
            || !ReferenceEquals(second.State, CharacterTurnActivityState.AwaitingResponse)
            || !target.TryReceiveResponse(CreateResponse(secondRequest))
            || !target.TryComplete()
            || !target.TryStart(thirdRequest)
            || target.Activity is not { } third
            || !ReferenceEquals(third.Request, thirdRequest)
            || !ReferenceEquals(third.State, CharacterTurnActivityState.AwaitingResponse))
        {
            Fail("Character-turn controller failed eligible fresh start or active preservation.");
            return false;
        }

        try
        {
            target.TryStart(null!);
        }
        catch (ArgumentNullException)
        {
            if (ReferenceEquals(target.Activity, third))
            {
                return true;
            }

            Fail("Character-turn controller changed state after a null fresh-start request.");
            return false;
        }

        Fail("Character-turn controller accepted a null fresh-start request.");
        return false;
    }

    private bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(CharacterTurnActivityController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = typeof(CharacterTurnActivityController)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        bool matches = properties.Length == 1
            && properties[0].Name == nameof(CharacterTurnActivityController.Activity)
            && properties[0].PropertyType == typeof(CharacterTurnActivity)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate
            && methods.Length == 5
            && methods[0].Name == nameof(CharacterTurnActivityController.TryCancel)
            && methods[0].ReturnType == typeof(bool)
            && methods[0].GetParameters().Length == 0
            && methods[1].Name == nameof(CharacterTurnActivityController.TryComplete)
            && methods[1].ReturnType == typeof(bool)
            && methods[1].GetParameters().Length == 0
            && methods[2].Name == nameof(CharacterTurnActivityController.TryInitialize)
            && methods[2].ReturnType == typeof(bool)
            && methods[2].GetParameters().Select(parameter => parameter.ParameterType)
                .SequenceEqual([typeof(CharacterTurnActivity)])
            && methods[3].Name == nameof(CharacterTurnActivityController.TryReceiveResponse)
            && methods[3].ReturnType == typeof(bool)
            && methods[3].GetParameters().Select(parameter => parameter.ParameterType)
                .SequenceEqual([typeof(CharacterTurnResponse)])
            && methods[4].Name == nameof(CharacterTurnActivityController.TryStart)
            && methods[4].ReturnType == typeof(bool)
            && methods[4].GetParameters().Select(parameter => parameter.ParameterType)
                .SequenceEqual([typeof(CharacterTurnRequest)]);

        if (!matches)
        {
            Fail("Character-turn controller exposed an unexpected public surface.");
        }

        return matches;
    }

    private static CharacterTurnActivity CreateActivity(CharacterTurnActivityState state)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Please wait.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterTurnResponse? response = state == CharacterTurnActivityState.AwaitingResponse
            ? null
            : CreateResponse(request);

        return new CharacterTurnActivity(request, response, state);
    }

    private static CharacterTurnResponse CreateResponse(CharacterTurnRequest request) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(request.RespondingCharacterId, "I will wait."),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
