FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first for dependency restore
COPY ["CommentService/CommentService.csproj", "CommentService/"]

RUN dotnet restore "CommentService/CommentService.csproj"

# Copy source code
COPY ["CommentService/", "CommentService/"]

# Publish
WORKDIR "/src/CommentService"
RUN dotnet publish "CommentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CommentService.dll"]