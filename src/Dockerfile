FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out --arch x64

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

VOLUME /config
VOLUME /logs

EXPOSE 6767

ENTRYPOINT ["dotnet", "PotatoBot.dll"]