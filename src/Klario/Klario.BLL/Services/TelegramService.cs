using System.Net.Http.Json;
using Klario.BLL.Interfaces;

namespace Klario.BLL.Services;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;

    public TelegramService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendMessageAsync(
        string botToken,
        string chatId,
        string message,
        string parseMode = "HTML",
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

        var payload = new
        {
            chat_id = chatId,
            text = message,
            parse_mode = parseMode,
            disable_web_page_preview = false
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
