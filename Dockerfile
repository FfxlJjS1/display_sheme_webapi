FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY display_sheme_webapi.csproj display_sheme_webapi.csproj
RUN dotnet restore
COPY . .
WORKDIR /src
RUN dotnet build display_sheme_webapi.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish display_sheme_webapi.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "display_sheme_webapi.dll" ]