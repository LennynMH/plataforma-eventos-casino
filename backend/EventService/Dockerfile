FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8070

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["EventService.API/EventService.API.csproj", "EventService.API/"]
COPY ["EventService.Application/EventService.Application.csproj", "EventService.Application/"]
COPY ["EventService.Domain/EventService.Domain.csproj", "EventService.Domain/"]
COPY ["EventService.Infrastructure/EventService.Infrastructure.csproj", "EventService.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "EventService.API/EventService.API.csproj"

# Copiar todo el código fuente
COPY . .

# Build
WORKDIR "/src/EventService.API"
RUN dotnet build "EventService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventService.API.dll"]
