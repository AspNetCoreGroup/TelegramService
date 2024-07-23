FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TelegramService.Api/TelegramService.Api.csproj", "TelegramService.Api/"]
COPY ["TelegramService.DataAccess/TelegramService.DataAccess.csproj", "TelegramService.DataAccess/"]
COPY ["TelegramService.Domain/TelegramService.Domain.csproj", "TelegramService.Domain/"]
COPY ["TelegramService.TelegramAccess/TelegramService.TelegramAccess.csproj", "TelegramService.TelegramAccess/"]
COPY ["TelegramService.MessageBrokerAccess/TelegramService.MessageBrokerAccess.csproj", "TelegramService.MessageBrokerAccess/"]
RUN dotnet restore "TelegramService.Api/TelegramService.Api.csproj"
COPY . .
WORKDIR "/src/TelegramService.Api"
RUN dotnet build "TelegramService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramService.Api.dll"]
