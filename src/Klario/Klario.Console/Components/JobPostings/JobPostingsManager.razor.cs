using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Klario.Common.Enums;
using Klario.Console.Models;
using Klario.Console.Services;

namespace Klario.Console.Components.JobPostings;

public partial class JobPostingsManager : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private KlarioApiClient ApiClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private List<JobPostingDto> jobItems = [];
    private bool isLoading = true;
    private int currentPage = 1;
    private int pageSize = 20;
    private int totalCount = 0;
    private int totalPages = 1;
    private bool hasPreviousPage => currentPage > 1;
    private bool hasNextPage => currentPage < totalPages;

    private PostingSource? SelectedSource { get; set; }
    private string SearchTerm { get; set; } = string.Empty;
    private string SelectedBlockedFilterString { get; set; } = string.Empty;
    private bool? SelectedBlockedFilter => SelectedBlockedFilterString switch
    {
        "true" => true,
        "false" => false,
        _ => null
    };

    protected override async Task OnInitializedAsync()
    {
        State.OnChange += HandleStateChanged;
        await LoadDataAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && State.UserTimeZone == null)
        {
            try
            {
                var timeZoneId = await JS.InvokeAsync<string>("eval", "Intl.DateTimeFormat().resolvedOptions().timeZone");
                if (!string.IsNullOrWhiteSpace(timeZoneId))
                {
                    State.UserTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    StateHasChanged();
                }
            }
            catch
            {
                try
                {
                    var offsetMinutes = await JS.InvokeAsync<int>("eval", "new Date().getTimezoneOffset()");
                    State.UserTimeZone = TimeZoneInfo.CreateCustomTimeZone("BrowserLocal", TimeSpan.FromMinutes(-offsetMinutes), "BrowserLocal", "BrowserLocal");
                    StateHasChanged();
                }
                catch
                {
                }
            }
        }
    }

    private string FormatDiscoveredTime(DateTime utcTime)
    {
        var localTime = State.UserTimeZone != null
            ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), State.UserTimeZone)
            : utcTime.ToLocalTime();

        return localTime.ToString("MM-dd hh:mm tt");
    }

    private async Task LoadDataAsync()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            var pagedResponse = await ApiClient.GetJobPostingsAsync(
                State.ServerUrl,
                State.SecretKey,
                page: currentPage,
                pageSize: pageSize,
                source: SelectedSource,
                search: SearchTerm,
                isBlocked: SelectedBlockedFilter
            );

            jobItems = [.. pagedResponse.Items];
            totalCount = pagedResponse.TotalCount;
            totalPages = pagedResponse.TotalPages;

            await State.RefreshJobStatsAsync();
        }
        catch (Exception ex)
        {
            State.ShowToast($"Failed to load job postings: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleFilterChangedAsync()
    {
        currentPage = 1;
        await LoadDataAsync();
    }

    private async Task HandleSearchClickAsync()
    {
        currentPage = 1;
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            currentPage = 1;
            await LoadDataAsync();
        }
    }

    private async Task GoToPreviousPageAsync()
    {
        if (hasPreviousPage && !isLoading)
        {
            currentPage--;
            await LoadDataAsync();
        }
    }

    private async Task GoToNextPageAsync()
    {
        if (hasNextPage && !isLoading)
        {
            currentPage++;
            await LoadDataAsync();
        }
    }

    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        State.OnChange -= HandleStateChanged;
    }
}
