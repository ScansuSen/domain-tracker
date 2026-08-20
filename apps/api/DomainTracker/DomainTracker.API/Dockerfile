# Build context is the `DomainTracker/` solution folder (not this project folder alone) so the
# ProjectReference'd sibling projects (Business, DataAccess, DTOs, Core) are available to restore
# and publish. See docker-compose.yml at the repo root: `context: ./DomainTracker`.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the .csproj files first so `dotnet restore` is cached across builds unless a
# dependency actually changed.
COPY DomainTracker.Core/DomainTracker.Core.csproj DomainTracker.Core/
COPY DomainTracker.Entities/DomainTracker.Entities.csproj DomainTracker.Entities/
COPY DomainTracker.DTOs/DomainTracker.DTOs.csproj DomainTracker.DTOs/
COPY DomainTracker.DataAccess/DomainTracker.DataAccess.csproj DomainTracker.DataAccess/
COPY DomainTracker.Business/DomainTracker.Business.csproj DomainTracker.Business/
COPY DomainTracker.API/DomainTracker.API.csproj DomainTracker.API/
RUN dotnet restore DomainTracker.API/DomainTracker.API.csproj

COPY . .
RUN dotnet publish DomainTracker.API/DomainTracker.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DomainTracker.API.dll"]
