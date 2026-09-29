FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PublisherService/PublisherService.csproj", "PublisherService/"]
COPY ["Shared/Observability/Observability.csproj", "Shared/Observability/"]

RUN dotnet restore "PublisherService/PublisherService.csproj"

COPY . .

WORKDIR "/src/PublisherService"
RUN dotnet publish "PublisherService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PublisherService.dll"]