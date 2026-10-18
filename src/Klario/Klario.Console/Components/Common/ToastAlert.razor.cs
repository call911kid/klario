using Microsoft.AspNetCore.Components;
using Klario.Console.Services;

namespace Klario.Console.Components.Common;

public partial class ToastAlert : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    private async Task ExecuteActionAsync()
    {
        var callback = State.ToastActionCallback;
        State.ClearToast();
        if (callback is not null)
        {
            await callback();
        }
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
