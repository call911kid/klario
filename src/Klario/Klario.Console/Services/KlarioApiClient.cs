using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Klario.Common.Enums;
using Klario.Common.Pagination;
using Klario.Console.Models;

namespace Klario.Console.Services;

public sealed class KlarioApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public KlarioApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient(string baseUrl, string apiKey)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            client.DefaultRequestHeaders.Remove("X-Api-Key");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", apiKey);
        }
        return client;
    }

    public async Task<List<SearchProfileViewModel>> GetProfilesAsync(string baseUrl, string apiKey)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.GetFromJsonAsync<ApiEnvelope<List<SearchProfileApiDto>>>("api/SearchProfiles", JsonOptions);
        
        if (response?.Data == null) return [];

        return response.Data.Select(dto => new SearchProfileViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            TargetJobTitles = dto.TargetJobTitles ?? [],
            TargetLocations = dto.TargetLocations ?? [],
            MaxPostingAgeMinutes = dto.MaxPostingAgeMinutes,
            Workplace = dto.Workplace,
            Experience = dto.Experience,
            JobType = dto.JobType,
            TelegramChatId = dto.TelegramChatId ?? string.Empty,
            BotToken = dto.BotToken ?? string.Empty,
            Schedules = (dto.Schedules ?? []).Select(s => new ProviderScheduleModel
            {
                Source = s.Source,
                Name = s.Source switch
                {
                    PostingSource.LinkedIn => "LinkedIn Jobs",
                    PostingSource.CustomCareerPage => "Custom Career Pages",
                    _ => s.Source.ToString()
                },
                Subtitle = s.Source switch
                {
                    PostingSource.LinkedIn => "Guest HTTP Ingestion",
                    PostingSource.CustomCareerPage => "Direct ATS Crawl",
                    _ => "Job Scraper"
                },
                IntervalMinutes = s.IntervalMinutes,
                IsEnabled = s.IsEnabled
            }).ToList(),
            CreatedAt = dto.CreatedAt
        }).ToList();
    }

    public async Task<Guid> CreateProfileAsync(string baseUrl, string apiKey, SearchProfileViewModel model)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new
        {
            name = model.Name,
            description = model.Description,
            targetJobTitles = model.TargetJobTitles,
            targetLocations = model.TargetLocations,
            maxPostingAgeMinutes = model.MaxPostingAgeMinutes,
            workplace = model.Workplace,
            experience = model.Experience,
            jobType = model.JobType,
            telegramChatId = model.TelegramChatId,
            botToken = model.BotToken
        };

        var response = await client.PostAsJsonAsync("api/SearchProfiles", payload, JsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiEnvelope<Guid>>(JsonOptions);
        return result?.Data ?? Guid.Empty;
    }

    public async Task UpdateProfileAsync(string baseUrl, string apiKey, SearchProfileViewModel model)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new
        {
            name = model.Name,
            description = model.Description,
            targetJobTitles = model.TargetJobTitles,
            targetLocations = model.TargetLocations,
            maxPostingAgeMinutes = model.MaxPostingAgeMinutes,
            workplace = model.Workplace,
            experience = model.Experience,
            jobType = model.JobType,
            telegramChatId = model.TelegramChatId,
            botToken = model.BotToken
        };

        var response = await client.PutAsJsonAsync($"api/SearchProfiles/{model.Id}", payload, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetProfileStatusAsync(string baseUrl, string apiKey, Guid id, bool isActive)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new { isActive };
        var response = await client.PutAsJsonAsync($"api/SearchProfiles/{id}/status", payload, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProfileAsync(string baseUrl, string apiKey, Guid id)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.DeleteAsync($"api/SearchProfiles/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ExcludedCompanyModel>> GetExcludedCompaniesAsync(string baseUrl, string apiKey)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.GetFromJsonAsync<ApiEnvelope<List<ExcludedCompanyModel>>>("api/excluded-companies", JsonOptions);
        return response?.Data ?? [];
    }

    public async Task<Guid> AddExcludedCompanyAsync(string baseUrl, string apiKey, string name)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new { name };
        var response = await client.PostAsJsonAsync("api/excluded-companies", payload, JsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiEnvelope<Guid>>(JsonOptions);
        return result?.Data ?? Guid.Empty;
    }

    public async Task DeleteExcludedCompanyAsync(string baseUrl, string apiKey, Guid id)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.DeleteAsync($"api/excluded-companies/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ConfigureScheduleAsync(string baseUrl, string apiKey, Guid searchProfileId, PostingSource source, int intervalMinutes)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new { intervalMinutes };
        var response = await client.PutAsJsonAsync($"api/search-profiles/{searchProfileId}/schedules/{source}", payload, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetScheduleStatusAsync(string baseUrl, string apiKey, Guid searchProfileId, PostingSource source, bool isEnabled)
    {
        var client = CreateClient(baseUrl, apiKey);
        var payload = new { isEnabled };
        var response = await client.PutAsJsonAsync($"api/search-profiles/{searchProfileId}/schedules/{source}/status", payload, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveScheduleAsync(string baseUrl, string apiKey, Guid searchProfileId, PostingSource source)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.DeleteAsync($"api/search-profiles/{searchProfileId}/schedules/{source}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<JobPostingStatsModel> GetJobPostingStatsAsync(string baseUrl, string apiKey)
    {
        var client = CreateClient(baseUrl, apiKey);
        var response = await client.GetFromJsonAsync<ApiEnvelope<JobPostingStatsModel>>("api/job-postings/stats", JsonOptions);
        return response?.Data ?? new JobPostingStatsModel(0, 0, 0);
    }

    public async Task<PagedResponse<JobPostingDto>> GetJobPostingsAsync(
        string baseUrl,
        string apiKey,
        int page = 1,
        int pageSize = 20,
        PostingSource? source = null,
        string? search = null)
    {
        var client = CreateClient(baseUrl, apiKey);
        var query = new List<string>
        {
            $"Page={page}",
            $"PageSize={pageSize}"
        };

        if (source.HasValue) query.Add($"source={source.Value}");
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");

        var url = $"api/job-postings?{string.Join("&", query)}";
        var response = await client.GetFromJsonAsync<ApiEnvelope<PagedResponse<JobPostingDto>>>(url, JsonOptions);
        return response?.Data ?? new PagedResponse<JobPostingDto>([], page, pageSize, 0);
    }
}
