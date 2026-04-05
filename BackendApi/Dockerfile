FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Másoljuk a projektfájlt és visszaállítjuk a csomagokat
COPY ["BackendApi.csproj", "./"]
RUN dotnet restore "BackendApi.csproj"
# Másoljuk az összes többi fájlt és lefordítjuk
COPY . .
RUN dotnet publish "BackendApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BackendApi.dll"]