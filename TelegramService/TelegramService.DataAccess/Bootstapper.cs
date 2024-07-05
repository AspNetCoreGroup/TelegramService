using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramService.DataAccess.Repositories;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Abstractions.Repositories;

namespace TelegramService.DataAccess;

public static class Bootstapper
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        return services
            .AddDbContext<DataContext>(options => options.UseNpgsql(connectionString))
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRegistrationCodeRepository, RegistrationCodeRepository>();
    }
}