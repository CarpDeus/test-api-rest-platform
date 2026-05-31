FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TestApiRestPlatform/TestApiRestPlatform.csproj TestApiRestPlatform/
RUN dotnet restore TestApiRestPlatform/TestApiRestPlatform.csproj

COPY TestApiRestPlatform/. TestApiRestPlatform/
RUN dotnet publish TestApiRestPlatform/TestApiRestPlatform.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestApiRestPlatform.dll"]
