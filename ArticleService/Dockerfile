FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for dependency restore
COPY ["ArticleService/ArticleService.csproj", "ArticleService/"]
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]

RUN dotnet restore "ArticleService/ArticleService.csproj"

# Copy source code
COPY ["ArticleService/", "ArticleService/"]
COPY ["Shared/Observability/", "Shared/Observability/"]

# Publish
WORKDIR "/src/ArticleService"
RUN dotnet publish "ArticleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ArticleService.dll"]