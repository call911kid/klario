using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Klario.Console.Services;

namespace Klario.Console.Components.Exclusions;

public partial class ExcludedCompaniesManager : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;

    private string newCompanyName = string.Empty;

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    private async Task HandleCompanyKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter")
        {
            await AddExcludedCompanyAsync();
        }
    }

    private async Task AddExcludedCompanyAsync()
    {
        if (string.IsNullOrWhiteSpace(newCompanyName)) return;

        await State.AddExcludedCompanyAsync(newCompanyName);
        newCompanyName = string.Empty;
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
