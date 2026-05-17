# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared build props and project files (enables layer-cached restore)
COPY Directory.Build.props .
COPY Backend/Backend.Api/Backend.Api.csproj             Backend/Backend.Api/
COPY Backend/Backend.Application/Backend.Application.csproj Backend/Backend.Application/
COPY Backend/Backend.Domain/Backend.Domain.csproj       Backend/Backend.Domain/
COPY Backend/Backend.Infrastructure/Backend.Infrastructure.csproj Backend/Backend.Infrastructure/

RUN dotnet restore Backend/Backend.Api/Backend.Api.csproj

# Copy remaining source and publish
COPY Backend/ Backend/

RUN dotnet publish Backend/Backend.Api/Backend.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Runtime stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Backend.Api.dll"]
