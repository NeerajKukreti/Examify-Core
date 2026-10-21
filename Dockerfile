# Multi-stage Dockerfile for ExamiFy (Render Zero-Cost Deployment)
# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY . .

# Restore dependencies
RUN dotnet restore Examify.sln

# Publish both API and MVC projects in Release mode
RUN dotnet publish ExamifyAPI/ExamifyApi.csproj -c Release -o /app/api --no-restore
RUN dotnet publish Examify/Examify.csproj -c Release -o /app/mvc --no-restore

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install bash & curl
RUN apt-get update && apt-get install -y --no-install-recommends bash curl && rm -rf /var/lib/apt/lists/*

# Copy published outputs from build stage
COPY --from=build /app/api /app/api
COPY --from=build /app/mvc /app/mvc

# Copy entrypoint script and normalize line endings for Linux
COPY entrypoint.sh /app/entrypoint.sh
RUN sed -i 's/\r$//' /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Expose Render default web port
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
