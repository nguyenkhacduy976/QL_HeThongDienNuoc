# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["QL_HethongDiennuoc.csproj", "./"]
RUN dotnet restore "QL_HethongDiennuoc.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "QL_HethongDiennuoc.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "QL_HethongDiennuoc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install dependencies for QuestPDF (libfontconfig) and fonts
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fonts-liberation \
    fonts-noto \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 443

# Copy published app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "QL_HethongDiennuoc.dll"]
