# TelegramService

## Для тестов

ngrok http http://localhost:5010

curl -F "url=https://your-ngrok-url.ngrok.io/api/update" https://api.telegram.org/bot{YourBotToken}/setWebhook

https://dashboard.ngrok.com/get-started/setup/windows

## Миграции

```cmd
dotnet ef migrations add MigrationName --startup-project TelegramService.Api --project TelegramService.DataAccess --context DataContext
```

```cmd
dotnet ef database update --startup-project TelegramService.Api --project TelegramService.DataAccess --context DataContext
```
