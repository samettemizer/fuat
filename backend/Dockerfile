# ---- SDK ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FuatAmca.sln ./
COPY src/FuatAmca.Api/FuatAmca.Api.csproj src/FuatAmca.Api/
RUN dotnet restore src/FuatAmca.Api/FuatAmca.Api.csproj

COPY src/ src/
RUN dotnet publish src/FuatAmca.Api/FuatAmca.Api.csproj \
    -c Release \
    -o /app \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Health check icin curl; imza anahtari klasoru non-root kullaniciya devredilir
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/fuat/keys \
    && chown -R app:app /var/fuat

COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "FuatAmca.Api.dll"]
