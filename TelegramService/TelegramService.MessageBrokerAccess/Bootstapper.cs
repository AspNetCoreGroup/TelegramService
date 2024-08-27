using Microsoft.Extensions.DependencyInjection;
using TelegramService.Domain.Abstractions;

namespace TelegramService.MessageBrokerAccess;

public static class Bootstapper
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services)
    {
        return services
            .AddScoped<IBrokerSender, RabbitMqSender>()
            .AddHostedService<RabbitMqListener>();
    }
}