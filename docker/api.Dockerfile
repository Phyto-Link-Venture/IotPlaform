# Multi-stage build for the IoTPlatform API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (leverage layer caching by copying project files first).
COPY IoTPlatform.slnx ./
COPY src/IoTPlatform.API/IoTPlatform.API.csproj src/IoTPlatform.API/
COPY src/IoTPlatform.Models/IoTPlatform.Models.csproj src/IoTPlatform.Models/
COPY src/IoTPlatform.Common/IoTPlatform.Common.csproj src/IoTPlatform.Common/
COPY src/IoTPlatform.Services/IoTPlatform.Services.csproj src/IoTPlatform.Services/
COPY src/IoTPlatform.Infrastructure/IoTPlatform.Infrastructure.csproj src/IoTPlatform.Infrastructure/
COPY src/IoTPlatform.Tests/IoTPlatform.Tests.csproj src/IoTPlatform.Tests/
RUN dotnet restore src/IoTPlatform.API/IoTPlatform.API.csproj

# Build + publish.
COPY . .
RUN dotnet publish src/IoTPlatform.API/IoTPlatform.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Persist the Data Protection key ring (AI API-key encryption) to a mounted volume.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "IoTPlatform.API.dll"]
