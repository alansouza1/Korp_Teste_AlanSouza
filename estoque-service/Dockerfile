FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./EstoqueService.sln ./
COPY ./src ./src

RUN dotnet restore ./EstoqueService.sln
RUN dotnet build ./src/EstoqueService.Api/EstoqueService.Api.csproj -c Release --no-restore
RUN dotnet publish ./src/EstoqueService.Api/EstoqueService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EstoqueService.Api.dll"]
