using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
using TelegramService.Api.Services;
using TelegramService.DataAccess;
using TelegramService.Domain;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Settings;
using TelegramService.TelegramAccess;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("TELEGRAMSERVICE_SERVER_URL") ?? 
                        config.GetConnectionString("ServiceUrl") ?? 
                        "http://localhost:5010");

// Add services to the container.
var services = builder.Services;
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    
    services
        .AddOptions<TelegramBotConfig>()
        .Bind(config.GetSection(nameof(TelegramBotConfig)))
        .Validate(x => !string.IsNullOrWhiteSpace(x.BotToken))
        .ValidateOnStart();
    services.AddHttpClient(Clients.TelegramBotClientName, client =>
    {
        client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TELEGRAMBOT_URL") ??
                                     config.GetConnectionString("TelegramBotUrl") ?? 
                                     throw new NavigationException("Need to set url to bot"));
    });
    services.AddSingleton<IConnectionMultiplexer>(opt => 
        ConnectionMultiplexer.Connect((Environment.GetEnvironmentVariable("REDISUSERDATA_CONNECTION") ?? 
                                       builder.Configuration.GetConnectionString("DockerRedisConnection")) ?? string.Empty));

    services.AddSerilog(lc => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(builder.Configuration));
}
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}
{
    services.AddScoped<ITelegramMessageSender, TelegramMessageSender>();
    services.AddScoped<IRegistrationService, RegistrationService>();
    
    services.AddDataAccess(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? 
                           config.GetConnectionString("TelegramServiceDb") ?? 
                           throw new Exception("No connection string to sql database"));
}


var app = builder.Build();

app.Services.Migrate();

{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Processing Service API v1");
            c.RoutePrefix = string.Empty;
        });
    }
    
    app.MapPost("/update", async (
            IRegistrationService registrationService,
            ITelegramMessageSender telegramMessageSender,
            [FromBody] TelegramUpdate update) =>
        {
            var messageToUser = await registrationService.TryRegister(update);
            await telegramMessageSender.SendMessageAsync(update.Message.Chat.Id, messageToUser);
            return StatusCodes.Status200OK;
        })
        .WithName("BotUpdate")
        .WithOpenApi();

    app.MapControllers();
    
    app.Run();
}
