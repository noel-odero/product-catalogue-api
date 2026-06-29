# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy solution + project files first (better layer caching)
COPY ProductCatalogue.slnx ./
COPY ProductCatalogue/ProductCatalogue.csproj ProductCatalogue/
COPY ProductCatalogue.Tests/ProductCatalogue.Tests.csproj ProductCatalogue.Tests/
RUN dotnet restore ProductCatalogue/ProductCatalogue.csproj

# copy the rest and publish the API only
COPY . .
RUN dotnet publish ProductCatalogue/ProductCatalogue.csproj \
    -c Release -o /app/publish --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT at runtime; Program.cs binds to it
ENTRYPOINT ["dotnet", "ProductCatalogue.dll"]