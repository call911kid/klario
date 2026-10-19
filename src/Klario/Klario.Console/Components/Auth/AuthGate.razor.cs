using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Klario.Console.Services;

namespace Klario.Console.Components.Auth;

public partial class AuthGate : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = default!;

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
            try
            {
                if (State.RememberKey)
                {
                    await ProtectedLocalStorage.SetAsync("klario_secret_key", State.SecretKey);
                }
                else
                {
                    await ProtectedLocalStorage.DeleteAsync("klario_secret_key");
                }
            }
            catch
            {
            }

            await OnUnlocked.InvokeAsync();
        }
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
