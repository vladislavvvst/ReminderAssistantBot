# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ReminderAssistantBot.sln .
COPY Directory.Packages.props .
COPY src/ReminderAssistantBot.Worker/ReminderAssistantBot.Worker.csproj src/ReminderAssistantBot.Worker/
COPY src/ReminderAssistantBot.Application/ReminderAssistantBot.Application.csproj src/ReminderAssistantBot.Application/
COPY src/ReminderAssistantBot.Bot/ReminderAssistantBot.Bot.csproj src/ReminderAssistantBot.Bot/
COPY src/ReminderAssistantBot.Infrastructure/ReminderAssistantBot.Infrastructure.csproj src/ReminderAssistantBot.Infrastructure/
COPY src/ReminderAssistantBot.Telegram/ReminderAssistantBot.Telegram.csproj src/ReminderAssistantBot.Telegram/
COPY src/ReminderAssistantBot.Domain/ReminderAssistantBot.Domain.csproj src/ReminderAssistantBot.Domain/
RUN dotnet restore src/ReminderAssistantBot.Worker/ReminderAssistantBot.Worker.csproj

COPY src/ ./src/
RUN dotnet publish src/ReminderAssistantBot.Worker/ReminderAssistantBot.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReminderAssistantBot.Worker.dll"]
