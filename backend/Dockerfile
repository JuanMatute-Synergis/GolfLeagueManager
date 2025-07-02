# Build the .NET backend
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5505

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Copy RSA keys for JWT authentication
COPY --from=build /src/jwt_private_key.pem .
COPY --from=build /src/jwt_public_key.pem .
ENV ASPNETCORE_URLS=http://+:5505
ENTRYPOINT ["dotnet", "backend.dll"]
