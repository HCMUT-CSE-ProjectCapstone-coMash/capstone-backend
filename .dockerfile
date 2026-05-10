FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY capstone-web-backend.sln .
COPY Capstone.Api/Capstone.Api.csproj Capstone.Api/
COPY Capstone.Application/Capstone.Application.csproj Capstone.Application/
COPY Capstone.Contracts/Capstone.Contracts.csproj Capstone.Contracts/
COPY Capstone.Domain/Capstone.Domain.csproj Capstone.Domain/
COPY Capstone.Infrastructure/Capstone.Infrastructure.csproj Capstone.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Capstone.Api/Capstone.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Capstone.Api.dll"]