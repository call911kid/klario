using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Klario.Console.Services;

namespace Klario.Console.Components.Pages;

public partial class KlarioConsole : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool isCheckingAuth = true;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
        isCheckingAuth = !State.IsUnlocked;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !State.IsUnlocked)
        {
            try
            {
                await JS.InvokeVoidAsync("initKlarioTopography", "loginTopoCanvas");
                var result = await ProtectedLocalStorage.GetAsync<string>("klario_secret_key");
                if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
                {
                    State.SecretKey = result.Value;
                    State.RememberKey = true;
                    var success = await State.UnlockAsync();
                    if (success)
                    {
                        await JS.InvokeVoidAsync("destroyKlarioTopography", "loginTopoCanvas");
                    }
                    else
                    {
                        await ProtectedLocalStorage.DeleteAsync("klario_secret_key");
                    }
                }
            }
            catch
            {
            }
            finally
            {
                isCheckingAuth = false;
                StateHasChanged();
            }
            return;
        }

        try
        {
            if (!State.IsUnlocked)
            {
                await JS.InvokeVoidAsync("initKlarioTopography", "loginTopoCanvas");
            }
            else
            {
                await JS.InvokeVoidAsync("initKlarioTopography", "homeTopoCanvas");
            }
        }
        catch
        {
        }
    }

    private async Task HandleUnlockedAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("destroyKlarioTopography", "loginTopoCanvas");
        }
        catch
        {
        }
    }

    private async Task HandleLockedAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("destroyKlarioTopography", "homeTopoCanvas");
        }
        catch
        {
        }
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
