using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Klario.Console.Services;

namespace Klario.Console.Components.Pages;

public partial class KlarioConsole : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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
