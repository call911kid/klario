using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Klario.Console.Services;

namespace Klario.Console.Components.Layout;

public partial class ConsoleHeader : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = default!;

    [Parameter] public EventCallback OnLocked { get; set; }

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    private async Task HandleLockAsync()
    {
        try
        {
            await ProtectedLocalStorage.DeleteAsync("klario_secret_key");
        }
        catch
        {
        }

        State.Lock();
        await OnLocked.InvokeAsync();
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
