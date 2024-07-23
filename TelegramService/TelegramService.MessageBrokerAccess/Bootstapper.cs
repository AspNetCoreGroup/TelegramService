using Microsoft.Extensions.DependencyInjection;
using TelegramService.Domain.Abstractions;

namespace TelegramService.MessageBrokerAccess;

public static class Bootstapper
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, string connectionString)
    {
        return services
            .AddScoped<IBrokerSender, RabbitMqSender>()
            .AddHostedService<RabbitMqListener>();
    }
}