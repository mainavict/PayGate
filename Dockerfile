# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["PayGate.csproj", "./"]
RUN dotnet restore "./PayGate.csproj"
COPY . .
RUN dotnet build "./PayGate.csproj" -c Release -o /app/build

# 2. Publish Stage
FROM build AS publish
RUN dotnet publish "./PayGate.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

# Copy the published output from the publish stage
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PayGate.dll"]