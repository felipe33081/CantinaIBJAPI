# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY . .
# --- DEBUG E CORREÇÃO AUTOMÁTICA ---
# Isso lista os arquivos no log para a gente ver (caso dê erro)
RUN ls -la
# O "|| true" faz o comando seguir sem erro se a pasta já estiver correta.
RUN mv CantinalBJ.WebApi CantinaIBJ.WebApi || true
RUN mv CantinalBJ.Data CantinaIBJ.Data || true
RUN mv CantinalBJ.Model CantinaIBJ.Model || true
RUN mv CantinalBJ.Framework CantinaIBJ.Framework || true
RUN mv CantinalBJ.Integration CantinaIBJ.Integration || true

RUN dotnet restore "CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj"

RUN dotnet build "CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinaIBJ.WebApi.dll"]