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

COPY . /src/ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj
COPY . /src/ProjetoCantinaIBJ/CantinaIBJ.Data/CantinaIBJ.Data.csproj
COPY . /src/ProjetoCantinaIBJ/CantinaIBJ.Model/CantinaIBJ.Model.csproj
COPY . /src/ProjetoCantinaIBJ/CantinaIBJ.Framework/CantinaIBJ.Framework.csproj
COPY . /src/ProjetoCantinaIBJ/CantinaIBJ.Integration/CantinaIBJ.Integration.csproj
RUN dotnet restore "./src/ProjetoCantinaIBJ/CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj"
COPY . .
WORKDIR "/src/CantinaIBJ.WebApi"
RUN dotnet build "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinaIBJ.WebApi.dll"]