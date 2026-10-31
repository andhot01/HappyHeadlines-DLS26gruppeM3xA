FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first for dependency restore
COPY ["ProfanityService/ProfanityService.csproj", "ProfanityService/"]

RUN dotnet restore "ProfanityService/ProfanityService.csproj"

# Copy source code
COPY ["ProfanityService/", "ProfanityService/"]

# Publish
WORKDIR "/src/ProfanityService"
RUN dotnet publish "ProfanityService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProfanityService.dll"]