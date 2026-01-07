# 1. Adicionando o bloco "base" que estava faltando (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# CORREÇÃO: Ajustei de "CantinaIBJ" (I) para "CantinaIBJ" (L) para bater com o GitHub
COPY ["CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj", "CantinaIBJ.WebApi/"]
COPY ["CantinaIBJ.Data/CantinaIBJ.Data.csproj", "CantinaIBJ.Data/"]
COPY ["CantinaIBJ.Model/CantinaIBJ.Model.csproj", "CantinaIBJ.Model/"]
COPY ["CantinaIBJ.Framework/CantinaIBJ.Framework.csproj", "CantinaIBJ.Framework/"]
COPY ["CantinaIBJ.Integration/CantinaIBJ.Integration.csproj", "CantinaIBJ.Integration/"]

# Restaura dependências
RUN dotnet restore "./CantinaIBJ.WebApi/CantinaIBJ.WebApi.csproj"

# Copia todo o resto
COPY . .
WORKDIR "/src/CantinaIBJ.WebApi"

# Build
RUN dotnet build "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CantinaIBJ.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final (Agora funciona porque o "base" foi definido lá em cima)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CantinaIBJ.WebApi.dll"]