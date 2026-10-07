namespace Klario.BLL.Interfaces;

public interface ITelegramService
{
    Task SendMessageAsync(
        string botToken,
        string chatId,
        string message,
        string parseMode = "HTML",
        CancellationToken cancellationToken = default);
}
