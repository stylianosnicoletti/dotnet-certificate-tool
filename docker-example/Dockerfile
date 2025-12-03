FROM mcr.microsoft.com/dotnet/sdk:10.0 as build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY **/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Install certificate tool locally
RUN dotnet tool install --tool-path /app/out dotnet-certificate-tool

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./docker-entrypoint.sh .

ENTRYPOINT ["./docker-entrypoint.sh"]