# ── Build Stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY src/NutriDash.Web/NutriDash.Web.csproj ./NutriDash.Web/
RUN dotnet restore ./NutriDash.Web/NutriDash.Web.csproj

COPY src/NutriDash.Web/ ./NutriDash.Web/
RUN dotnet publish ./NutriDash.Web/NutriDash.Web.csproj \
    -c Release -o /app/publish \
    --no-restore

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Environment defaults
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "NutriDash.Web.dll"]
