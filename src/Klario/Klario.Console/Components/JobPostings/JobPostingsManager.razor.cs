using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Klario.Common.Enums;
using Klario.Console.Models;
using Klario.Console.Services;

namespace Klario.Console.Components.JobPostings;

public partial class JobPostingsManager : ComponentBase, IDisposable
{
    [Inject] private ConsoleState State { get; set; } = default!;
    [Inject] private KlarioApiClient ApiClient { get; set; } = default!;

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


    protected override async Task OnInitializedAsync()
    {
        State.OnChange += HandleStateChanged;
        await LoadDataAsync();
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
                search: SearchTerm
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
