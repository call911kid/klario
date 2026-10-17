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

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
