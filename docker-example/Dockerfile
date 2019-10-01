FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ConsoleApp/ConsoleApp.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Install certificate tool locally
RUN dotnet tool install --tool-path /app/ConsoleApp/out dotnet-certificate-tool

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY --from=build-env /app/ConsoleApp/out .

ENTRYPOINT ["./entrypoint.sh"]