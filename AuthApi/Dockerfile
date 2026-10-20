FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AuthApi/AuthApi.csproj AuthApi/
RUN dotnet restore AuthApi/AuthApi.csproj

COPY AuthApi/ AuthApi/
RUN dotnet publish AuthApi/AuthApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthApi.dll"]
