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

    private SearchProfileViewModel? _trackedProfile;
    private ProfileSnapshot? _savedSnapshot;

    private PostingSource? expandedSource = null;
    private string newTitleTag = string.Empty;
    private string newLocationTag = string.Empty;

    private static readonly string[] titleSuggestions = [".NET Developer", "Backend Developer", "C# Developer", "Software Engineer"];
    private static readonly string[] locationSuggestions = ["Egypt", "United Arab Emirates", "Saudi Arabia", "Canada", "Ireland"];

    protected override void OnParametersSet()
    {
        if (Profile is not null && Profile != _trackedProfile)
        {
            _trackedProfile = Profile;
            _savedSnapshot = ProfileSnapshot.From(Profile);
        }
        else if (Profile is null)
        {
            _trackedProfile = null;
            _savedSnapshot = null;
        }
    }

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

    private bool HasUnsavedChanges
    {
        get
        {
            if (Profile is null) return false;
            if (string.IsNullOrWhiteSpace(Profile.Name)) return false;
            if (Profile.Id == Guid.Empty) return true;
            return _savedSnapshot is not null && !_savedSnapshot.Matches(Profile);
        }
    }

    private void DiscardChanges()
    {
        if (Profile is null || _savedSnapshot is null) return;
        _savedSnapshot.ApplyTo(Profile);
        State.ShowToast("Changes discarded.");
    }

    private void HandleMaxPostingAgeInput(ChangeEventArgs e)
    {
        if (Profile is null) return;
        if (int.TryParse(e.Value?.ToString(), out var val) && val >= 1)
        {
            Profile.MaxPostingAgeMinutes = val;
        }
    }

    private async Task SaveCurrentProfileAsync()
    {
        if (Profile is null || !HasUnsavedChanges) return;
        var ok = await State.SaveProfileAsync(Profile);
        if (ok && Profile.Id != Guid.Empty)
        {
            _trackedProfile = Profile;
            _savedSnapshot = ProfileSnapshot.From(Profile);
            StateHasChanged();
        }
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

    private sealed record ProfileSnapshot(
        string Name,
        string? Description,
        int MaxPostingAgeMinutes,
        WorkplacePreference Workplace,
        ExperienceLevel Experience,
        JobType JobType,
        string TelegramChatId,
        string BotToken,
        List<string> TargetJobTitles,
        List<string> TargetLocations)
    {
        public static ProfileSnapshot From(SearchProfileViewModel profile) => new(
            profile.Name ?? string.Empty,
            profile.Description ?? string.Empty,
            profile.MaxPostingAgeMinutes,
            profile.Workplace,
            profile.Experience,
            profile.JobType,
            profile.TelegramChatId ?? string.Empty,
            profile.BotToken ?? string.Empty,
            [.. profile.TargetJobTitles],
            [.. profile.TargetLocations]);

        public bool Matches(SearchProfileViewModel profile)
        {
            if (!string.Equals(Name, profile.Name ?? string.Empty, StringComparison.Ordinal))
                return false;

            if (!string.Equals(Description ?? string.Empty, profile.Description ?? string.Empty, StringComparison.Ordinal))
                return false;

            if (MaxPostingAgeMinutes != profile.MaxPostingAgeMinutes)
                return false;

            if (Workplace != profile.Workplace)
                return false;

            if (Experience != profile.Experience)
                return false;

            if (JobType != profile.JobType)
                return false;

            if (!string.Equals(TelegramChatId ?? string.Empty, profile.TelegramChatId ?? string.Empty, StringComparison.Ordinal))
                return false;

            if (!string.Equals(BotToken ?? string.Empty, profile.BotToken ?? string.Empty, StringComparison.Ordinal))
                return false;

            if (!CollectionsEqual(TargetJobTitles, profile.TargetJobTitles))
                return false;

            if (!CollectionsEqual(TargetLocations, profile.TargetLocations))
                return false;

            return true;
        }

        public void ApplyTo(SearchProfileViewModel profile)
        {
            profile.Name = Name;
            profile.Description = Description;
            profile.MaxPostingAgeMinutes = MaxPostingAgeMinutes;
            profile.Workplace = Workplace;
            profile.Experience = Experience;
            profile.JobType = JobType;
            profile.TelegramChatId = TelegramChatId;
            profile.BotToken = BotToken;
            profile.TargetJobTitles = [.. TargetJobTitles];
            profile.TargetLocations = [.. TargetLocations];
        }

        private static bool CollectionsEqual(List<string> a, List<string> b)
        {
            if (a.Count != b.Count) return false;
            var sortedA = a.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var sortedB = b.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            return sortedA.SequenceEqual(sortedB, StringComparer.OrdinalIgnoreCase);
        }
    }
}
