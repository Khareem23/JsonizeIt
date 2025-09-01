FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JsonizeIT.API/JsonizeIT.API.csproj", "JsonizeIT.API/"]
RUN dotnet restore "JsonizeIT.API/JsonizeIT.API.csproj"
COPY . .
WORKDIR "/src/JsonizeIT.API"
RUN dotnet build "./JsonizeIT.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JsonizeIT.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JsonizeIT.API.dll"]
