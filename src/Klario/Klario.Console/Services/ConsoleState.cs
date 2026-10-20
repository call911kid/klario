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
    public string? ToastActionText { get; private set; }
    public Func<Task>? ToastActionCallback { get; private set; }
    private CancellationTokenSource? _toastCts;
    private readonly Dictionary<Guid, (SearchProfileViewModel Profile, int Index, CancellationTokenSource Cts)> _pendingDeletions = new();
    private readonly Dictionary<Guid, (ExcludedCompanyModel Company, int Index, CancellationTokenSource Cts)> _pendingCompanyDeletions = new();

    public List<SearchProfileViewModel> Profiles { get; private set; } = [];
    public List<ExcludedCompanyModel> ExcludedCompanies { get; private set; } = [];
    public JobPostingStatsModel JobStats { get; private set; } = new(0, 0, 0);
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

        if (SecretKey.Any(c => c > 127))
        {
            GateError = "I've seen better attempts.";
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
            JobStats = await _apiClient.GetJobPostingStatsAsync(ServerUrl, SecretKey);

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
        catch (FormatException)
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
        SecretKey = string.Empty;
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
                }
            ],
            CreatedAt = DateTime.UtcNow
        };
        Profiles.Insert(0, fresh);
        SelectedProfile = fresh;
        NotifyStateChanged();
    }

    public async Task<bool> SaveProfileAsync(SearchProfileViewModel profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            ShowToast("Channel name required.");
            return false;
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
            return true;
        }
        catch (Exception ex)
        {
            ShowToast($"Save failed: {ex.Message}");
            return false;
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
        if (profile.Id == Guid.Empty)
        {
            Profiles.Remove(profile);
            SelectedProfile = Profiles.FirstOrDefault();
            ShowToast("Channel draft discarded.");
            NotifyStateChanged();
            return;
        }

        await CommitPendingDeletionsAsync();

        var originalIndex = Profiles.IndexOf(profile);
        Profiles.Remove(profile);
        SelectedProfile = Profiles.FirstOrDefault();
        NotifyStateChanged();

        var cts = new CancellationTokenSource();
        _pendingDeletions[profile.Id] = (profile, originalIndex, cts);
        var token = cts.Token;

        ShowToast(
            $"Channel '{profile.Name}' deleted",
            actionText: "Undo",
            actionCallback: () =>
            {
                cts.Cancel();
                _pendingDeletions.Remove(profile.Id);

                if (originalIndex >= 0 && originalIndex <= Profiles.Count)
                {
                    Profiles.Insert(originalIndex, profile);
                }
                else
                {
                    Profiles.Add(profile);
                }

                SelectedProfile = profile;
                ShowToast($"Restored '{profile.Name}'.");
                NotifyStateChanged();
                return Task.CompletedTask;
            },
            durationMs: 5000);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000, token);
                if (!token.IsCancellationRequested)
                {
                    _pendingDeletions.Remove(profile.Id);
                    await _apiClient.DeleteProfileAsync(ServerUrl, SecretKey, profile.Id);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                ShowToast($"Failed to delete {profile.Name}: {ex.Message}");
                if (!Profiles.Contains(profile))
                {
                    Profiles.Insert(Math.Min(originalIndex >= 0 ? originalIndex : 0, Profiles.Count), profile);
                    NotifyStateChanged();
                }
            }
        }, token);
    }

    public async Task CommitPendingDeletionsAsync()
    {
        if (_pendingDeletions.Count == 0) return;

        var entries = _pendingDeletions.Values.ToList();
        _pendingDeletions.Clear();

        foreach (var (p, _, cts) in entries)
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
                try
                {
                    await _apiClient.DeleteProfileAsync(ServerUrl, SecretKey, p.Id);
                }
                catch
                {
                }
            }
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
        var company = ExcludedCompanies.FirstOrDefault(c => c.Id == id);
        if (company is null) return;

        await CommitPendingCompanyDeletionsAsync();

        var originalIndex = ExcludedCompanies.IndexOf(company);
        ExcludedCompanies.Remove(company);
        NotifyStateChanged();

        var cts = new CancellationTokenSource();
        _pendingCompanyDeletions[id] = (company, originalIndex, cts);
        var token = cts.Token;

        ShowToast(
            $"Removed '{company.Name}' from exclusions",
            actionText: "Undo",
            actionCallback: () =>
            {
                cts.Cancel();
                _pendingCompanyDeletions.Remove(id);

                if (originalIndex >= 0 && originalIndex <= ExcludedCompanies.Count)
                {
                    ExcludedCompanies.Insert(originalIndex, company);
                }
                else
                {
                    ExcludedCompanies.Add(company);
                }

                ShowToast($"Restored '{company.Name}'.");
                NotifyStateChanged();
                return Task.CompletedTask;
            },
            durationMs: 5000);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000, token);
                if (!token.IsCancellationRequested)
                {
                    _pendingCompanyDeletions.Remove(id);
                    await _apiClient.DeleteExcludedCompanyAsync(ServerUrl, SecretKey, id);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                ShowToast($"Failed to remove {company.Name}: {ex.Message}");
                if (!ExcludedCompanies.Contains(company))
                {
                    ExcludedCompanies.Insert(Math.Min(originalIndex >= 0 ? originalIndex : 0, ExcludedCompanies.Count), company);
                    NotifyStateChanged();
                }
            }
        }, token);
    }

    public async Task CommitPendingCompanyDeletionsAsync()
    {
        if (_pendingCompanyDeletions.Count == 0) return;

        var entries = _pendingCompanyDeletions.Values.ToList();
        _pendingCompanyDeletions.Clear();

        foreach (var (c, _, cts) in entries)
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
                try
                {
                    await _apiClient.DeleteExcludedCompanyAsync(ServerUrl, SecretKey, c.Id);
                }
                catch
                {
                }
            }
        }
    }

    public void ShowToast(string message, string? actionText = null, Func<Task>? actionCallback = null, int durationMs = 2500)
    {
        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();
        var token = _toastCts.Token;

        ToastMessage = message;
        ToastActionText = actionText;
        ToastActionCallback = actionCallback;
        NotifyStateChanged();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(durationMs, token);
                if (!token.IsCancellationRequested)
                {
                    ToastMessage = string.Empty;
                    ToastActionText = null;
                    ToastActionCallback = null;
                    NotifyStateChanged();
                }
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    public void ClearToast()
    {
        _toastCts?.Cancel();
        _toastCts = null;
        ToastMessage = string.Empty;
        ToastActionText = null;
        ToastActionCallback = null;
        NotifyStateChanged();
    }

    public async Task RefreshJobStatsAsync()
    {
        try
        {
            JobStats = await _apiClient.GetJobPostingStatsAsync(ServerUrl, SecretKey);
            NotifyStateChanged();
        }
        catch
        {
        }
    }

    public void NotifyStateChanged() => OnChange?.Invoke();
}
