FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM base AS publish
WORKDIR /src

ARG TARGETPLATFORM

RUN echo "Building on platform: $TARGETPLATFORM"

COPY $TARGETPLATFORM/ /app/publish/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FingridDatahubLogger.dll"]