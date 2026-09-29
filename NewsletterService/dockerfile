FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["NewsletterService/NewsletterService.csproj", "NewsletterService/"]
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]

RUN dotnet restore "NewsletterService/NewsletterService.csproj"

COPY ["NewsletterService/", "NewsletterService/"]
COPY ["Shared/Observability/", "Shared/Observability/"]

WORKDIR "/src/NewsletterService"
RUN dotnet publish "NewsletterService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NewsletterService.dll"]