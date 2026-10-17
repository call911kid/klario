using Microsoft.AspNetCore.Components;
using Klario.Console.Services;

namespace Klario.Console.Components.Profiles;

public partial class ChannelList : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    private void OnFilterInput(ChangeEventArgs e)
    {
        State.SetSearchFilter(e.Value?.ToString() ?? string.Empty);
    }

    private void CreateNewChannel()
    {
        State.CreateNewProfile();
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
