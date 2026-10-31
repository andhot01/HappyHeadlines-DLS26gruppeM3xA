FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for dependency restore
COPY ["DraftService/DraftService.csproj", "DraftService/"]
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]

RUN dotnet restore "DraftService/DraftService.csproj"

# Copy source code
COPY ["DraftService/", "DraftService/"]
COPY ["Shared/Observability/", "Shared/Observability/"]

# Publish
WORKDIR "/src/DraftService"
RUN dotnet publish "DraftService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DraftService.dll"]