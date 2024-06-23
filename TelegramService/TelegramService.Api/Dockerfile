FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Monitoring.Authorization/Monitoring.Authorization.csproj", "Monitoring.Authorization/"]
RUN dotnet restore "TelegramService.Api/TelegramService.Api.csproj"
COPY . .
WORKDIR "/src/Monitoring.Authorization"
RUN dotnet build "TelegramService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramService.Api.dll"]
