# ── Stage 1: Build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY RapidoLog.slnx .
COPY RapidoLog.Domain/RapidoLog.Domain.csproj RapidoLog.Domain/
COPY RapidoLog.Application/RapidoLog.Application.csproj RapidoLog.Application/
COPY RapidoLog.Infrastructure/RapidoLog.Infrastructure.csproj RapidoLog.Infrastructure/
COPY RapidoLog.Api/RapidoLog.Api.csproj RapidoLog.Api/

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore RapidoLog.Api/RapidoLog.Api.csproj

# Copy remaining source code
COPY . .

# Publish release build
RUN dotnet publish RapidoLog.Api/RapidoLog.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RapidoLog.Api.dll"]
