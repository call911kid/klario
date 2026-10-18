using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Klario.Common.Enums;
using Klario.Console.Models;
using Klario.Console.Services;

namespace Klario.Console.Components.Profiles;

public partial class ChannelEditor : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public SearchProfileViewModel? Profile { get; set; }

    private PostingSource? expandedSource = null;
    private string newTitleTag = string.Empty;
    private string newLocationTag = string.Empty;

    private static readonly string[] titleSuggestions = [".NET Developer", "Backend Developer", "C# Developer", "Software Engineer"];
    private static readonly string[] locationSuggestions = ["Egypt", "United Arab Emirates", "Saudi Arabia", "Canada", "Ireland"];

    protected override void OnInitialized()
    {
        State.OnChange += HandleStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Profile is null)
        {
            try
            {
                await JS.InvokeVoidAsync("initKlarioTopography", "emptyWorkbenchCanvas");
            }
            catch
            {
            }
        }
    }

    private void AddTitleSuggestion(string title)
    {
        if (Profile is null) return;
        if (!Profile.TargetJobTitles.Contains(title, StringComparer.OrdinalIgnoreCase))
        {
            Profile.TargetJobTitles.Add(title);
            State.ShowToast($"Added '{title}'");
        }
    }

    private void AddLocationSuggestion(string loc)
    {
        if (Profile is null) return;
        if (!Profile.TargetLocations.Contains(loc, StringComparer.OrdinalIgnoreCase))
        {
            Profile.TargetLocations.Add(loc);
            State.ShowToast($"Added '{loc}'");
        }
    }

    private void HandleTitleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" && !string.IsNullOrWhiteSpace(newTitleTag) && Profile is not null)
        {
            var val = newTitleTag.Trim().TrimEnd(',');
            if (!string.IsNullOrWhiteSpace(val) && !Profile.TargetJobTitles.Contains(val, StringComparer.OrdinalIgnoreCase))
            {
                Profile.TargetJobTitles.Add(val);
            }
            newTitleTag = string.Empty;
        }
    }

    private void HandleLocationKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" && !string.IsNullOrWhiteSpace(newLocationTag) && Profile is not null)
        {
            var val = newLocationTag.Trim().TrimEnd(',');
            if (!string.IsNullOrWhiteSpace(val) && !Profile.TargetLocations.Contains(val, StringComparer.OrdinalIgnoreCase))
            {
                Profile.TargetLocations.Add(val);
            }
            newLocationTag = string.Empty;
        }
    }

    private async Task SaveCurrentProfileAsync()
    {
        if (Profile is null) return;
        await State.SaveProfileAsync(Profile);
    }

    private async Task ToggleActiveStatusAsync()
    {
        if (Profile is null) return;
        await State.ToggleProfileStatusAsync(Profile);
    }

    private async Task DeleteCurrentProfileAsync()
    {
        if (Profile is null) return;
        await State.DeleteProfileAsync(Profile);
    }

    private async Task AddScheduleAsync(PostingSource source)
    {
        if (Profile is null) return;
        var (name, subtitle, defaultInterval) = GetSourceMetadata(source);
        await State.AddScheduleAsync(Profile, source, name, subtitle, defaultInterval);
    }

    private void ToggleExpandSource(PostingSource source)
    {
        expandedSource = expandedSource == source ? null : source;
    }

    private async Task ToggleScheduleStatusAsync(ProviderScheduleModel schedule)
    {
        if (Profile is null) return;
        await State.ToggleScheduleStatusAsync(Profile, schedule);
    }

    private async Task UpdateScheduleIntervalAsync(ProviderScheduleModel schedule, int intervalMinutes)
    {
        if (Profile is null) return;
        await State.UpdateScheduleIntervalAsync(Profile, schedule, intervalMinutes);
    }

    private async Task HandleIntervalInputChange(ProviderScheduleModel schedule, ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var val) && val >= 1)
        {
            await UpdateScheduleIntervalAsync(schedule, val);
        }
    }

    private static (string Name, string Subtitle, int DefaultInterval) GetSourceMetadata(PostingSource source) =>
        source switch
        {
            PostingSource.LinkedIn => ("LinkedIn", "Public HTTP Ingestion", 15),
            PostingSource.CustomCareerPage => ("Career Pages", "Direct ATS Crawl", 30),
            _ => (source.ToString(), "Job Engine", 30)
        };

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
