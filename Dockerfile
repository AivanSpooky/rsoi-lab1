FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PersonService.slnx ./
COPY src/PersonService/PersonService.csproj src/PersonService/
COPY tests/PersonService.Tests/PersonService.Tests.csproj tests/PersonService.Tests/
RUN dotnet restore PersonService.slnx

COPY . .
RUN dotnet publish src/PersonService/PersonService.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install --yes --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "PersonService.dll"]
