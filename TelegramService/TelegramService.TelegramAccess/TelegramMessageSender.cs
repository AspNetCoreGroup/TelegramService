using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelegramService.Domain;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Settings;

namespace TelegramService.TelegramAccess;

public class TelegramMessageSender : ITelegramMessageSender
{
    private readonly ILogger<TelegramMessageSender> _logger;
    
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    public TelegramMessageSender(
        IOptions<TelegramBotConfig> options, 
        ILogger<TelegramMessageSender> logger,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _httpClient = clientFactory.CreateClient(Clients.TelegramBotClientName);
        _botToken = options.Value.BotToken;
    }

    public async Task<bool> SendMessageAsync(long chatId, string message)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var payload = new
        {
            chat_id = chatId,
            text = message
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }
}