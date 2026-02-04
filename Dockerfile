# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source code and build
COPY . ./
RUN dotnet publish UserManagement.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port (Render uses PORT env variable)
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "UserManagement.dll"]


