using Microsoft.AspNetCore.Components;
using Klario.Console.Services;

namespace Klario.Console.Components.Auth;

public partial class AuthGate : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;

    [Parameter] public EventCallback OnUnlocked { get; set; }

    private bool showPassword = false;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    private void OnSecretKeyInput(ChangeEventArgs e)
    {
        State.SecretKey = e.Value?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(State.GateError))
        {
            State.GateError = string.Empty;
            State.NotifyStateChanged();
        }
    }

    private async Task HandleUnlockAsync()
    {
        var success = await State.UnlockAsync();
        if (success)
        {
            await OnUnlocked.InvokeAsync();
        }
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
