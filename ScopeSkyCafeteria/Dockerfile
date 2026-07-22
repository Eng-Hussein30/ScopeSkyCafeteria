FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore ScopeSkyCafeteria.csproj

RUN dotnet build ScopeSkyCafeteria.csproj -c Release -o /app/build

RUN dotnet publish ScopeSkyCafeteria.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ScopeSkyCafeteria.dll"]

