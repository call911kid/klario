using Microsoft.AspNetCore.Components;
using Klario.Common.Enums;
using Klario.Console.Models;

namespace Klario.Console.Services;

public sealed class ConsoleState
{
    private readonly KlarioApiClient _apiClient;

    public ConsoleState(KlarioApiClient apiClient, NavigationManager? navigationManager = null)
    {
        _apiClient = apiClient;
        if (navigationManager != null && !string.IsNullOrWhiteSpace(navigationManager.BaseUri))
        {
            ServerUrl = navigationManager.BaseUri.TrimEnd('/');
        }
    }

    public bool IsUnlocked { get; private set; }
    public bool IsVerifying { get; private set; }
    public string SecretKey { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = "https://localhost:7220";
    public string GateError { get; set; } = string.Empty;
    public bool RememberKey { get; set; } = true;

    public string ActiveTab { get; set; } = "profiles";
    public string SearchFilter { get; set; } = string.Empty;
    public string ToastMessage { get; private set; } = string.Empty;

    public List<SearchProfileViewModel> Profiles { get; private set; } = [];
    public List<ExcludedCompanyModel> ExcludedCompanies { get; private set; } = [];
    public SearchProfileViewModel? SelectedProfile { get; set; }

    public IReadOnlyList<SearchProfileViewModel> FilteredProfiles =>
        string.IsNullOrWhiteSpace(SearchFilter)
            ? Profiles
            : Profiles.Where(p => p.Name.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();

    public event Action? OnChange;

    public void SetActiveTab(string tab)
    {
        ActiveTab = tab;
        NotifyStateChanged();
    }

    public void SetSearchFilter(string filter)
    {
        SearchFilter = filter;
        NotifyStateChanged();
    }

    public void SelectProfile(SearchProfileViewModel? profile)
    {
        SelectedProfile = profile;
        NotifyStateChanged();
    }

    public async Task<bool> UnlockAsync()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            GateError = "You had one job!";
            NotifyStateChanged();
            return false;
        }

        GateError = string.Empty;
        IsVerifying = true;
        NotifyStateChanged();

        try
        {
            Profiles = await _apiClient.GetProfilesAsync(ServerUrl, SecretKey);
            ExcludedCompanies = await _apiClient.GetExcludedCompaniesAsync(ServerUrl, SecretKey);

            if (Profiles.Count == 0)
            {
                CreateNewProfile();
            }
            else
            {
                SelectedProfile = Profiles.FirstOrDefault();
            }

            IsUnlocked = true;
            ShowToast("Connected to Klario.");
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            GateError = "I've seen better attempts.";
            return false;
        }
        catch (Exception ex)
        {
            GateError = "Unable to connect to API.";
            ShowToast($"Connection error: {ex.Message}");
            return false;
        }
        finally
        {
            IsVerifying = false;
            NotifyStateChanged();
        }
    }

    public void Lock()
    {
        IsUnlocked = false;
        GateError = string.Empty;
        NotifyStateChanged();
    }

    public void CreateNewProfile()
    {
        var fresh = new SearchProfileViewModel
        {
            Id = Guid.Empty,
            Name = "New Channel",
            TargetJobTitles = [".NET Architect", "Backend Lead"],
            TargetLocations = ["Remote"],
            Workplace = WorkplacePreference.Remote | WorkplacePreference.Hybrid,
            Experience = ExperienceLevel.Senior | ExperienceLevel.Lead,
            JobType = JobType.FullTime,
            MaxPostingAgeMinutes = 1440,
            IsActive = true,
            Schedules = [
                new ProviderScheduleModel
                {
                    Source = PostingSource.LinkedIn,
                    Name = "LinkedIn Jobs",
                    Subtitle = "Guest HTTP Ingestion",
                    IntervalMinutes = 15,
                    IsEnabled = true
                },
                new ProviderScheduleModel
                {
                    Source = PostingSource.CustomCareerPage,
                    Name = "Custom Career Pages",
                    Subtitle = "Direct ATS Crawl",
                    IntervalMinutes = 30,
                    IsEnabled = true
                }
            ],
            CreatedAt = DateTime.UtcNow
        };
        Profiles.Insert(0, fresh);
        SelectedProfile = fresh;
        NotifyStateChanged();
    }

    public async Task SaveProfileAsync(SearchProfileViewModel profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            ShowToast("Channel name required.");
            return;
        }

        try
        {
            if (profile.Id == Guid.Empty)
            {
                var newId = await _apiClient.CreateProfileAsync(ServerUrl, SecretKey, profile);
                profile.Id = newId;
                ShowToast($"Channel '{profile.Name}' created.");
            }
            else
            {
                await _apiClient.UpdateProfileAsync(ServerUrl, SecretKey, profile);
                ShowToast($"Channel '{profile.Name}' saved.");
            }

            if (profile.Id != Guid.Empty && profile.Schedules.Count > 0)
            {
                foreach (var schedule in profile.Schedules)
                {
                    await _apiClient.ConfigureScheduleAsync(ServerUrl, SecretKey, profile.Id, schedule.Source, schedule.IntervalMinutes);
                    await _apiClient.SetScheduleStatusAsync(ServerUrl, SecretKey, profile.Id, schedule.Source, schedule.IsEnabled);
                }
            }
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ShowToast($"Save failed: {ex.Message}");
        }
    }

    public async Task ToggleProfileStatusAsync(SearchProfileViewModel profile)
    {
        if (profile.Id == Guid.Empty) return;

        try
        {
            var next = !profile.IsActive;
            await _apiClient.SetProfileStatusAsync(ServerUrl, SecretKey, profile.Id, next);
            profile.IsActive = next;
            ShowToast($"Channel {(next ? "active" : "paused")}.");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ShowToast($"Error: {ex.Message}");
        }
    }

    public async Task DeleteProfileAsync(SearchProfileViewModel profile)
    {
        if (profile.Id == Guid.Empty) return;

        try
        {
            await _apiClient.DeleteProfileAsync(ServerUrl, SecretKey, profile.Id);
            Profiles.Remove(profile);
            SelectedProfile = Profiles.FirstOrDefault();
            ShowToast("Channel deleted.");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ShowToast($"Delete failed: {ex.Message}");
        }
    }

    public async Task AddScheduleAsync(SearchProfileViewModel profile, PostingSource source, string name, string subtitle, int defaultInterval)
    {
        if (profile.Schedules.Any(s => s.Source == source)) return;

        var newSched = new ProviderScheduleModel
        {
            Source = source,
            Name = name,
            Subtitle = subtitle,
            IntervalMinutes = defaultInterval,
            IsEnabled = true
        };
        profile.Schedules.Add(newSched);

        if (profile.Id != Guid.Empty)
        {
            try
            {
                await _apiClient.ConfigureScheduleAsync(ServerUrl, SecretKey, profile.Id, source, newSched.IntervalMinutes);
                ShowToast($"Connected {newSched.Name}.");
            }
            catch (Exception ex)
            {
                ShowToast($"Failed to save {newSched.Name}: {ex.Message}");
            }
        }
        else
        {
            ShowToast($"Connected {newSched.Name}.");
        }
        NotifyStateChanged();
    }

    public async Task RemoveScheduleAsync(SearchProfileViewModel profile, ProviderScheduleModel schedule)
    {
        if (profile.Id != Guid.Empty)
        {
            try
            {
                await _apiClient.RemoveScheduleAsync(ServerUrl, SecretKey, profile.Id, schedule.Source);
                profile.Schedules.Remove(schedule);
                ShowToast($"Disconnected {schedule.Name}.");
            }
            catch (Exception ex)
            {
                ShowToast($"Failed to disconnect: {ex.Message}");
            }
        }
        else
        {
            profile.Schedules.Remove(schedule);
            ShowToast($"Disconnected {schedule.Name}.");
        }
        NotifyStateChanged();
    }

    public async Task ToggleScheduleStatusAsync(SearchProfileViewModel profile, ProviderScheduleModel schedule)
    {
        schedule.IsEnabled = !schedule.IsEnabled;
        if (profile.Id == Guid.Empty)
        {
            NotifyStateChanged();
            return;
        }

        try
        {
            await _apiClient.SetScheduleStatusAsync(ServerUrl, SecretKey, profile.Id, schedule.Source, schedule.IsEnabled);
            ShowToast($"{schedule.Name} {(schedule.IsEnabled ? "enabled" : "paused")}.");
        }
        catch (Exception ex)
        {
            schedule.IsEnabled = !schedule.IsEnabled;
            ShowToast($"Failed to update {schedule.Name}: {ex.Message}");
        }
        NotifyStateChanged();
    }

    public async Task UpdateScheduleIntervalAsync(SearchProfileViewModel profile, ProviderScheduleModel schedule, int intervalMinutes)
    {
        schedule.IntervalMinutes = intervalMinutes;
        if (profile.Id == Guid.Empty)
        {
            NotifyStateChanged();
            return;
        }

        try
        {
            await _apiClient.ConfigureScheduleAsync(ServerUrl, SecretKey, profile.Id, schedule.Source, schedule.IntervalMinutes);
            ShowToast($"{schedule.Name} interval set to {schedule.IntervalMinutes}m.");
        }
        catch (Exception ex)
        {
            ShowToast($"Failed to update interval: {ex.Message}");
        }
        NotifyStateChanged();
    }

    public async Task AddExcludedCompanyAsync(string companyName)
    {
        var trimmed = companyName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return;

        try
        {
            var id = await _apiClient.AddExcludedCompanyAsync(ServerUrl, SecretKey, trimmed);
            ExcludedCompanies.Add(new ExcludedCompanyModel { Id = id, Name = trimmed });
            ShowToast("Company excluded.");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ShowToast($"Error: {ex.Message}");
        }
    }

    public async Task DeleteExcludedCompanyAsync(Guid id)
    {
        try
        {
            await _apiClient.DeleteExcludedCompanyAsync(ServerUrl, SecretKey, id);
            ExcludedCompanies.RemoveAll(c => c.Id == id);
            ShowToast("Company removed from exclusions.");
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ShowToast($"Error: {ex.Message}");
        }
    }

    public void ShowToast(string message)
    {
        ToastMessage = message;
        NotifyStateChanged();

        _ = Task.Run(async () =>
        {
            await Task.Delay(2500);
            ToastMessage = string.Empty;
            NotifyStateChanged();
        });
    }

    public void NotifyStateChanged() => OnChange?.Invoke();
}
