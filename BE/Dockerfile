# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/DocuMind.API/DocuMind.API.csproj", "src/DocuMind.API/"]
COPY ["src/DocuMind.Application/DocuMind.Application.csproj", "src/DocuMind.Application/"]
COPY ["src/DocuMind.Core/DocuMind.Core.csproj", "src/DocuMind.Core/"]
COPY ["src/DocuMind.Infrastructure/DocuMind.Infrastructure.csproj", "src/DocuMind.Infrastructure/"]

RUN dotnet restore "src/DocuMind.API/DocuMind.API.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR /src/src/DocuMind.API
RUN dotnet publish "DocuMind.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .    
ENTRYPOINT ["dotnet", "DocuMind.API.dll"]
