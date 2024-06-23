using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
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

// builder.Host
//     .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
//         .ReadFrom.Configuration(context.Configuration)
//         .ReadFrom.Services(services)
//         .Enrich.FromLogContext()
//         .WriteTo.Console());


// Add services to the container.
var services = builder.Services;
{
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
    services.AddScoped<IUserRepository, UserRepository>();
}


var app = builder.Build();

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

    app.MapPost("/send-message", async (ITelegramMessageSender telegramMessageSender, string message) =>
        {
            const long chatId = -1001746524212;
            var isOk = await telegramMessageSender.SendMessageAsync(chatId, message);
            return isOk ? StatusCodes.Status200OK : StatusCodes.Status502BadGateway;
        })
        .WithName("SendMessageInTelegram")
        .WithOpenApi();
    app.MapPost("/update", async (
            ILogger<TelegramMessageSender> logger,
            IUserRepository userRepository,
            ITelegramMessageSender t,
            [FromBody] TelegramUpdate update) =>
        {
            // if (update?.Message == null) 
            //     return StatusCodes.Status400BadRequest;
            
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            
            logger.LogInformation($"Received a message from chat ID: {chatId}, Message: {messageText}");
            
            var users = userRepository.GetAllUsers();
            var user = users.FirstOrDefault(user => user.ChatId == chatId.ToString());
            logger.LogInformation("User {@User}", user);
            
            if (user == default)
                userRepository.CreateUser(new User(){ ChatId = chatId.ToString(), UserId = Guid.NewGuid() });
            await t.SendMessageAsync(chatId, "Thank you for your message!");

            return StatusCodes.Status200OK;
        })
        .WithName("BotUpdate")
        .WithOpenApi();
    app.MapGet("/all-users", (
            ILogger<TelegramMessageSender> logger,
            IUserRepository userRepository) =>
        {
            var users = userRepository.GetAllUsers();
            return users;
        })
        .WithName("GetAllUsers")
        .WithOpenApi();

    app.Run();
}
