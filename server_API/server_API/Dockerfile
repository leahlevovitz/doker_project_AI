FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY bin/Release/net7.0/publish/ .
ENTRYPOINT ["dotnet", "server_API.dll"]
