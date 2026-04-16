FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./FaturamentoService.sln ./
COPY ./src ./src

RUN dotnet restore ./FaturamentoService.sln
RUN dotnet build ./src/FaturamentoService.Api/FaturamentoService.Api.csproj -c Release --no-restore
RUN dotnet publish ./src/FaturamentoService.Api/FaturamentoService.Api.csproj -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FaturamentoService.Api.dll"]
