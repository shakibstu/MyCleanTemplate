#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MyCleanTemplate.API/MyCleanTemplate.API.csproj", "src/MyCleanTemplate.API/"]
COPY ["src/MyCleanTemplate.Infrastructure/MyCleanTemplate.Infrastructure.csproj", "src/MyCleanTemplate.Infrastructure/"]
COPY ["src/MyCleanTemplate.Application/MyCleanTemplate.Application.csproj", "src/MyCleanTemplate.Application/"]
COPY ["src/MyCleanTemplate.Application.Contract/MyCleanTemplate.Application.Contract.csproj", "src/MyCleanTemplate.Application.Contract/"]
COPY ["src/MyCleanTemplate.Domain/MyCleanTemplate.Domain.csproj", "src/MyCleanTemplate.Domain/"]
COPY ["src/MyCleanTemplate.SourceGenerator/MyCleanTemplate.SourceGenerator.csproj", "src/MyCleanTemplate.SourceGenerator/"]
RUN dotnet restore "./src/MyCleanTemplate.API/./MyCleanTemplate.API.csproj"
COPY . .
WORKDIR "/src/src/MyCleanTemplate.API"
RUN dotnet build "./MyCleanTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyCleanTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCleanTemplate.API.dll"]